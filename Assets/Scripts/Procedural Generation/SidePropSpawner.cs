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
    [SerializeField] private Collider  leftArea;
    [SerializeField] private Collider  rightArea;

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask;

    [Header("Props")]
    [SerializeField] private GameObject[] prefabs;

    [Header("Placement")]
    [SerializeField, Min(1)] private int anchorsPerSide = 3;
    [SerializeField, Range(0f, 0.45f)] private float endMargin = 0.08f;
    [SerializeField, Range(0f, 1f)] private float leftStripT  = 0.85f;
    [SerializeField, Range(0f, 1f)] private float rightStripT = 0.15f;
    [SerializeField] private float rayStartAbove = 2f;
    [SerializeField] private float yOffset       = 0.02f;
    [SerializeField] private bool  faceMiddle    = true;

    // one-shot guard
    private bool _spawned;

    // measured bounds per prefab
    private struct PrefabBounds
    {
        public GameObject prefab;
        public Vector3    localCenter;
        public Vector3    localHalf;
        public float      localBottom;
        public float      prefabYaw;
    }
    private PrefabBounds[] cache;

    // ======== GLOBAL SHUFFLE-BAG (shared by all spawners while the game runs) ========
    private static System.Random s_rng;
    private static List<int>     s_validIndices; // all valid prefab indices
    private static List<int>     s_bag;          // current bag (shuffled order we pop from)
    private static int           s_prefabSetHash; // to detect when the prefab list changed

    private void Awake()
    {
        if (!propsRoot) propsRoot = transform;
    }

    private void OnEnable()
    {
        // spawn after the piece is positioned (and only once)
        StartCoroutine(SpawnWhenReady());
    }

    private IEnumerator SpawnWhenReady()
    {
        if (_spawned) yield break;
        // wait one frame to ensure the segment is moved/parented
        yield return null;
        TrySpawnOnce();
    }

    private void TrySpawnOnce()
    {
        if (_spawned) return;
        if (!propsRoot) propsRoot = transform;

        // idempotent: if propsRoot already has children, skip
        if (propsRoot && propsRoot.childCount > 0) { _spawned = true; return; }

        if (prefabs == null || prefabs.Length == 0) { _spawned = true; return; }

        BuildBoundsCache();
        EnsureGlobalBag(); // <-- set up the shared shuffle-bag

        PlaceOnStrip(leftArea,  isLeft: true,  leftStripT);
        PlaceOnStrip(rightArea, isLeft: false, rightStripT);

        _spawned = true;
    }

    private void BuildBoundsCache()
    {
        cache = new PrefabBounds[prefabs.Length];
        for (int i = 0; i < prefabs.Length; i++)
        {
            var pf = prefabs[i];
            if (!pf) { cache[i] = default; continue; }

            var temp = Instantiate(pf);
            temp.hideFlags = HideFlags.HideAndDontSave;
            temp.SetActive(false);
            temp.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            temp.transform.localScale = pf.transform.localScale;

            Bounds b = GetCombinedBounds(temp);
            Destroy(temp);

            cache[i] = new PrefabBounds
            {
                prefab      = pf,
                localCenter = b.center,
                localHalf   = b.extents,
                localBottom = b.min.y,
                prefabYaw   = pf.transform.eulerAngles.y
            };
        }
    }

    // ---------- GLOBAL BAG ----------
    private void EnsureGlobalBag()
    {
        // Build a signature of the current prefab set (nulls filtered)
        int hash = 17;
        unchecked
        {
            hash = hash * 31 + (prefabs?.Length ?? 0);
            if (prefabs != null)
            {
                for (int i = 0; i < prefabs.Length; i++)
                    hash = hash * 31 + (prefabs[i] ? prefabs[i].GetInstanceID() : 0);
            }
        }

        if (s_rng == null) s_rng = new System.Random(Guid.NewGuid().GetHashCode());

        // If set changed (different list/ordering/nulls), rebuild valid indices and the bag
        if (s_validIndices == null || s_prefabSetHash != hash)
        {
            s_prefabSetHash = hash;
            s_validIndices = new List<int>(prefabs.Length);
            for (int i = 0; i < prefabs.Length; i++)
                if (prefabs[i] != null) s_validIndices.Add(i);

            // start with a fresh bag
            s_bag = null;
        }

        // Refill the bag if empty or not yet created
        if (s_bag == null || s_bag.Count == 0)
        {
            s_bag = new List<int>(s_validIndices);
            // Fisher–Yates shuffle
            for (int i = s_bag.Count - 1; i > 0; i--)
            {
                int j = s_rng.Next(0, i + 1);
                (s_bag[i], s_bag[j]) = (s_bag[j], s_bag[i]);
            }
        }
    }

    // Pop one index from the global bag (refills automatically when empty)
    private int NextIndexFromGlobalBag()
    {
        if (s_bag == null || s_bag.Count == 0)
        {
            EnsureGlobalBag();
            if (s_bag == null || s_bag.Count == 0) return -1;
        }
        int last = s_bag.Count - 1;
        int idx = s_bag[last];
        s_bag.RemoveAt(last);
        return idx;
    }

    private void PlaceOnStrip(Collider area, bool isLeft, float stripT)
    {
        if (!area || cache == null || cache.Length == 0 || anchorsPerSide <= 0) return;

        Bounds b = area.bounds;
        bool alongX = b.size.x >= b.size.z;

        float minLong = alongX ? b.min.x : b.min.z;
        float maxLong = alongX ? b.max.x : b.max.z;
        float start   = Mathf.Lerp(minLong, maxLong, endMargin);
        float end     = Mathf.Lerp(minLong, maxLong, 1f - endMargin);

        float shortVal = alongX
            ? Mathf.Lerp(b.min.z, b.max.z, stripT)
            : Mathf.Lerp(b.min.x, b.max.x, stripT);

        for (int i = 0; i < anchorsPerSide; i++)
        {
            float t = anchorsPerSide == 1 ? 0.5f : (i + 0.5f) / anchorsPerSide;
            float longVal = Mathf.Lerp(start, end, t);

            Vector3 top = new Vector3(
                alongX ? longVal  : shortVal,
                b.max.y + rayStartAbove,
                alongX ? shortVal : longVal
            );

            if (!Physics.Raycast(top, Vector3.down, out RaycastHit hit, 200f, groundMask, QueryTriggerInteraction.Collide))
                continue;

            Vector3 floor = hit.point;
            float desiredBottom = floor.y + Mathf.Max(0f, yOffset);

            int idx = NextIndexFromGlobalBag();    // <- draw without replacement
            if (idx < 0 || idx >= cache.Length) continue;
            ref var spec = ref cache[idx];
            if (spec.prefab == null) continue;

            Quaternion rot = ComputeRotation(spec.prefabYaw, isLeft);

            Vector3 pivot = new Vector3(floor.x, desiredBottom - spec.localBottom, floor.z);
            var go = Instantiate(spec.prefab, pivot, rot, propsRoot ? propsRoot : null);

            // ensure dynamic (avoid occlusion culling/static batching issues)
            go.isStatic = false;
            foreach (Transform c in go.transform) c.gameObject.isStatic = false;

            // micro Y-correction with actual bounds
            Bounds ib = GetCombinedBounds(go);
            float dy = desiredBottom - ib.min.y;
            if (Mathf.Abs(dy) > 0.0001f) go.transform.position += Vector3.up * dy;
        }
    }

    private Quaternion ComputeRotation(float prefabYaw, bool isLeft)
    {
        if (!faceMiddle || middle == null)
            return Quaternion.Euler(0f, prefabYaw + (isLeft ? +90f : -90f), 0f);

        Vector3 toMid = middle.position - transform.position;
        toMid.y = 0f;
        if (toMid.sqrMagnitude < 1e-6f) toMid = transform.forward;

        Quaternion face = Quaternion.LookRotation(toMid.normalized, Vector3.up);
        Quaternion side = Quaternion.AngleAxis(isLeft ? +90f : -90f, Vector3.up);
        Quaternion pf   = Quaternion.Euler(0f, prefabYaw, 0f);
        return face * pf * side;
    }

    private static Bounds GetCombinedBounds(GameObject go)
    {
        var cols = go.GetComponentsInChildren<Collider>(true);
        if (cols.Length > 0)
        {
            Bounds b = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
            return b;
        }
        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            return b;
        }
        return new Bounds(go.transform.position, Vector3.zero);
    }
}
