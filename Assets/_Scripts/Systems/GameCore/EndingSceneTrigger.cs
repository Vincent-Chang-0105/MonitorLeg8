using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class EndingSceneTrigger : MonoBehaviour
{
    [SerializeField] private VideoClip firstVideo;
    [SerializeField] private VideoClip secondVideo;
    [SerializeField] private CharacterController playerController;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerController != null)
            {
                playerController.enabled = false; // Disable player controller to prevent movement during video playback
            }
            // Play first video, then second video, then load MainMenu
            StartCoroutine(PlayTwoVideosSequenceWithDelay(1f));
        }
    }

    private IEnumerator PlayTwoVideosSequenceWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerController != null)
        {
            playerController.enabled = false; // Disable player controller to prevent movement during video playback
        }

        PlayTwoVideosSequence();
    }
    private void PlayTwoVideosSequence()
    {
        Debug.Log("=== Starting video sequence ===");
        VideoManager.Instance.SetAllowSkipping(false);

        // Play first video
        VideoManager.Instance.PlayVideo(firstVideo, () =>
        {
            Debug.Log("=== FIRST VIDEO COMPLETED ===");
            StartCoroutine(PlaySecondVideoWithDelay(0.1f));
        });
    }

    IEnumerator PlaySecondVideoWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySecondVideo();
    }

    private void PlaySecondVideo()
    {
        Debug.Log("=== Playing second video ===");
        VideoManager.Instance.PlayVideo(secondVideo, () =>
        {
            Debug.Log("=== SECOND VIDEO COMPLETED - THIS SHOULD APPEAR ===");

            // When second video completes, load MainMenu
            VideoManager.Instance.SetAllowSkipping(true);
            GameManager.Instance.LoadScene("MainMenu");
        });
    }
}
