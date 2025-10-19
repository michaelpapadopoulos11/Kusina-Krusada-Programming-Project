using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class RuntimeCloneCleaner : MonoBehaviour
{
	[Tooltip("If true, the component will remove clone-named GameObjects when the scene starts.")]
	public bool cleanOnStart = false;

	[Tooltip("Substring to match in GameObject names (case-sensitive). Default: '(Clone)'")]
	public string suffix = "(Clone)";

	void Start()
	{
		if (cleanOnStart)
			RemoveClones();
	}

	public void RemoveClones()
	{
		var allTransforms = FindObjectsOfType<Transform>();
		var toRemove = new List<GameObject>();

		for (int i = 0; i < allTransforms.Length; i++)
		{
			var go = allTransforms[i].gameObject;
			if (go == null) continue;
			var name = go.name ?? string.Empty;
			if (!name.EndsWith(suffix)) continue;

			// whitelist base names
			if (name.StartsWith("VIRUS") || name.StartsWith("Point"))
				toRemove.Add(go);
		}

		for (int i = 0; i < toRemove.Count; i++)
		{
			var g = toRemove[i];
			if (g != null)
				Destroy(g);
		}
	}
}

