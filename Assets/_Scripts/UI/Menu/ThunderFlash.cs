using UnityEngine;
using Cinemachine;
using System.Collections;
using AudioSystem;

public class ThunderFlash : MonoBehaviour
{
    [Header("Directional Light Settings")]
    public Light directionalLight;
    public float flashIntensity = 10f;
    public float flashRange = 50f;
    public Color flashColor = Color.white;
    public float flashDuration = 0.2f;
    
    [Header("Thunder Settings")]

    [SerializeField] SoundData[] thunderSounds; 
    private SoundBuilder soundBuilder;
    public float minInterval = 8f;
    public float maxInterval = 20f;
    
    [Header("Camera Shake")]
    public CinemachineImpulseSource impulseSource;
    public float shakeForce = 1f;
    
    private float originalIntensity;
    private float originalRange;
    private Color originalColor;
    
    void Start()
    {
        if (directionalLight != null)
        {
            originalIntensity = directionalLight.intensity;
            originalRange = directionalLight.range;
            originalColor = directionalLight.color;
        }

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        
        StartCoroutine(RandomThunderLoop());
    }
    
    IEnumerator RandomThunderLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            TriggerThunder();
        }
    }
    
    public void TriggerThunder()
    {
        StartCoroutine(ThunderSequence());
    }

    IEnumerator ThunderSequence()
    {
        if (directionalLight != null && directionalLight.enabled == false)
        {
            // Ensure light is enabled for the flash
            directionalLight.enabled = true;
        }

        // Multiple quick flashes for realistic lightning
        int flashCount = Random.Range(2, 5);

        for (int i = 0; i < flashCount; i++)
        {
            yield return StartCoroutine(SingleFlash());
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }

        directionalLight.enabled = false; // Disable light after flash

        // Play thunder sound after lightning
        yield return new WaitForSeconds(Random.Range(0.5f, 2f)); // Sound delay
        PlayThunderSound();

        // Camera shake with thunder
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(Vector3.one * shakeForce);
        }
        
    }
    
    IEnumerator SingleFlash()
    {
        if (directionalLight != null)
        {
            // Flash on with random intensity variation
            float randomIntensity = flashIntensity * Random.Range(0.7f, 1.3f);
            directionalLight.intensity = randomIntensity;
            directionalLight.range = flashRange;
            directionalLight.color = flashColor;
        }
        
        yield return new WaitForSeconds(flashDuration * Random.Range(0.5f, 1.5f));
        
        // Flash off
        if (directionalLight != null)
        {
            directionalLight.intensity = originalIntensity;
            directionalLight.range = originalRange;
            directionalLight.color = originalColor;
        }
    }
    
    void PlayThunderSound()
    {
        if (thunderSounds != null && thunderSounds.Length > 0)
        {
            SoundData randomThunder = thunderSounds[Random.Range(0, thunderSounds.Length)];
            soundBuilder.Play(randomThunder);
        }
    }
}