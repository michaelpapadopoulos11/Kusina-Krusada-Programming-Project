using UnityEngine;

public class PathSpawner : MonoBehaviour
{
    public GameObject pathPrefab;
    public float segmentLength = 200f;
    public float fixedX = -6f;

    [SerializeField] Transform parentRef;

    void Awake()
    {
        parentRef ??= transform.parent;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Vector3 p = parentRef.position;
        Vector3 spawnPos = new Vector3(fixedX, p.y, p.z + segmentLength);

        Transform parent = GameObject.FindWithTag("PathParent")?.transform;
        GameObject seg = Instantiate(pathPrefab, spawnPos, Quaternion.identity, parent);
        if (PathManager.Instance != null) PathManager.Instance.Register(seg);
    }
}
