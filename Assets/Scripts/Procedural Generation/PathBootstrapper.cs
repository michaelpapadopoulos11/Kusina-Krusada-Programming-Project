using UnityEngine;

public class PathBootstrapper : MonoBehaviour
{
    public GameObject pathPrefab;
    public int initialToSpawn = 4;
    public float segmentLength = 40f;
    public float fixedX = -6f;
    public float startY = 0f;
    public float startZ = -10f;
    public bool matchRotation = false;

    void Start()
    {
        RemoveExistingPathPiecesByName("Path Piece");

        float nextZ = startZ;
        for (int i = 0; i < initialToSpawn; i++)
        {
            Vector3 spawnPos = new Vector3(fixedX, startY, nextZ);
            Quaternion rot = matchRotation ? transform.rotation : Quaternion.identity;

            Transform parent = GameObject.FindWithTag("PathParent")?.transform;
            GameObject seg = Instantiate(pathPrefab, spawnPos, rot, parent);
            PathManager.Instance.Register(seg);

            nextZ += segmentLength;
        }
    }
    private void RemoveExistingPathPiecesByName(string targetName)
    {
        int removed = 0;
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < all.Length; i++)
        {
            GameObject go = all[i];
            if (go == null) continue;

            if (!go.scene.IsValid()) continue;

            if (go.name == targetName)
            {
                Destroy(go);
                removed++;
            }
        }
    }
}