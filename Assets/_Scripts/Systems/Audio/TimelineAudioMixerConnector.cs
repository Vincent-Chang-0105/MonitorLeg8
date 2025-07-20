using UnityEngine;
using UnityEngine.Audio;

public class TimelineAudioMixerConnector : MonoBehaviour
{
    [Header("Mixer Settings")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup; // Assign your SFX mixer group
    [SerializeField] private bool applyOnStart = true;
    
    private AudioSource[] audioSources;
    
    void Start()
    {
        if (applyOnStart)
            ConnectToMixer();
    }
    
    public void ConnectToMixer()
    {
        // Get all AudioSource components
        audioSources = GetComponentsInChildren<AudioSource>();
        
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != null && sfxMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = sfxMixerGroup;
                Debug.Log($"Connected {audioSource.name} to SFX mixer group");
            }
        }
    }
    
    // Call this method from Timeline if needed
    public void SetMixerGroup(AudioMixerGroup mixerGroup)
    {
        sfxMixerGroup = mixerGroup;
        ConnectToMixer();
    }
}