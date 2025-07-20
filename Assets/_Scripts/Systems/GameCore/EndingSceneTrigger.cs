using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class EndingSceneTrigger : MonoBehaviour
{
    [SerializeField] private VideoClip firstVideo; 
    [SerializeField] private VideoClip secondVideo;

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
            // Play first video, then second video, then load MainMenu
            PlayTwoVideosSequence();
        }
    }

    private void PlayTwoVideosSequence()
    {
        // Play first video
        VideoManager.Instance.PlayVideo(firstVideo, () =>
        {
            // When first video completes, play second video
            VideoManager.Instance.PlayVideo(secondVideo, () =>
            {
                // When second video completes, load MainMenu
                GameManager.Instance.LoadScene("MainMenu");
            });
        });
    }
}
