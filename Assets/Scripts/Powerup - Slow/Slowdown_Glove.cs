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
            player.forwardSpeed = player.forwardSpeed / 2;
            player.slowTimer = slowDuration;
            Debug.Log("Player picked up slowdown glove");
            Destroy(gameObject);
        } else {
            Debug.Log("Player is no longer slowed");
            player.isSlowed = false;
            player.forwardSpeed = player.forwardSpeed * 2;
        }
    }
}
