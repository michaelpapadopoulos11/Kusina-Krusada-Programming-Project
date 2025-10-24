using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Double_Points : MonoBehaviour
{
    public bool doublePointsActive = false;
    public float doublePointsDuration = 5f;

    AudioManager audioManager;

    private void Awake() {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        doublePointsActive = false;
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other) {
        // Movement player = other.GetComponent<Movement>();
        // if (player != null) {
        //     player.doublePointsActive = true;
            
            
            Debug.Log("Player picked up double points powerup");
            audioManager.playSFX(audioManager.powerup, 0.6f);
            Destroy(gameObject);
        }
    }

