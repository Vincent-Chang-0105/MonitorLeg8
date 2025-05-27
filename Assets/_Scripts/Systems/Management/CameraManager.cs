using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : ImpersistentSingleton<CameraManager>
{
    [SerializeField] private CinemachineVirtualCamera playerCamera;

    private List<CinemachineVirtualCamera> jumpscareCameras = new();

    private CinemachineVirtualCamera currentCam;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddJumpscareCamera(CinemachineVirtualCamera jumpscareCamera)
    {
        if (!jumpscareCameras.Contains(jumpscareCamera))
        {
            jumpscareCameras.Add(jumpscareCamera);
        }
    }

    public void SwitchToJumpscareCamera(CinemachineVirtualCamera jumpscareCamera)
    {
        jumpscareCamera.Priority = 20;
    }
}
