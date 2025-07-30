using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChangeSceneTrigger : MonoBehaviour
{
    [SerializeField] private int sceneIndex;
    [SerializeField] private PlayerMovement playerMovement;
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
            // Get PlayerMovement component if not assigned
            if (playerMovement == null)
            {
                playerMovement = other.GetComponent<PlayerMovement>();
            }

            if (playerMovement != null)
            {
                playerMovement.DisableMovement(); // This will properly stop wheel sounds
            }
            StartCoroutine(LoadSceneWithDelay(1f));
        }
    }
    
    IEnumerator LoadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.LoadScene(sceneIndex);
    }
}
