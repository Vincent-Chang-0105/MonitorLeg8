using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private GameObject deathScreenUI;

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
        deathScreenUI.SetActive(true);
    }

    public void retryFunction()
    {
        SceneManager.LoadScene("Level1");
        InputSystem.Instance.SetInputState(true);
    }

    public void exitFunction()
    {
        // Exit the game
        SceneManager.LoadScene("MainMenu");
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
