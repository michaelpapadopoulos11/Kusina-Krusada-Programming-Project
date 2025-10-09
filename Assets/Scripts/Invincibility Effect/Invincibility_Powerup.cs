using UnityEngine;

public class Invincibility_Powerup : MonoBehaviour {
        void OnTriggerEnter (Collider other) {
            Movement player = other.GetComponent<Movement>();
            if (player != null) {
                player.isInvincible = true;
                player.invincibilityTimer = player.invincibilityDuration;  
                Destroy(gameObject);  // Destroys powerup after using it
        }
    }  
}