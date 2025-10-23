using UnityEngine;

public class AudioManager : MonoBehaviour {

[Header("---------------- Audio Source ----------------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

[Header("----------------- Audio Clip -----------------")]
    public AudioClip background; // background music
    public AudioClip powerup; // when player picks up a powerup
    public AudioClip fruit_collected; // when player collects fruit

    public AudioClip run; // when player is running ----------------------------------
    public AudioClip switch_lanes; // when player switches lanes
    public AudioClip jump; // when player jumps
    public AudioClip slide; // when player ducks

    public AudioClip quiz_correct; // when player answers quiz correctly
    public AudioClip quiz_error; // when player answers quiz incorrectly

    public AudioClip button_press; // when player presses a button -- swapping ENG to TAG / maybe menu button too ??

    private void Start() {
        musicSource.clip = background;
        musicSource.Play();
        }

    public void playSFX(AudioClip clip, float volume = 1.0f) {
        if (clip == null) {
            Debug.LogWarning("AudioClip is null in playSFX");
            return;
        }
        SFXSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void setSFXPitch(float pitch) {
        SFXSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
    }
}
