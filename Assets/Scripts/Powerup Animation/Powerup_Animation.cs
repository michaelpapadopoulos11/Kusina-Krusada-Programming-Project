using UnityEngine;

public class Spin : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public float hoverAmplitude = 0.3f;
    public float hoverFrequency = 1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        //  left - right spin animation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // hover up - down animation
        float yOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position = startPos + new Vector3(0, yOffset, 0);
    }
}
