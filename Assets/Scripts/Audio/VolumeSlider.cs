using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private bool isSyncing = false;
    public void Start()
    {
        // using a bool gate to prevent event fires when we're setting
        // initial state
        isSyncing = true;
        // we're using Lerp to take our 0f-1f volume value and map it to our
        // volume slider, to allow the slider to be of different 'ranges'...
        // for safety.
        volumeSlider.value = Mathf.Lerp(
            volumeSlider.minValue, 
            volumeSlider.maxValue, 
            MusicManager.GetInstance().Volume);
        isSyncing = false;
    }

    public void OnVolumeChanged()
    {
        if (isSyncing) return; // if we're syncing with the MusicManager, return
        
        // we use InverseLerp to find the normalized (0-1f) value on our sliders range.
        // this maps it to the 0-1f range of the volume of our audio sources.
        MusicManager.GetInstance()
            .SetVolume(
                Mathf.InverseLerp(
                    volumeSlider.minValue, 
                    volumeSlider.maxValue, 
                    volumeSlider.value));
    }
}
