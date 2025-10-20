using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICamera : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 2;
    
    // Performance optimization: cache rotation calculation
    private float currentRotationY = 0f;

    void Start()
    {
        currentRotationY = transform.eulerAngles.y;
    }

    void Update()
    {
        // Use cached deltaTime and avoid creating new Vector3 every frame
        currentRotationY += rotateSpeed * PerformanceHelper.CachedDeltaTime;
        transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
    }
}
