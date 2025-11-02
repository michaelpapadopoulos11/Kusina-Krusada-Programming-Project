using UnityEngine;

public class Slowdown_Glove : MonoBehaviour
{

    AudioManager audioManager;

    public static bool slowActive = false;
    public float slowDuration = 5f;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        slowActive = false;
    }

    void OnTriggerEnter(Collider other)
    {
        Movement player = other.GetComponent<Movement>();
        if (player != null)
        {
            // Apply slowdown using current speed as reference
            player.ApplySlowdownEffect(slowDuration, 0.5f);
            slowActive = true;
            audioManager.playSFX(audioManager.powerup, 0.6f);
            Debug.Log("Player picked up slowdown glove");
            Destroy(gameObject);
        }
    }
}

