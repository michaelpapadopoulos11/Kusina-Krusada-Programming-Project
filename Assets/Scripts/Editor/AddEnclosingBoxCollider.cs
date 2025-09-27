// Assets/Editor/AddEnclosingBoxCollider.cs
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AddEnclosingBoxCollider : EditorWindow
{
    [MenuItem("Tools/Colliders/Add Enclosing BoxCollider (Selected)")]
    private static void AddForSelectionQuick()
    {
        ProcessSelection(removeExisting: true, padding: Vector3.zero, isTrigger: false);
    }

    [MenuItem("Tools/Colliders/Add Enclosing BoxCollider (Window)...")]
    private static void OpenWindow()
    {
        var w = GetWindow<AddEnclosingBoxCollider>("Enclosing BoxCollider");
        w.minSize = new Vector2(320, 160);
        w.Show();
    }

    // --- Window UI ---
    private bool removeExisting = true;
    private bool setIsTrigger = false;
    private Vector3 padding = Vector3.zero;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Add one BoxCollider that encloses the whole object (including children).", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        removeExisting = EditorGUILayout.Toggle(new GUIContent("Remove existing colliders on root", "Removes any Collider on the root before adding"), removeExisting);
        setIsTrigger    = EditorGUILayout.Toggle(new GUIContent("Set Is Trigger on new collider", "Sets BoxCollider.isTrigger = true"), setIsTrigger);
        padding         = EditorGUILayout.Vector3Field(new GUIContent("Padding (meters)", "Extra size added on each axis"), padding);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button($"Add To {Selection.gameObjects.Length} Selected"))
            {
                ProcessSelection(removeExisting, padding, setIsTrigger);
            }
        }

        if (GUILayout.Button("Help / Notes"))
        {
            EditorUtility.DisplayDialog("Notes",
                "- Works with scene objects, prefab instances, and prefab assets.\n" +
                "- If a prefab asset is selected, it opens/saves the asset through PrefabUtility.\n" +
                "- Bounds are computed from Renderers (SkinnedMeshRenderer/MeshRenderer). If none found, falls back to Colliders.\n" +
                "- BoxCollider is added on the ROOT only; center & size are set in the root's local space.\n" +
                "- BoxCollider is axis-aligned in the root's local space (as Unity requires).",
                "OK");
        }
    }

    // --- Core ---
    private static void ProcessSelection(bool removeExisting, Vector3 padding, bool isTrigger)
    {
        var selected = Selection.objects;
        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("No selection.");
            return;
        }

        foreach (var obj in selected)
        {
            if (obj is GameObject go)
            {
                // Scene object or prefab instance
                AddColliderToRoot(go, removeExisting, padding, isTrigger, recordUndo: true);
            }
            else
            {
                // Prefab asset?
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    var root = PrefabUtility.LoadPrefabContents(path);
                    if (root != null)
                    {
                        AddColliderToRoot(root, removeExisting, padding, isTrigger, recordUndo: false);
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        PrefabUtility.UnloadPrefabContents(root);
                        Debug.Log($"Updated prefab asset: {path}");
                    }
                }
            }
        }
    }

    private static void AddColliderToRoot(GameObject root, bool removeExisting, Vector3 padding, bool isTrigger, bool recordUndo)
    {
        if (root == null) return;

        // Compute world bounds over all child renderers (incl. inactive). If none, fall back to child colliders.
        if (!TryComputeWorldBounds(root.transform, out Bounds worldBounds))
        {
            Debug.LogWarning($"[{root.name}] No Renderers or Colliders found to compute bounds.");
            return;
        }

        // Convert world bounds to root-local bounds (BoxCollider wants local center/size)
        Bounds localBounds = WorldBoundsToLocal(worldBounds, root.transform);

        // Apply padding
        localBounds.Expand(padding);

        // Remove existing colliders on root if requested
        if (removeExisting)
        {
            foreach (var col in root.GetComponents<Collider>())
            {
                if (recordUndo) Undo.DestroyObjectImmediate(col);
                else Object.DestroyImmediate(col);
            }
        }

        // Create/Reuse BoxCollider on root
        var box = root.GetComponent<BoxCollider>();
        if (box == null)
        {
            if (recordUndo) box = Undo.AddComponent<BoxCollider>(root);
            else            box = root.AddComponent<BoxCollider>();
        }

        // Set center/size in local space
        box.center   = localBounds.center;
        box.size     = localBounds.size;
        box.isTrigger= isTrigger;

        // Nice log
        Debug.Log($"[{root.name}] BoxCollider set. Center={box.center}, Size={box.size}");
    }

    // --- Bounds helpers ---
    private static bool TryComputeWorldBounds(Transform root, out Bounds worldBounds)
    {
        worldBounds = default;
        bool hasAny = false;

        // Priority: Renderers
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            // Skip editor-only or hidden
            if (!r) continue;
            var b = r.bounds; // world-space
            if (!hasAny) { worldBounds = b; hasAny = true; }
            else worldBounds.Encapsulate(b);
        }

        if (hasAny) return true;

        // Fallback: Colliders (world bounds)
        var colliders = root.GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
        {
            if (!c) continue;
            var b = c.bounds;
            if (!hasAny) { worldBounds = b; hasAny = true; }
            else worldBounds.Encapsulate(b);
        }

        return hasAny;
    }

    private static Bounds WorldBoundsToLocal(Bounds world, Transform root)
    {
        // Transform the 8 corners to local, then rebuild a local-space Bounds
        Vector3[] corners = new Vector3[8];
        Vector3 min = world.min;
        Vector3 max = world.max;

        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(max.x, min.y, min.z);
        corners[2] = new Vector3(min.x, max.y, min.z);
        corners[3] = new Vector3(max.x, max.y, min.z);
        corners[4] = new Vector3(min.x, min.y, max.z);
        corners[5] = new Vector3(max.x, min.y, max.z);
        corners[6] = new Vector3(min.x, max.y, max.z);
        corners[7] = new Vector3(max.x, max.y, max.z);

        for (int i = 0; i < 8; i++)
            corners[i] = root.InverseTransformPoint(corners[i]);

        Bounds local = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < 8; i++) local.Encapsulate(corners[i]);
        return local;
    }
}
