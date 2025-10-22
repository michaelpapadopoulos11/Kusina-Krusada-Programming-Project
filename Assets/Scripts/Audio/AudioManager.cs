using UnityEngine;

public class AudioManager : MonoBehaviour {

[Header("---------------- Audio Source ----------------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

[Header("----------------- Audio Clip -----------------")]
    public AudioClip background; // background music
    public AudioClip powerup; // when player picks up a powerup
    public AudioClip fruit_collected; // when player collects fruit


    private void Start() {
        musicSource.clip = background;
        musicSource.Play();
        }

    public void playSFX(AudioClip clip) {
        SFXSource.PlayOneShot(clip);
    }
}
