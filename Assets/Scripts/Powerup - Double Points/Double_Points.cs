using System.Collections;
using UnityEngine;

public class Double_Points : MonoBehaviour {
    AudioManager audioManager;

    private void Awake() {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void OnTriggerEnter(Collider other) {
        Movement player = other.GetComponent<Movement>();

        if (player != null) {
            player.isDoublePoints = true; 
            player.doublePointsTimer = player.doublePointsDuration; 
            Debug.Log("Player picks up double points powerup");

            audioManager.playSFX(audioManager.powerup, 0.6f);
            Destroy(gameObject);
        }
    }
}