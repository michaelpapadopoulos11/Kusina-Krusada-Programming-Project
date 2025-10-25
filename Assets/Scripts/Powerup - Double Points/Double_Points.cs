using System.Collections;
using UnityEngine;

public class Double_Points : MonoBehaviour {
    public bool doublePointsActive = false;
    public float doublePointsDuration = 5f;

    private float timer = 0f; // Timer for the powerup duration
    AudioManager audioManager;

    private void Awake() {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start() {
        doublePointsActive = false;
        timer = 0f; // timer starts at 0
    }

    void Update() {
        if (doublePointsActive) {
            timer -= Time.deltaTime;
            if (timer <= 0f) {
                doublePointsActive = false;
                timer = 0f; // Reset the timer
                Movement player = FindObjectOfType<Movement>();
                if (player != null) {
                    player.pointsMultiplier = 1; // Revert to normal points collection
                    Debug.Log("Double points powerup expired");
                }
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        Movement player = other.GetComponent<Movement>();

        if (player != null) {
            doublePointsActive = true;
            timer = doublePointsDuration; // Start the timer
            Debug.Log("Player picks up double points powerup");

            player.pointsMultiplier = 2; // x2 fruit points
            audioManager.playSFX(audioManager.powerup, 0.6f);
            Destroy(gameObject);
        }
    }
}