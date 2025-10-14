using UnityEngine;

public class Invincibility_Powerup : MonoBehaviour {

public bool invincibilityActive = false;
public float invincibilityDuration = 5f;

    void Start() {
        invincibilityActive = false;
    }

    void OnTriggerEnter (Collider other) {
        Movement player = other.GetComponent<Movement>();
        if (player != null) {
            // Give control of the invincibility to the player script
            player.isInvincible = true;
            player.invincibilityTimer = invincibilityDuration;
            Debug.Log("Player picked up invincibility");
            Destroy(gameObject);
        } else {
                Debug.Log("Player is no longer invincibile");
        }
    }
}