using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }
    public int maxSegments = 8;
    private readonly Queue<GameObject> active = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(GameObject segment)
    {
        if (!segment) return;
        active.Enqueue(segment);
        if (active.Count > maxSegments)
        {
            var oldest = active.Dequeue();
            if (oldest) Destroy(oldest);
        }
    }
}
