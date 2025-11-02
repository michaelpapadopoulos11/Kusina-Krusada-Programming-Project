using UnityEngine;

public class Invincibility_Powerup : MonoBehaviour {

AudioManager audioManager;

public static bool invincibilityActive = false;
public float invincibilityDuration = 5f;

    private void Awake() {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start() {
        invincibilityActive = false;
    }

    void OnTriggerEnter (Collider other) {
        Movement player = other.GetComponent<Movement>();
        
        if (player != null ) {
            player.isInvincible = true;
            audioManager.playSFX(audioManager.powerup, 0.6f);
            invincibilityActive = true;
            Movement.invincibilityTimer = invincibilityDuration;
            Debug.Log("Player picked up invincibility");
            Destroy(gameObject);
        }
    }
}