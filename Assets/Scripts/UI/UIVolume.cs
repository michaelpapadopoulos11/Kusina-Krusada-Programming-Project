using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVolume : MonoBehaviour
{
    [SerializeField] private Slider VolumeSlider;
    public static float soundVolume = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        VolumeSlider.value = soundVolume;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void changeVolume()
    {
        soundVolume = VolumeSlider.value;
        Debug.Log(soundVolume);
    }
}
