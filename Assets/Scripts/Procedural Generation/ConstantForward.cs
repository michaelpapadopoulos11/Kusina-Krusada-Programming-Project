using UnityEngine;

public class ConstantForward : MonoBehaviour
{
    public float forwardSpeed = 5f;

    void Update()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);
    }
}
