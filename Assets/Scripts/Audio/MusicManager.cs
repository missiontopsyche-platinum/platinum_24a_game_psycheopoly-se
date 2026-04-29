using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    [Range(0f,1f)]
    [SerializeField] private float volume = 0.5f;
    [Header("Audio Files")]
    [SerializeField] private AudioSource introMusic;
    [SerializeField] private AudioSource loopMusic;

    private static MusicManager instance;

    public bool isMuted { get; private set; }

    public float Volume => volume;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
            isMuted = false;
        }
    }

    void Start()
    {
        StartCoroutine(BeginAudio());
    }

    private IEnumerator BeginAudio()
    {
        // wait until the first frame is done to avoid processing
        // lag that desyncs this group of methods from the audio DSP timing
        yield return new WaitForEndOfFrame();
        
        // get the start time relative to the audio DSPs internal clock with a buffer to ensure sync
        double startTime = AudioSettings.dspTime + .25f;
        double endTime = startTime + introMusic.clip.length;
        
        introMusic.loop = false;
        introMusic.volume = volume;
        // schedule the start of the sound
        introMusic.PlayScheduled(startTime);
        introMusic.SetScheduledEndTime(endTime);
        
        loopMusic.loop = true;
        loopMusic.volume = volume;
        // schedule the looped music to start immediately after the intro is done on DSP time
        loopMusic.PlayScheduled(endTime);
    }
    
    public static MusicManager GetInstance() => instance;

    public void SetVolume(float newVolume)
    {
        volume = newVolume;
        introMusic.volume = volume;
        loopMusic.volume = volume;
    }

    /// <summary>
    /// Toggles the mute state of the music.
    /// </summary>
    /// <returns>Current state of <c>isMuted</c></returns>
    public bool ToggleMute()
    {
        if (isMuted)
            Unmute();
        else
            Mute();
        
        return isMuted;
    }

    private void Mute()
    {
        isMuted = true;
        introMusic.mute = true;
        loopMusic.mute = true;
    }
    
    private void Unmute()
    {
        isMuted = false;
        introMusic.mute = false;
        loopMusic.mute = false;
    }
}
