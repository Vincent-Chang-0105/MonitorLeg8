using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float flickerSpeed = 0.1f;
    [SerializeField] private float flickerChance = 0.3f;

    [Header("Advanced Settings")]
    [SerializeField] private bool useRandomFlickerIntervals = true;
    [SerializeField] private float minFlickerInterval = 0.05f;
    [SerializeField] private float maxFlickerInterval = 0.3f;
    [SerializeField] private bool enableIntensityNoise = true;
    [SerializeField] private float noiseStrength = 0.1f;
    [SerializeField] private float noiseSpeed = 2f;

    private Light lightComponent;
    private float originalIntensity;
    private float targetIntensity;
    private float currentIntensity;
    private float nextFlickerTime;
    private float noiseOffset;

    void Start()
    {
        // Get the Light component
        lightComponent = GetComponent<Light>();

        if (lightComponent == null)
        {
            Debug.LogError("LightFlicker: No Light component found on " + gameObject.name);
            enabled = false;
            return;
        }

        // Store original intensity
        originalIntensity = lightComponent.intensity;
        currentIntensity = originalIntensity;
        targetIntensity = originalIntensity;

        // Random noise offset for each light
        noiseOffset = Random.Range(0f, 100f);

        // Set initial flicker time
        SetNextFlickerTime();
    }

    void Update()
    {
        // Check if it's time to flicker
        if (Time.time >= nextFlickerTime)
        {
            if (Random.value < flickerChance)
            {
                TriggerFlicker();
            }
            SetNextFlickerTime();
        }

        // Smoothly interpolate to target intensity
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, flickerSpeed * Time.deltaTime * 10f);

        // Apply noise if enabled
        float finalIntensity = currentIntensity;
        if (enableIntensityNoise)
        {
            float noise = Mathf.PerlinNoise(Time.time * noiseSpeed + noiseOffset, 0) - 0.5f;
            finalIntensity += noise * noiseStrength;
        }

        // Apply the intensity to the light
        lightComponent.intensity = Mathf.Max(0, finalIntensity);
    }

    void TriggerFlicker()
    {
        // Randomly choose between dimming and brightening
        if (Random.value < 0.7f)
        {
            // Dim the light (more common for flicker effect)
            targetIntensity = Random.Range(minIntensity, originalIntensity * 0.8f);
        }
        else
        {
            // Brighten the light occasionally
            targetIntensity = Random.Range(originalIntensity, maxIntensity);
        }

        // Randomly decide how long to hold this intensity
        float holdTime = Random.Range(0.01f, 0.15f);
        StartCoroutine(ReturnToNormalAfterDelay(holdTime));
    }

    System.Collections.IEnumerator ReturnToNormalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Return to original intensity with some random variation
        targetIntensity = originalIntensity + Random.Range(-0.1f, 0.1f);
        targetIntensity = Mathf.Clamp(targetIntensity, minIntensity, maxIntensity);
    }

    void SetNextFlickerTime()
    {
        if (useRandomFlickerIntervals)
        {
            nextFlickerTime = Time.time + Random.Range(minFlickerInterval, maxFlickerInterval);
        }
        else
        {
            nextFlickerTime = Time.time + flickerSpeed;
        }
    }

    // Public methods to control the flicker from other scripts
    public void SetFlickerChance(float chance)
    {
        flickerChance = Mathf.Clamp01(chance);
    }

    public void SetFlickerSpeed(float speed)
    {
        flickerSpeed = Mathf.Max(0.01f, speed);
    }

    public void SetIntensityRange(float min, float max)
    {
        minIntensity = Mathf.Max(0, min);
        maxIntensity = Mathf.Max(minIntensity, max);
    }

    public void StartFlickering()
    {
        enabled = true;
    }

    public void StopFlickering()
    {
        enabled = false;
        if (lightComponent != null)
        {
            lightComponent.intensity = originalIntensity;
        }
    }
}
