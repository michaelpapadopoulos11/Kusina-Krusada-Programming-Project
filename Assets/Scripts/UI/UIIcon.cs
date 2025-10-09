using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIIcon : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 5;
    [SerializeField] private float rotationLimit = 15;
    
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float baseScale = 8; //Dont change
    [SerializeField] private float bobLimit = 12;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void scaleIcon()
    {
        transform.localScale += Vector3.one * bobSpeed * Time.deltaTime;
    }

    private void rotateIcon()
    {
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
    // Update is called once per frame
    void Update()
    {
        if(transform.localScale.x < baseScale) {
            bobSpeed = Mathf.Abs(bobSpeed);
        }
        else if(transform.localScale.x > bobLimit) {
            bobSpeed = -Mathf.Abs(bobSpeed);
        }

        float zRotation = transform.eulerAngles.z;

        // Convert 0–360 range to -180–180 for easier logic
        if (zRotation > 180f)
            zRotation -= 360f;

        if (zRotation < -rotationLimit)
        {
            rotateSpeed = Mathf.Abs(rotateSpeed);
        }
        else if (zRotation > rotationLimit)
        {
            rotateSpeed = -Mathf.Abs(rotateSpeed);
        }
        scaleIcon();
        rotateIcon();
    }
}
