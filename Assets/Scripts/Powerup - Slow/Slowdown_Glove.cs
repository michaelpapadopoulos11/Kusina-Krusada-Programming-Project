using UnityEngine;

public class Slowdown_Glove : MonoBehaviour {

public bool slowActive = false;
public float slowDuration = 5f;

    void Start() {
        slowActive = false;
    }

    void OnTriggerEnter (Collider other) {
        Movement player = other.GetComponent<Movement>();
        if (player != null) {
            player.isSlowed = true;
            player.slowTimer = slowDuration;
            Debug.Log("Player picked up slowdown glove");
            Destroy(gameObject);
        }
    }
}
