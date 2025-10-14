using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

    void Start() {
    }

    void OnTriggerEnter(Collider other) {
        Movement player = other.GetComponent<Movement>();
        if (player != null)
        {
            if (player.isInvincible) {
                Destroy(gameObject);
            }
            else {
                Debug.Log("Player hit obstacle while vulnerable");
            }
        }
    }
}
