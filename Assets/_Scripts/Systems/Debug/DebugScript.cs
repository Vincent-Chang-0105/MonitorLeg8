using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadMainMenu()
    {
        GameManager.Instance?.LoadScene("MainMenu");

    }

    public void Loadlevel1()
    {
        GameManager.Instance?.LoadScene("Level1");
    }

    public void Loadlevel2()
    {
        GameManager.Instance?.LoadScene("Level2");
    }

    public void Loadlevel3()
    {
        GameManager.Instance?.LoadScene("Level3Pol");
    }
}
