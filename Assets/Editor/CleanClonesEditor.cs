using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class CleanClonesEditor
{
	[MenuItem("Tools/Clean Clones/Remove VIRUS & Point Clones in Scene", priority = 2000)]
	public static void RemoveAllClonesInActiveScene()
	{
		if (!EditorUtility.DisplayDialog("Remove clone GameObjects?",
			"This will permanently remove all GameObjects in the active scene whose name ends with '(Clone)' and start with 'VIRUS' or 'Point'. Continue?",
			"Remove", "Cancel"))
			return;

		var scene = SceneManager.GetActiveScene();
		var roots = scene.GetRootGameObjects();
		var matches = new List<GameObject>();

		// traverse
		foreach (var root in roots)
			CollectMatches(root.transform, matches);

		// Perform removals with undo support
		int removed = 0;
		for (int i = 0; i < matches.Count; i++)
		{
			var go = matches[i];
			if (go == null) continue;
			Undo.DestroyObjectImmediate(go);
			removed++;
		}

		EditorUtility.DisplayDialog("Clean Clones", $"Removed {removed} GameObjects (VIRUS/Point clones) in the active scene.", "OK");
	}

	private static void CollectMatches(Transform t, List<GameObject> matches)
	{
		if (t == null) return;
		var name = t.name ?? string.Empty;

		// whitelist: only remove clones of "VIRUS" or "Point"
		// expect names like "VIRUS(Clone)" or "Point(Clone)"
		if (name.EndsWith("(Clone)") && (name.StartsWith("VIRUS") || name.StartsWith("Point") || name.StartsWith("carrot")))
			matches.Add(t.gameObject);

		for (int i = 0; i < t.childCount; i++)
			CollectMatches(t.GetChild(i), matches);
	}
}

