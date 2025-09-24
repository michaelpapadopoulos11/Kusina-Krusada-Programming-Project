using UnityEngine;
using UnityEngine.Serialization; // for FormerlySerializedAs

public class SidePropSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform middle;     // point to face (middle lane)
    [SerializeField] private Transform propsRoot;  // parent with scale 1,1,1
    [SerializeField] private Collider leftArea;    // spawn area (Mesh/Box/any Collider)
    [SerializeField] private Collider rightArea;   // spawn area (Mesh/Box/any Collider)

    [Header("Masks")]
    [SerializeField] private LayerMask groundMask; // floors only (e.g., "Ground")
    [SerializeField] private LayerMask blockMask;  // walls/fence (and optionally Props)

    [Header("Props (weighted)")]
    [Tooltip("Main list. Each entry has a weight (default 1). Old 'prefabs[]' will auto-migrate here.")]
    [SerializeField] private PropEntry[] props;

    [System.Serializable]
    public struct PropEntry
    {
        public GameObject prefab;
        [Min(0f)] public float weight; // default to 1 in OnValidate/Start if unset
    }

    // Hidden, one-time migration from legacy GameObject[] 'prefabs'
    [FormerlySerializedAs("prefabs")]
    [SerializeField, HideInInspector] private GameObject[] legacyPrefabs;

    [Header("Placement")]
    [SerializeField] private int minPerSide = 4;
    [SerializeField] private int maxPerSide = 8;
    [SerializeField] private float rayStartAbove = 3f;      // sample height above area AABB
    [SerializeField] private float yOffset = 0.02f;         // tiny hover above the floor
    [SerializeField] private float avoidRadius = 0.55f;     // fast pre-check radius vs walls/fence
    [SerializeField] private int maxAttempts = 200;         // total attempts per side
    [SerializeField] private float areaEdgePadding = 0.05f; // shrink area AABB to avoid edges

    [Header("Debug Draw")]
    [SerializeField] private bool debugDraw = true;
    [SerializeField] private float debugDuration = 2f;

    // Internal tuning
    private const float PostOverlapEpsilon = 0.005f; // small inflation so "touching" counts
    private const float UnderfootRayDown = 1.5f;     // how far we check beneath footprint
    private const float FootprintSampleLift = 0.05f; // start footprint rays slightly above bottom

#if UNITY_EDITOR
    private void OnValidate()
    {
        // One-time auto-migrate legacy prefabs -> props (weight=1)
        if ((props == null || props.Length == 0) && legacyPrefabs != null && legacyPrefabs.Length > 0)
        {
            props = new PropEntry[legacyPrefabs.Length];
            for (int i = 0; i < legacyPrefabs.Length; i++)
                props[i] = new PropEntry { prefab = legacyPrefabs[i], weight = 1f };
        }

        // Ensure any zero/negative weights default to 1
        if (props != null)
            for (int i = 0; i < props.Length; i++)
                if (props[i].prefab && props[i].weight <= 0f)
                    props[i].weight = 1f;
    }
#endif

    private void Start()
    {
        if (!propsRoot) propsRoot = transform;
        if (props == null || props.Length == 0) return;

        SpawnInArea(leftArea,  true);   // LEFT side
        SpawnInArea(rightArea, false);  // RIGHT side
    }

    private void SpawnInArea(Collider area, bool isLeft)
    {
        if (!area) return;

        int target = Random.Range(minPerSide, maxPerSide + 1);
        int placed = 0, attempts = 0;

        while (placed < target && attempts++ < maxAttempts)
        {
            // --- sample a candidate floor point inside the area ---
            Vector3 top = SampleTopPointInside(area, rayStartAbove, areaEdgePadding);

            if (debugDraw)
            {
                Debug.DrawRay(top, Vector3.up * 0.25f, Color.cyan, debugDuration);         // sample marker
                Debug.DrawRay(top, Vector3.down * 50f, new Color(1f,1f,0f,0.85f), 0.05f);  // test ray
            }

            if (!Physics.Raycast(top, Vector3.down, out RaycastHit hit, 50f, groundMask, QueryTriggerInteraction.Ignore))
            {
                if (debugDraw) Debug.DrawRay(top, Vector3.down * 50f, Color.red, debugDuration); // MISS
                continue;
            }

            Vector3 floorPoint = hit.point;
            if (debugDraw) Debug.DrawRay(floorPoint, Vector3.up * 0.4f, Color.green, debugDuration);

            // quick “keep away from fences/walls” filter
            if (avoidRadius > 0f && Physics.CheckSphere(floorPoint, avoidRadius, blockMask, QueryTriggerInteraction.Ignore))
            {
                if (debugDraw) DrawXZCross(floorPoint, avoidRadius, Color.yellow, debugDuration);
                continue;
            }

            // weighted pick
            GameObject pf = PickWeightedPrefab(props);
            if (!pf) continue;

            // --- face the player, then add prefab's own yaw, then yaw ±90° based on side ---
            Quaternion rot = ComputeSideRotationAddPrefabYaw(pf, isLeft);



            // --- try place (temp instantiate, validate, then keep or discard) ---
            GameObject temp = Instantiate(pf, floorPoint, rot);

            // compute actual bounds, align bottom to floor + yOffset
            Bounds b = GetCombinedBounds(temp);
            float desiredBottomY = floorPoint.y + Mathf.Max(0f, yOffset);
            float dy = desiredBottomY - b.min.y;
            if (Mathf.Abs(dy) > 0.0001f) temp.transform.position += Vector3.up * dy;
            b = GetCombinedBounds(temp); // refresh

            // ensure the whole footprint stays INSIDE the area bounds in XZ
            if (!FootprintInsideArea(b, area.bounds, 0.001f))
            {
                if (debugDraw) Debug.DrawRay(b.center, Vector3.up * 1.1f, new Color(1f, 0.5f, 0f), debugDuration); // outside area
                Destroy(temp); // discard ENTIRE prefab (don’t keep partials)
                continue;
            }

            // precise overlap vs walls/fence (blockMask)
            if (OverlapsBlockMask(temp, blockMask, PostOverlapEpsilon))
            {
                if (debugDraw) Debug.DrawRay(b.center, Vector3.up * 1.1f, Color.magenta, debugDuration); // blocked
                Destroy(temp);
                continue;
            }

            // re-verify ground under the footprint (center + 4 corners)
            if (!HasGroundUnderFootprint(b, groundMask))
            {
                if (debugDraw) Debug.DrawRay(b.center, Vector3.up * 1.1f, new Color(1f, 0.5f, 0f), debugDuration); // no ground
                Destroy(temp);
                continue;
            }

            // keep it: parent safely, preserve world pose
            temp.transform.SetParent(propsRoot, true);
            if (debugDraw) Debug.DrawRay(temp.transform.position, temp.transform.forward * 0.8f, Color.blue, debugDuration);

            placed++;
        }
    }

    // -------- weighted pick --------
    private static GameObject PickWeightedPrefab(PropEntry[] entries)
    {
        float total = 0f;
        for (int i = 0; i < entries.Length; i++)
            if (entries[i].prefab) total += Mathf.Max(0f, entries[i].weight <= 0f ? 1f : entries[i].weight);

        if (total > 0f)
        {
            float r = Random.value * total, acc = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                if (!entries[i].prefab) continue;
                float w = Mathf.Max(0f, entries[i].weight <= 0f ? 1f : entries[i].weight);
                acc += w;
                if (r <= acc) return entries[i].prefab;
            }
        }

        // fallback uniform among non-null
        for (int i = entries.Length - 1; i >= 0; i--)
            if (entries[i].prefab) return entries[i].prefab;
        return null;
    }

    // -------- helpers --------

    // Face the incoming player (opposite of this segment's forward), then
    // add the prefab's own Y rotation, then rotate ±90° toward the lane,
    // +90° for left side, -90° for right side.
    private Quaternion ComputeSideRotationAddPrefabYaw(GameObject prefab, bool isLeft)
    {
        // Base “face the player” direction = opposite of the segment's forward
        Vector3 facePlayer = -transform.forward;
        facePlayer.y = 0f;
        if (facePlayer.sqrMagnitude < 1e-6f) facePlayer = Vector3.forward;

        Quaternion baseFace = Quaternion.LookRotation(facePlayer, Vector3.up);

        // Prefab's own Y rotation (keep only yaw so we don’t inherit any tilt)
        float prefabYaw = prefab.transform.eulerAngles.y;
        Quaternion prefabYawQ = Quaternion.Euler(0f, prefabYaw, 0f);

        // Side adjustment: +90° on the left lane, -90° on the right lane
        Quaternion sideAdj = Quaternion.AngleAxis(isLeft ? +90f : -90f, Vector3.up);

        // Combine: face player → apply prefab yaw → rotate toward lane
        return baseFace * prefabYawQ * sideAdj;
    }



    private static Bounds GetCombinedBounds(GameObject go)
    {
        var cols = go.GetComponentsInChildren<Collider>(true);
        if (cols.Length > 0)
        {
            Bounds cb = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++) cb.Encapsulate(cols[i].bounds);
            return cb;
        }

        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length > 0)
        {
            Bounds rb = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) rb.Encapsulate(rends[i].bounds);
            return rb;
        }

        return new Bounds(go.transform.position, Vector3.zero);
    }

    private static bool OverlapsBlockMask(GameObject obj, LayerMask mask, float eps)
    {
        var cols = obj.GetComponentsInChildren<Collider>(true);
        if (cols.Length > 0)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                if (!c.enabled) continue;

                Bounds b = c.bounds;
                Vector3 halfExt = b.extents + Vector3.one * eps;
                var hits = Physics.OverlapBox(b.center, halfExt, Quaternion.identity, mask, QueryTriggerInteraction.Ignore);
                for (int h = 0; h < hits.Length; h++)
                {
                    if (hits[h].transform.IsChildOf(obj.transform)) continue; // ignore self
                    return true;
                }
            }
            return false;
        }

        Bounds rb = GetCombinedBounds(obj);
        Vector3 halfR = rb.extents + Vector3.one * eps;
        var rhits = Physics.OverlapBox(rb.center, halfR, Quaternion.identity, mask, QueryTriggerInteraction.Ignore);
        for (int h = 0; h < rhits.Length; h++)
        {
            if (rhits[h].transform.IsChildOf(obj.transform)) continue;
            return true;
        }
        return false;
    }

    private bool HasGroundUnderFootprint(Bounds b, LayerMask ground)
    {
        // center + 4 corners of XZ footprint
        Vector3 baseY = new Vector3(0f, b.min.y + FootprintSampleLift, 0f);
        Vector3 c  = new Vector3(b.center.x, 0f, b.center.z) + baseY;
        Vector3 p1 = new Vector3(b.min.x,   0f, b.min.z)   + baseY;
        Vector3 p2 = new Vector3(b.min.x,   0f, b.max.z)   + baseY;
        Vector3 p3 = new Vector3(b.max.x,   0f, b.min.z)   + baseY;
        Vector3 p4 = new Vector3(b.max.x,   0f, b.max.z)   + baseY;

        bool ok =
            RayHitsGround(c,  ground) &&
            RayHitsGround(p1, ground) &&
            RayHitsGround(p2, ground) &&
            RayHitsGround(p3, ground) &&
            RayHitsGround(p4, ground);

        return ok;
    }

    private bool RayHitsGround(Vector3 from, LayerMask ground)
    {
        if (debugDraw) Debug.DrawRay(from, Vector3.down * UnderfootRayDown, new Color(0.2f, 0.9f, 0.2f, 0.8f), debugDuration);
        return Physics.Raycast(from, Vector3.down, UnderfootRayDown, ground, QueryTriggerInteraction.Ignore);
    }

    private static bool FootprintInsideArea(Bounds prop, Bounds area, float margin)
    {
        bool insideX = prop.min.x >= (area.min.x + margin) && prop.max.x <= (area.max.x - margin);
        bool insideZ = prop.min.z >= (area.min.z + margin) && prop.max.z <= (area.max.z - margin);
        return insideX && insideZ;
    }

    private static Vector3 SampleTopPointInside(Collider area, float lift, float edgePadding)
    {
        Bounds b = area.bounds;

        float padX = Mathf.Clamp01(edgePadding) * (b.size.x * 0.5f);
        float padZ = Mathf.Clamp01(edgePadding) * (b.size.z * 0.5f);

        float minX = b.center.x - (b.extents.x - padX);
        float maxX = b.center.x + (b.extents.x - padX);
        float minZ = b.center.z - (b.extents.z - padZ);
        float maxZ = b.center.z + (b.extents.z - padZ);

        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        float y = b.max.y + lift;

        return new Vector3(x, y, z);
    }

    // Visualize a blocked spot as a small cross on the XZ plane.
    private static void DrawXZCross(Vector3 center, float radius, Color color, float duration)
    {
        Vector3 a1 = center + new Vector3(-radius, 0f, 0f);
        Vector3 a2 = center + new Vector3( radius, 0f, 0f);
        Vector3 b1 = center + new Vector3(0f, 0f, -radius);
        Vector3 b2 = center + new Vector3(0f, 0f,  radius);

        Debug.DrawLine(a1, a2, color, duration);
        Debug.DrawLine(b1, b2, color, duration);
        Debug.DrawRay(center, Vector3.up * 0.35f, color, duration);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!debugDraw) return;

        Gizmos.matrix = Matrix4x4.identity;

        if (leftArea)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.15f);
            Gizmos.DrawCube(leftArea.bounds.center, leftArea.bounds.size);
            Gizmos.color = new Color(1f, 0f, 1f, 0.9f);
            Gizmos.DrawWireCube(leftArea.bounds.center, leftArea.bounds.size);
        }
        if (rightArea)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
            Gizmos.DrawCube(rightArea.bounds.center, rightArea.bounds.size);
            Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
            Gizmos.DrawWireCube(rightArea.bounds.center, rightArea.bounds.size);
        }

        if (middle)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(middle.position, 0.08f);
        }
    }
#endif
}
