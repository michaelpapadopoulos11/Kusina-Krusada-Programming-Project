using UnityEngine;
using UnityEditor;

public static class AddMeshColliders
{
    [MenuItem("Tools/Add Mesh Colliders To Selected (and Save Prefab)")]
    private static void AddColliders()
    {
        var go = Selection.activeGameObject;
        if (!go)
        {
            Debug.LogWarning("Select a prefab (in Project) or a prefab instance (in Scene).");
            return;
        }

        // Are we selecting a prefab asset or an instance?
        var prefabType = PrefabUtility.GetPrefabAssetType(go);
        var instanceStatus = PrefabUtility.GetPrefabInstanceStatus(go);

        if (prefabType != PrefabAssetType.NotAPrefab && instanceStatus == PrefabInstanceStatus.NotAPrefab)
        {
            // PROJECT VIEW: selected is a prefab asset
            string path = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Could not resolve prefab asset path.");
                return;
            }

            // Load, modify, save, unload
            var root = PrefabUtility.LoadPrefabContents(path);
            int added = AddMeshCollidersUnder(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);

            Debug.Log($"[Prefab Asset] Added {added} MeshColliders to '{path}'.");
        }
        else
        {
            // SCENE: selected is a prefab instance (or a normal GO)
            int added = AddMeshCollidersUnder(go);
            EditorUtility.SetDirty(go);

            // If it’s a prefab instance, apply changes back to the prefab
            var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (source && PrefabUtility.GetPrefabInstanceStatus(go) != PrefabInstanceStatus.NotAPrefab)
            {
                PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
                Debug.Log($"[Prefab Instance] Added {added} MeshColliders and applied to prefab '{source.name}'.");
            }
            else
            {
                Debug.Log($"[Scene Object] Added {added} MeshColliders under '{go.name}'.");
            }
        }
    }

    private static int AddMeshCollidersUnder(GameObject root)
    {
        int count = 0;
        // Add to every MeshFilter child that doesn't already have a collider
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!mf.sharedMesh) continue;
            if (!mf.GetComponent<MeshCollider>())
            {
                var mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;   // important so it sticks after reload
                mc.convex = false;               // floors/props usually static
                count++;
            }
        }
        return count;
    }
}
