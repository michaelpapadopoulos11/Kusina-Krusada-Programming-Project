using UnityEngine;

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

    [Header("Props")]
    [SerializeField] private GameObject[] prefabs; // tents, crates, etc.
    [SerializeField] private int minPerSide = 4;
    [SerializeField] private int maxPerSide = 8;

    [Header("Placement")]
    [SerializeField] private float rayStartAbove = 3f;      // how high above area bounds we start the down ray
    [SerializeField] private float yOffset = 0.02f;         // tiny hover above the floor (no contact)
    [SerializeField] private float avoidRadius = 0.55f;     // coarse pre-check radius vs walls/fence
    [SerializeField] private int maxAttempts = 200;         // safety
    [SerializeField] private float areaEdgePadding = 0.05f; // shrink bounds a touch to avoid edges

    [Header("Debug Draw")]
    [SerializeField] private bool debugDraw = true;
    [SerializeField] private float debugDuration = 2f;

    // Internal tuning (no new inspector fields)
    private const float PostOverlapEpsilon = 0.005f; // small inflation so "touching" counts
    private const float UnderfootRayDown = 1.5f;     // how far we check beneath each footprint sample
    private const float FootprintSampleLift = 0.05f; // start rays slightly above object bottom

    private void Start()
    {
        if (!propsRoot) propsRoot = transform;
        if (prefabs == null || prefabs.Length == 0) return;

        SpawnInArea(leftArea);
        SpawnInArea(rightArea);
    }

    private void SpawnInArea(Collider area)
    {
        if (!area) return;

        int toPlace = Random.Range(minPerSide, maxPerSide + 1);
        int placed = 0, attempts = 0;

        while (placed < toPlace && attempts++ < maxAttempts)
        {
            // 1) sample above the area’s (shrunk) world AABB
            Vector3 top = SampleTopPointInside(area, rayStartAbove, areaEdgePadding);

            if (debugDraw)
            {
                Debug.DrawRay(top, Vector3.up * 0.25f, Color.cyan, debugDuration);                  // sample marker
                Debug.DrawRay(top, Vector3.down * 50f, new Color(1f, 1f, 0f, 0.85f), 0.05f);        // test ray
            }

            // 2) raycast down to GROUND ONLY
            if (!Physics.Raycast(top, Vector3.down, out RaycastHit hit, 50f, groundMask, QueryTriggerInteraction.Ignore))
            {
                if (debugDraw) Debug.DrawRay(top, Vector3.down * 50f, Color.red, debugDuration);    // MISS floor
                continue;
            }

            Vector3 floorPoint = hit.point;
            if (debugDraw) Debug.DrawRay(floorPoint, Vector3.up * 0.4f, Color.green, debugDuration);

            // 3) coarse pre-check vs walls/fence (fast reject)
            if (avoidRadius > 0f && Physics.CheckSphere(floorPoint, avoidRadius, blockMask, QueryTriggerInteraction.Ignore))
            {
                if (debugDraw) DrawXZCross(floorPoint, avoidRadius, Color.yellow, debugDuration);
                continue;
            }

            // 4) choose prefab
            GameObject pf = prefabs[Random.Range(0, prefabs.Length)];
            if (!pf) continue;

            // 5) face the middle (yaw only) — no random rotation
            Quaternion rot = Quaternion.identity;
            if (middle)
            {
                Vector3 lookPoint = new Vector3(middle.position.x, floorPoint.y, middle.position.z);
                Vector3 dir = lookPoint - floorPoint; dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f) rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }

            // 6) instantiate TEMPORARILY (no parent), align bottom to floor + yOffset
            GameObject temp = Instantiate(pf, floorPoint, rot);

            // true object bounds (prefer colliders; fallback to renderers)
            Bounds b = GetCombinedBounds(temp);

            // align so bottom sits at floor + yOffset (no contact with floor)
            float desiredBottomY = floorPoint.y + Mathf.Max(0f, yOffset);
            float dy = desiredBottomY - b.min.y;
            if (Mathf.Abs(dy) > 0.0001f) temp.transform.position += Vector3.up * dy;

            // recompute bounds after move
            b = GetCombinedBounds(temp);

            // 7) **post-placement** strict checks

            // 7a) precise overlap vs walls/fence
            if (OverlapsBlockMask(temp, blockMask, PostOverlapEpsilon))
            {
                if (debugDraw) Debug.DrawRay(b.center, Vector3.up * 1.1f, Color.magenta, debugDuration); // rejected: blocked
                Destroy(temp);
                continue;
            }

            // 7b) verify there is **ground** directly beneath the prop’s footprint (center + 4 corners)
            if (!HasGroundUnderFootprint(b, groundMask))
            {
                if (debugDraw) Debug.DrawRay(b.center, Vector3.up * 1.1f, new Color(1f, 0.5f, 0f), debugDuration); // rejected: not over ground
                Destroy(temp);
                continue;
            }

            // 8) valid — parent safely (preserve world pose)
            temp.transform.SetParent(propsRoot, true);

            if (debugDraw) Debug.DrawRay(temp.transform.position, temp.transform.forward * 0.8f, Color.blue, debugDuration);
            placed++;
        }
    }

    // --- helpers ---

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
        // check each child collider's world AABB; fallback to combined render bounds
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

        // no colliders: use combined render bounds
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
        // sample center + 4 corners of the XZ footprint
        Vector3 c = new Vector3(b.center.x, b.min.y + FootprintSampleLift, b.center.z);
        Vector3 p1 = new Vector3(b.min.x, c.y, b.min.z);
        Vector3 p2 = new Vector3(b.min.x, c.y, b.max.z);
        Vector3 p3 = new Vector3(b.max.x, c.y, b.min.z);
        Vector3 p4 = new Vector3(b.max.x, c.y, b.max.z);

        // every sample must hit Ground within UnderfootRayDown
        bool ok =
            RayHitsGround(c, ground) &&
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

    private static Vector3 SampleTopPointInside(Collider area, float lift, float edgePadding)
    {
        Bounds b = area.bounds;

        // shrink XZ by padding %
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

    private static void DrawXZCross(Vector3 center, float radius, Color color, float duration)
    {
        Vector3 a1 = center + new Vector3(-radius, 0f, 0f);
        Vector3 a2 = center + new Vector3( radius, 0f, 0f);
        Vector3 b1 = center + new Vector3(0f, 0f, -radius);
        Vector3 b2 = center + new Vector3(0f, 0f,  radius);

        Debug.DrawLine(a1, a2, color, duration);
        Debug.DrawLine(b1, b2, color, duration);
        Debug.DrawRay(center, Vector3.up * 0.3f, color, duration);
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
