using UnityEngine;
using UnityEditor;

public static class CenterPrefabPivot
{
    [MenuItem("Tools/Props/Center Pivot To Bottom (Selected Prefab or Instance)")]
    public static void CenterPivotBottom()
    {
        var go = Selection.activeGameObject;
        if (!go) { Debug.LogWarning("Select a prefab asset in Project, or a prefab instance in Scene."); return; }

        var assetType = PrefabUtility.GetPrefabAssetType(go);
        var isAsset = assetType != PrefabAssetType.NotAPrefab &&
                      PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.NotAPrefab;

        if (isAsset)
        {
            string path = AssetDatabase.GetAssetPath(go);
            var root = PrefabUtility.LoadPrefabContents(path);
            FixPivot(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log($"Centered pivot (bottom) for prefab asset: {path}");
        }
        else
        {
            FixPivot(go);
            EditorUtility.SetDirty(go);
            var src = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (src) PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
            Debug.Log($"Centered pivot (bottom) for instance: {go.name}");
        }
    }

    private static void FixPivot(GameObject root)
    {
        // Gather renderers for bounds (fallback to colliders if no renderers)
        var rends = root.GetComponentsInChildren<Renderer>(true);
        Bounds b;
        if (rends.Length > 0)
        {
            b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        }
        else
        {
            var cols = root.GetComponentsInChildren<Collider>(true);
            if (cols.Length == 0) { Debug.LogWarning("No Renderer/Collider found under prefab, skipping."); return; }
            b = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
        }

        // The offset that moves bottom-center to world origin
        Vector3 bottomCenter = new Vector3(b.center.x, b.min.y, b.center.z);

        // Create wrapper only once
        Transform model = root.transform.Find("Model");
        if (!model)
        {
            var go = new GameObject("Model");
            model = go.transform;
            // Move all children under Model
            while (root.transform.childCount > 0)
                root.transform.GetChild(0).SetParent(model, true);
            model.SetParent(root.transform, true);
        }

        // Shift Model so that root’s origin becomes bottom-center of the mesh
        // Compute current world-space origin of the root
        Vector3 rootWorld = root.transform.position;
        // We want: (bottomCenter) -> (rootWorld)
        Vector3 delta = rootWorld - bottomCenter;
        model.position += delta;

        // Ensure root has clean transform (recommended)
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
    }
}