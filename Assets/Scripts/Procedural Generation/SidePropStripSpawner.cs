using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class SidePropStripSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform middle;
    [SerializeField] private Transform propsRoot;
    [SerializeField] private Collider leftArea;
    [SerializeField] private Collider rightArea;

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask;

    [Header("Props")]
    [Tooltip("All normal/small props")]
    [SerializeField] private GameObject[] smallPrefabs;

    [Tooltip("Big props that should take the entire side by themselves")]
    [SerializeField] private GameObject[] bigPrefabs;

    [Header("Placement")]
    [SerializeField, Min(1)] private int anchorsPerSide = 3;
    [SerializeField, Range(0f, 0.45f)] private float endMargin = 0.08f;
    [SerializeField, Range(0f, 1f)]    private float leftStripT  = 0.85f;
    [SerializeField, Range(0f, 1f)]    private float rightStripT = 0.15f;
    [SerializeField] private float rayStartAbove = 2f;
    [SerializeField] private float yOffset       = 0.02f;
    [SerializeField] private bool  faceMiddle    = true;

    [Header("Big Prefab Offset")]
    [SerializeField] private float bigSideOffset = 4.4f;

    private bool _spawned;

    // ---------- GLOBAL CYCLE MEMORY ----------
    // Tracks which prefabs we've ALREADY spawned in the current global cycle.
    // Once we've used *every* prefab at least once, we clear and start fresh.
    private static HashSet<int> s_usedThisCycle = new HashSet<int>();

    // ---------- DATA STRUCTS ----------
    private struct PrefabInfo
    {
        public GameObject prefab;
        public int        prefabId;     // prefab.GetInstanceID()
        public Vector3    localCenter;  // combined bounds center at identity
        public float      localBottom;  // combined bounds min.y at identity
        public Vector3    prefabEuler;  // original rotation
        public bool       isBig;        // true if from bigPrefabs
    }

    private struct Anchor
    {
        public Vector3 rayStart;
    }

    private void Awake()
    {
        if (!propsRoot) propsRoot = transform;
    }

    private void OnEnable()
    {
        _spawned = false;
        StartCoroutine(SpawnWhenReady());
    }

    private IEnumerator SpawnWhenReady()
    {
        if (_spawned) yield break;
        // wait a frame so the segment is positioned in the world
        yield return null;
        TrySpawnOnce();
    }

    private void TrySpawnOnce()
    {
        if (_spawned) return;
        if (!propsRoot) propsRoot = transform;

        // don't double-populate if children already exist (baked/manual)
        if (propsRoot.childCount > 0)
        {
            _spawned = true;
            return;
        }

        // 1. Build a list of ALL prefabs (small+big) with their bounds
        List<PrefabInfo> fullDeck = BuildFullDeck();

        if (fullDeck.Count == 0)
        {
            _spawned = true;
            return;
        }

        // 2. Filter that deck to only prefabs NOT used yet in this global cycle
        List<PrefabInfo> available = FilterForUnused(fullDeck);

        // 3. If nothing's available, it means we already used ALL prefabs this cycle.
        //    => reset cycle memory, and EVERYTHING becomes available again.
        if (available.Count == 0)
        {
            s_usedThisCycle.Clear();
            available = new List<PrefabInfo>(fullDeck);
        }

        // 4. Shuffle available so this path segment gets a random pick
        Shuffle(available);

        // 5. Precompute anchor rays for both sides
        List<Anchor> leftAnchors  = BuildAnchors(leftArea,  anchorsPerSide, leftStripT);
        List<Anchor> rightAnchors = BuildAnchors(rightArea, anchorsPerSide, rightStripT);

        // 6. Fill LEFT using available list, consuming whatever we actually spawn
        ConsumeAndFillSide(
            area: leftArea,
            anchors: leftAnchors,
            isLeft: true,
            pool: available // we will REMOVE PrefabInfos that we actually spawn
        );

        // 7. Fill RIGHT using whatever is still left in 'available' after left has consumed
        ConsumeAndFillSide(
            area: rightArea,
            anchors: rightAnchors,
            isLeft: false,
            pool: available
        );

        _spawned = true;
    }

    // ==========================================================
    // Build full list of PrefabInfo for ALL prefabs you exposed
    // ==========================================================
    private List<PrefabInfo> BuildFullDeck()
    {
        var list = new List<PrefabInfo>();

        // small prefabs
        if (smallPrefabs != null)
        {
            for (int i = 0; i < smallPrefabs.Length; i++)
            {
                var pf = smallPrefabs[i];
                if (!pf) continue;

                list.Add(BuildInfoForPrefab(pf, isBig:false));
            }
        }

        // big prefabs
        if (bigPrefabs != null)
        {
            for (int i = 0; i < bigPrefabs.Length; i++)
            {
                var pf = bigPrefabs[i];
                if (!pf) continue;

                list.Add(BuildInfoForPrefab(pf, isBig:true));
            }
        }

        return list;
    }

    private PrefabInfo BuildInfoForPrefab(GameObject pf, bool isBig)
    {
        // We clone temporarily to measure final world-ish bounds
        GameObject temp = Instantiate(pf);
        temp.hideFlags = HideFlags.HideAndDontSave;
        temp.SetActive(false);
        temp.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        temp.transform.localScale = pf.transform.localScale;

        Bounds b = GetCombinedBounds(temp);
        DestroyImmediate(temp);

        PrefabInfo info;
        info.prefab      = pf;
        info.prefabId    = pf.GetInstanceID();
        info.localCenter = b.center;
        info.localBottom = b.min.y;
        info.prefabEuler = pf.transform.eulerAngles;
        info.isBig       = isBig;
        return info;
    }

    // ==========================================================
    // Take only prefabs we haven't spawned in this global "cycle"
    // ==========================================================
    private List<PrefabInfo> FilterForUnused(List<PrefabInfo> fullDeck)
    {
        var available = new List<PrefabInfo>(fullDeck.Count);
        for (int i = 0; i < fullDeck.Count; i++)
        {
            if (!s_usedThisCycle.Contains(fullDeck[i].prefabId))
            {
                available.Add(fullDeck[i]);
            }
        }
        return available;
    }

    // ==========================================================
    // Shuffle helper
    // ==========================================================
    private void Shuffle(List<PrefabInfo> list)
    {
        var rng = new System.Random(Guid.NewGuid().GetHashCode());
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ==========================================================
    // Build anchors for a given side
    // ==========================================================
    private List<Anchor> BuildAnchors(Collider area, int count, float stripT)
    {
        var anchors = new List<Anchor>();
        if (!area || count <= 0) return anchors;

        Bounds b = area.bounds;
        bool alongX = b.size.x >= b.size.z; // which axis is longer

        float minLong = alongX ? b.min.x : b.min.z;
        float maxLong = alongX ? b.max.x : b.max.z;

        float start = Mathf.Lerp(minLong, maxLong, endMargin);
        float end   = Mathf.Lerp(minLong, maxLong, 1f - endMargin);

        float shortVal = alongX
            ? Mathf.Lerp(b.min.z, b.max.z, stripT)
            : Mathf.Lerp(b.min.x, b.max.x, stripT);

        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0.5f : (i + 0.5f) / count;
            float longVal = Mathf.Lerp(start, end, t);

            Vector3 rayStart = new Vector3(
                alongX ? longVal  : shortVal,
                b.max.y + rayStartAbove,
                alongX ? shortVal : longVal
            );

            anchors.Add(new Anchor { rayStart = rayStart });
        }

        return anchors;
    }

    // ==========================================================
    // ConsumeAndFillSide:
    //
    // - pool: the shared "available" list that hasn't been used yet
    //   in THIS path's spawn. We'll also remove what we use from this pool.
    //
    // STEPS:
    // 1. Take first from pool.
    // 2. If big → spawn once in center, mark it used globally, stop for this side.
    // 3. If small → build a list of unique small prefabs for this side by
    //    pulling more SMALLS from pool until anchorsPerSide or pool empty.
    //    Spawn those 1:1 into anchors. Mark each as used globally.
    // ==========================================================
    private void ConsumeAndFillSide(
        Collider area,
        List<Anchor> anchors,
        bool isLeft,
        List<PrefabInfo> pool
    )
    {
        if (!area) return;
        if (pool == null || pool.Count == 0) return;

        // pull first candidate
        PrefabInfo first = pool[pool.Count - 1];
        pool.RemoveAt(pool.Count - 1);

        if (first.isBig)
        {
            // place big prefab (takes whole side)
            SpawnBigCentered(area, isLeft, first);

            // mark it used for the global cycle
            s_usedThisCycle.Add(first.prefabId);
            return;
        }

        // else it's small:
        // gather UNIQUE small prefabs for this side INCLUDING 'first'
        // from the pool, consuming them as we go
        List<PrefabInfo> sideList = new List<PrefabInfo>();
        sideList.Add(first);
        s_usedThisCycle.Add(first.prefabId);

        for (int i = pool.Count - 1; i >= 0 && sideList.Count < anchorsPerSide; i--)
        {
            if (!pool[i].isBig)
            {
                PrefabInfo nextSmall = pool[i];
                // consume it from pool so right side can't reuse it this path
                pool.RemoveAt(i);

                // add to side list
                sideList.Add(nextSmall);

                // mark used globally
                s_usedThisCycle.Add(nextSmall.prefabId);
            }
        }

        // Now spawn each of those small prefabs into anchors 1:1
        int countToSpawn = Mathf.Min(anchors.Count, sideList.Count);
        for (int a = 0; a < countToSpawn; a++)
        {
            PrefabInfo infoForAnchor = sideList[a];

            // raycast down for that anchor
            if (!Physics.Raycast(anchors[a].rayStart, Vector3.down, out RaycastHit hit, 200f, groundMask, QueryTriggerInteraction.Collide))
                continue;

            Quaternion rot = ComputeRotation(infoForAnchor.prefabEuler, isLeft);

            // align visual center with hit point
            Vector3 rotatedLocalCenter = rot * infoForAnchor.localCenter;
            Vector3 spawnPos = hit.point - rotatedLocalCenter;

            // smalls don't get sideways offset
            SpawnAndSnap(infoForAnchor, spawnPos, rot, hit.point, applyOffset:false, isLeft:isLeft);
        }
    }

    // ==========================================================
    // Big prefab spawn in middle of the collider, with side offset
    // ==========================================================
    private void SpawnBigCentered(Collider area, bool isLeft, PrefabInfo info)
    {
        if (!info.prefab) return;

        Bounds b = area.bounds;

        // raycast from physical center of this side area
        Vector3 rayStart = new Vector3(
            b.center.x,
            b.max.y + rayStartAbove,
            b.center.z
        );

        if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f, groundMask, QueryTriggerInteraction.Collide))
            return;

        Quaternion rot = ComputeRotation(info.prefabEuler, isLeft);

        Vector3 rotatedLocalCenter = rot * info.localCenter;
        Vector3 spawnPos = hit.point - rotatedLocalCenter;

        // push it outward from the road
        if (isLeft)
            spawnPos.x += bigSideOffset;
        else
            spawnPos.x -= bigSideOffset;

        SpawnAndSnap(info, spawnPos, rot, hit.point, applyOffset:false, isLeft:isLeft);

        // mark used globally (already done in caller, but harmless if also done here)
        s_usedThisCycle.Add(info.prefabId);
    }

    // ==========================================================
    // Actually instantiate + vertical snap
    // ==========================================================
    private void SpawnAndSnap(
        PrefabInfo info,
        Vector3 spawnPos,
        Quaternion rot,
        Vector3 groundPoint,
        bool applyOffset,
        bool isLeft
    )
    {
        if (!info.prefab) return;

        GameObject go = Instantiate(
            info.prefab,
            spawnPos,
            rot,
            propsRoot ? propsRoot : null
        );

        // runtime props shouldn't be static
        go.isStatic = false;
        foreach (Transform c in go.transform)
            c.gameObject.isStatic = false;

        // snap vertically so bottom touches ground + yOffset
        Bounds ib = GetCombinedBounds(go);
        float desiredBottomY = groundPoint.y + Mathf.Max(0f, yOffset);
        float dy = desiredBottomY - ib.min.y;
        if (Mathf.Abs(dy) > 0.0001f)
        {
            go.transform.position += Vector3.up * dy;
        }
    }

    // ==========================================================
    // Rotation helper
    // keep prefab's original X/Z tilt, solve Y so it faces across street
    // ==========================================================
    private Quaternion ComputeRotation(Vector3 prefabEuler, bool isLeft)
    {
        float baseX = prefabEuler.x;
        float baseZ = prefabEuler.z;

        float facingY;
        if (!faceMiddle || middle == null)
        {
            // simple sideways relative to road
            facingY = prefabEuler.y + (isLeft ? +90f : -90f);
        }
        else
        {
            // look at 'middle', then rotate ±90°, then apply prefab yaw
            Vector3 toMid = middle.position - transform.position;
            toMid.y = 0f;
            if (toMid.sqrMagnitude < 1e-6f)
                toMid = transform.forward;

            Quaternion face = Quaternion.LookRotation(toMid.normalized, Vector3.up);
            float midY  = face.eulerAngles.y;
            float sideY = midY + (isLeft ? +90f : -90f);

            facingY = sideY + prefabEuler.y;
        }

        return Quaternion.Euler(baseX, facingY, baseZ);
    }

    // ==========================================================
    // GetCombinedBounds
    // grabs the total bounds of the object based on colliders first,
    // then renderers, so we can line it up with the ground
    // ==========================================================
    private static Bounds GetCombinedBounds(GameObject go)
    {
        var cols = go.GetComponentsInChildren<Collider>(true);
        if (cols.Length > 0)
        {
            Bounds b = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++)
                b.Encapsulate(cols[i].bounds);
            return b;
        }

        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);
            return b;
        }

        return new Bounds(go.transform.position, Vector3.zero);
    }
}
