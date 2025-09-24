using UnityEngine;

public class PathBootstrapper : MonoBehaviour
{
    public GameObject pathPrefab;
    public Transform firstPreplaced;

    [Header("Layout")]
    public int initialToSpawn = 3;
    public float segmentLength = 40f;
    public float fixedX = -5f;
    public bool matchRotation = false;

    void Start()
    {
        if (PathManager.Instance == null)
        {
            Debug.LogWarning("PathManager not found in scene.");
            return;
        }

        if (firstPreplaced) PathManager.Instance.Register(firstPreplaced.gameObject);

        Transform anchor = firstPreplaced != null ? firstPreplaced : transform;
        Vector3 lastPos = anchor.position;

        for (int i = 0; i < initialToSpawn; i++)
        {
            Vector3 spawnPos = new Vector3(fixedX, lastPos.y, lastPos.z + segmentLength);
            Quaternion rot = matchRotation ? anchor.rotation : Quaternion.identity;

            GameObject seg = Instantiate(pathPrefab, spawnPos, rot);
            PathManager.Instance.Register(seg);

            // next one goes after the one we just placed
            lastPos = seg.transform.position;
            anchor = seg.transform;
        }
    }
}
