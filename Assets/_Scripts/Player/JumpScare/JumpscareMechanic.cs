using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class JumpscareMechanic : MonoBehaviour
{
    private CinemachineVirtualCamera jumpscareCam;

    // Start is called before the first frame update
    void Start()
    {
        jumpscareCam = GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SwitchJumpscareCam()
    {
        jumpscareCam.Priority = 20;
    }
}
