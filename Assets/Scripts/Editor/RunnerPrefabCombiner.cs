#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

public static class RunnerPrefabCombiner
{
    // ---------- OPTIONS ----------
    const string COMBINED_ROOT_NAME = "__COMBINED_AUTO__"; // tag generated content
    const bool INCLUDE_INACTIVE = true;   // include disabled children
    const bool SKIP_SKINNED    = true;    // skip SkinnedMeshRenderer
    const bool SKIP_LOD_GROUPS = true;    // skip children under LODGroup
    // -----------------------------

    struct BucketKey
    {
        public Material mat;
        public int lightmapIndex;
        public Vector4 lightmapScaleOffset;

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 23 + (mat ? mat.GetHashCode() : 0);
                h = h * 23 + lightmapIndex;
                h = h * 23 + lightmapScaleOffset.GetHashCode();
                return h;
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is BucketKey b)) return false;
            return mat == b.mat &&
                   lightmapIndex == b.lightmapIndex &&
                   lightmapScaleOffset == b.lightmapScaleOffset;
        }
    }

    [MenuItem("Tools/Optimize/Combine Meshes (Prefab Mode, Multi-Material)")]
    public static void CombineInPrefabMode()
    {
        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null) { Debug.LogWarning("Open a prefab in Prefab Mode first."); return; }

        var root = stage.prefabContentsRoot;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.RegisterFullObjectHierarchyUndo(root, "Combine Meshes (Prefab Mode)");

        int created = CombineCore(root, stage.assetPath);
        Undo.CollapseUndoOperations(group);

        if (created > 0)
        {
            PrefabUtility.SaveAsPrefabAsset(root, stage.assetPath);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }
        Debug.Log($"[Combine] Created {created} combined renderers in {stage.assetPath}");
    }

    [MenuItem("Tools/Optimize/Combine Meshes → Save Into Prefab (Multi-Material)")]
    public static void CombineIntoPrefabAsset()
    {
        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
            {
                Debug.LogWarning($"Skip: {obj.name} is not a prefab asset."); continue;
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.RegisterFullObjectHierarchyUndo(root, "Combine Meshes Into Prefab");

            int created = 0;
            try { created = CombineCore(root, path); }
            finally
            {
                Undo.CollapseUndoOperations(group);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
                AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            }
            Debug.Log($"[Combine] Created {created} combined renderers in {path}");
        }
    }

    [MenuItem("Tools/Optimize/Revert Combined Meshes (Undo)")]
    public static void RevertCombined()
    {
        // Works in Prefab Mode or on selected prefab assets in Project.
        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
            var root = stage.prefabContentsRoot;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.RegisterFullObjectHierarchyUndo(root, "Revert Combined (Prefab Mode)");
            int removed = RevertCore(root, stage.assetPath);
            Undo.CollapseUndoOperations(group);
            PrefabUtility.SaveAsPrefabAsset(root, stage.assetPath);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log($"[Revert] Removed {removed} generated objects in {stage.assetPath}");
            return;
        }

        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab")) continue;

            var root = PrefabUtility.LoadPrefabContents(path);
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.RegisterFullObjectHierarchyUndo(root, "Revert Combined");

            int removed = 0;
            try { removed = RevertCore(root, path); }
            finally
            {
                Undo.CollapseUndoOperations(group);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
                AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            }
            Debug.Log($"[Revert] Removed {removed} generated objects in {path}");
        }
    }

    static bool IsUnderLOD(Transform t)
    {
        if (!SKIP_LOD_GROUPS) return false;
        while (t != null)
        {
            if (t.TryGetComponent<LODGroup>(out _)) return true;
            t = t.parent;
        }
        return false;
    }

    static int CombineCore(GameObject root, string prefabPath)
    {
        // ensure a clean container for new combined objects
        var ex = root.transform.Find(COMBINED_ROOT_NAME);
        if (ex != null) Undo.DestroyObjectImmediate(ex.gameObject);

        var container = new GameObject(COMBINED_ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(container, "Create Combined Container");
        container.transform.SetParent(root.transform, false);

        var allMRs = root.GetComponentsInChildren<MeshRenderer>(INCLUDE_INACTIVE)
                         .Where(mr => mr.enabled || INCLUDE_INACTIVE)
                         .ToArray();

        // build buckets
        var buckets = new Dictionary<BucketKey, List<(MeshFilter mf, MeshRenderer mr, Matrix4x4 xf, int sub)> >();
        int candidates = 0;

        var rootW2L = root.transform.worldToLocalMatrix;

        foreach (var mr in allMRs)
        {
            if (!mr.TryGetComponent<MeshFilter>(out var mf)) continue;
            if (!mf.sharedMesh) continue;
            if (SKIP_SKINNED && mr is SkinnedMeshRenderer) continue;
            if (IsUnderLOD(mr.transform)) continue;

            var mesh = mf.sharedMesh;
            var subCount = mesh.subMeshCount;
            var mats = mr.sharedMaterials;
            var slots = Mathf.Min(mats.Length, subCount);

            if (slots == 0) continue;

            for (int i = 0; i < slots; i++)
            {
                var mat = mats[i];
                if (!mat) continue;

                var key = new BucketKey
                {
                    mat = mat,
                    lightmapIndex = mr.lightmapIndex,
                    lightmapScaleOffset = mr.lightmapScaleOffset
                };

                if (!buckets.TryGetValue(key, out var list))
                    buckets[key] = list = new List<(MeshFilter, MeshRenderer, Matrix4x4, int)>();

                var xf = rootW2L * mf.transform.localToWorldMatrix;
                list.Add((mf, mr, xf, i));
                candidates++;
            }
        }

        int created = 0;

        foreach (var kv in buckets)
        {
            var key = kv.Key;
            var list = kv.Value;
            if (list.Count == 0) continue; // nothing to do

            var cis = new CombineInstance[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var (mf, _, xf, sub) = list[i];
                cis[i] = new CombineInstance
                {
                    mesh = mf.sharedMesh,
                    subMeshIndex = sub,
                    transform = xf
                };
            }

            var combined = new Mesh
            {
                name = $"Combined_{key.mat.name}_LM{key.lightmapIndex}_{created}"
            };
            combined.indexFormat = IndexFormat.UInt32;
            combined.CombineMeshes(cis, true, true, false);

            // Save as sub-asset so it persists
            AssetDatabase.AddObjectToAsset(combined, prefabPath);
            EditorUtility.SetDirty(combined);

            // Create renderer under container
            var go = new GameObject(combined.name);
            Undo.RegisterCreatedObjectUndo(go, "Create Combined Renderer");
            go.transform.SetParent(container.transform, false);

            var mfOut = go.AddComponent<MeshFilter>();   mfOut.sharedMesh = combined;
            var mrOut = go.AddComponent<MeshRenderer>(); mrOut.sharedMaterial = key.mat;
            mrOut.lightmapIndex = key.lightmapIndex;
            mrOut.lightmapScaleOffset = key.lightmapScaleOffset;

            EditorUtility.SetDirty(go);
            created++;
        }

        // Disable sources (don’t delete; safe and undoable)
        foreach (var mr in allMRs)
        {
            if (mr.transform.IsChildOf(container.transform)) continue; // skip ours
            Undo.RecordObject(mr.gameObject, "Disable Source After Combine");
            mr.gameObject.SetActive(false);
        }

        Debug.Log($"[Combine] Candidates: {candidates}, Groups: {buckets.Count}, Created: {created}");
        return created;
    }

    static int RevertCore(GameObject root, string prefabPath)
    {
        int removed = 0;

        // 1) Re-enable original sources
        var all = root.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in all)
        {
            if (mr.transform.name.StartsWith("Combined_")) continue; // generated child
            mr.gameObject.SetActive(true);
        }

        // 2) Remove our generated container + children
        var container = root.transform.Find(COMBINED_ROOT_NAME);
        if (container != null)
        {
            Undo.DestroyObjectImmediate(container.gameObject);
            removed++;
        }

        // 3) Remove generated sub-asset meshes
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(prefabPath);
        foreach (var a in subAssets)
        {
            if (a is Mesh m && m.name.StartsWith("Combined_"))
            {
                Object.DestroyImmediate(m, true);
                removed++;
            }
        }

        return removed;
    }
}
#endif
