using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class DeathScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject deathScreenUI;
    [SerializeField] private RawImage staticOverlay;
    
    [Header("Video Settings")]
    [SerializeField] private VideoClip staticVideo;
    [SerializeField] private float staticDuration = 2f;
    [SerializeField] private VideoPlayer videoPlayer;

    void Awake()
    {
        // Setup video player if not assigned
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24);
            videoPlayer.clip = staticVideo;
        }

        if (staticOverlay != null)
        {
            staticOverlay.texture = videoPlayer.targetTexture;
            staticOverlay.gameObject.SetActive(false);
        }

        deathScreenUI.SetActive(false);
    }

    void OnEnable()
    {
        UIEvents.OnOpenDeathScreen += ShowDeathScreen;
    }

    void OnDisable()
    {
        UIEvents.OnOpenDeathScreen -= ShowDeathScreen;
    }

    private void ShowDeathScreen()
    {
        // Your death screen logic here
        StartCoroutine(StaticSequence());
    }

    private IEnumerator StaticSequence()
    {
        // Show static overlay and play video
        staticOverlay.gameObject.SetActive(true);
        videoPlayer.Play();

        // Wait for static effect duration
        yield return new WaitForSeconds(staticDuration);

        // Stop video and sound
        //videoPlayer.Stop();

        // Hide static and show death screen
        //staticOverlay.gameObject.SetActive(false);
        deathScreenUI.SetActive(true);
    }

    public void retryFunction()
    {
        GameManager.Instance.ReloadCurrentScene();
        InputSystem.Instance.SetInputState(true);
    }

    public void exitFunction()
    {
        // Exit the game
        GameManager.Instance.LoadScene("MainMenu");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
