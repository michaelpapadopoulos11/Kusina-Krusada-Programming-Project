using UnityEngine;
using System;
using System.Collections;

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

    // per-piece RNG
    private System.Random _rng;
    private bool _hasExternalSeed;

    private struct PrefabBounds
    {
        public GameObject prefab;
        public Vector3    localCenter;
        public Vector3    localHalf;
        public float      localBottom;
        public float      prefabYaw;
    }
    private PrefabBounds[] cache;

    private void Awake()
    {
        if (!propsRoot) propsRoot = transform;
    }

    private void OnEnable()
    {
        // If the PathManager doesn’t call SpawnWithSeed, we’ll spawn ourselves after the piece is positioned.
        StartCoroutine(SpawnWhenReady());
    }

    public void SpawnWithSeed(int seed)
    {
        if (_spawned) return;
        _rng = new System.Random(seed);
        _hasExternalSeed = true;
        TrySpawnOnce();
    }

    private IEnumerator SpawnWhenReady()
    {
        // If manager seeds us, do nothing.
        if (_hasExternalSeed || _spawned) yield break;

        // Wait a couple frames so PathManager can move this piece to its final world position.
        yield return null;
        yield return null;

        if (_spawned) yield break;

        // Seed from *final* world pose + a tiny time salt so runs differ.
        int seed = HashCode.Combine(
            transform.position.GetHashCode(),
            transform.rotation.eulerAngles.GetHashCode(),
            Environment.TickCount
        );
        _rng = new System.Random(seed);
        TrySpawnOnce();
    }

    private void TrySpawnOnce()
    {
        if (_spawned) return;
        if (!propsRoot) propsRoot = transform;

        // idempotent: don’t double-spawn on this piece
        if (propsRoot && propsRoot.childCount > 0) { _spawned = true; return; }

        if (prefabs == null || prefabs.Length == 0) { _spawned = true; return; }

        BuildBoundsCache();

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

            int idx = NextValidIndex();
            if (idx < 0) continue;

            ref var spec = ref cache[idx];

            Quaternion rot = ComputeRotation(spec.prefabYaw, isLeft);

            Vector3 pivot = new Vector3(floor.x, desiredBottom - spec.localBottom, floor.z);
            var go = Instantiate(spec.prefab, pivot, rot, propsRoot ? propsRoot : null);

            // ensure not static so runtime occlusion/batching doesn’t hide it
            go.isStatic = false;
            foreach (Transform c in go.transform) c.gameObject.isStatic = false;

            // micro Y-correction with actual bounds
            Bounds ib = GetCombinedBounds(go);
            float dy = desiredBottom - ib.min.y;
            if (Mathf.Abs(dy) > 0.0001f) go.transform.position += Vector3.up * dy;
        }
    }

    // choose uniformly using our per-piece RNG
    private int NextValidIndex()
    {
        // collect valid indices (cached once per call; small list so cheap)
        int count = 0;
        for (int i = 0; i < cache.Length; i++) if (cache[i].prefab) count++;
        if (count == 0) return -1;

        int pick = _rng.Next(0, count);
        for (int i = 0; i < cache.Length; i++)
        {
            if (!cache[i].prefab) continue;
            if (pick-- == 0) return i;
        }
        return -1;
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