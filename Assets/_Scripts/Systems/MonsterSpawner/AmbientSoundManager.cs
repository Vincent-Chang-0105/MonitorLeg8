using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

public class AmbientSoundManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private List<SoundData> ambientClips = new List<SoundData>();
    
    [Header("Player Reference")]
    [SerializeField] private Transform player;
    
    [Header("Settings")]
    [SerializeField] private float minPlayInterval = 3f;    // Minimum seconds between sounds
    [SerializeField] private float maxPlayInterval = 15f;   // Maximum seconds between sounds
    [SerializeField] private float minDistance = 5f;       // Minimum distance from player
    [SerializeField] private float maxDistance = 20f;      // Maximum distance from player
    [SerializeField] private int maxConcurrentSounds = 3;  // How many sounds can play at once
    
    private SoundBuilder soundBuilder;
    private List<SoundEmitter> activeSounds = new List<SoundEmitter>();
    private Coroutine ambientCoroutine;
    
    void Start()
    {
        // Wait a frame to ensure SoundManager is initialized
        StartCoroutine(InitializeWithDelay());
    }
    
    private IEnumerator InitializeWithDelay()
    {
        // Wait until SoundManager is available and fully initialized
        while (SoundManager.Instance == null || !IsSoundManagerReady())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // Initialize sound builder
        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
            }
        }
    
        
        // Start playing ambient sounds
        ambientCoroutine = StartCoroutine(PlayAmbientSounds());
    }
    
    private bool IsSoundManagerReady()
    {
        try
        {
            // Try to get a sound emitter to test if the pool is ready
            // This will throw an exception if the pool isn't initialized
            var testEmitter = SoundManager.Instance.Get();
            if (testEmitter != null)
            {
                // Return it immediately since we were just testing
                SoundManager.Instance.ReturnToPool(testEmitter);
                return true;
            }
        }
        catch
        {
            // Pool not ready yet
            return false;
        }
        return false;
    }
    
    private IEnumerator PlayAmbientSounds()
    {
        while (true)
        {
            // Clean up finished sounds
            activeSounds.RemoveAll(sound => sound == null || !sound.gameObject.activeInHierarchy);
            
            // Play new sound if we have clips and aren't at max concurrent sounds
            if (ambientClips.Count > 0 && activeSounds.Count < maxConcurrentSounds && player != null)
            {
                PlayRandomSound();
            }
            
            // Wait random interval before next sound
            float waitTime = Random.Range(minPlayInterval, maxPlayInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    private void PlayRandomSound()
    {
        // Additional safety check
        if (soundBuilder == null || SoundManager.Instance == null)
        {
            Debug.LogWarning("AmbientSoundManager: Sound system not ready");
            return;
        }
        
        // Pick random audio clip
        SoundData randomClip = ambientClips[Random.Range(0, ambientClips.Count)];
        
        // Get random position around player
        Vector3 randomPosition = GetRandomPositionAroundPlayer();
        
        try
        {
            // Play the sound
            SoundEmitter emitter = soundBuilder
                .WithPosition(randomPosition)
                .WithRandomPitch()
                .Play(randomClip);
            
            if (emitter != null)
            {
                activeSounds.Add(emitter);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AmbientSoundManager: Failed to play sound - {e.Message}");
        }
    }
    
    private Vector3 GetRandomPositionAroundPlayer()
    {
        // Generate random position in a circle around the player
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minDistance, maxDistance);
        
        Vector3 playerPos = player.position;
        Vector3 randomPos = playerPos + new Vector3(
            randomDirection.x * randomDistance,
            Random.Range(-2f, 2f), // Random height variation
            randomDirection.y * randomDistance
        );
        
        return randomPos;
    }
    
    // Public methods to control the system
    public void StopAllSounds()
    {
        StopAllCoroutines();
        foreach (var sound in activeSounds)
        {
            if (sound != null)
                sound.Stop();
        }
        activeSounds.Clear();
    }
    
    public void ResumeAmbientSounds()
    {
        StopAllSounds();
        StartCoroutine(PlayAmbientSounds());
    }
    
    // Visualize the sound spawn area in the editor
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, minDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, maxDistance);
    }
}
