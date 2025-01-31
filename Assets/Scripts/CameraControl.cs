using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    public static CameraControl I;
    public CinemachineFreeLook freeLookCamera;
    public CinemachineVirtualCamera followCamera;
    //The camera's rotation speed
    public float rotationSpeed = 30f;

    private bool isCelebrating = false;

    void Start() {
        I = this;
        //At the start, the FreeLook camera's priority is low, and the virtual camera's priority is high.
        freeLookCamera.Priority = 0;
        followCamera.Priority = 10;
    }

    public void StartCelebration() {
        //The FreeLook camera is activated
        freeLookCamera.Priority = 10;
        //The Virtual camera is deactivated
        followCamera.Priority = 0;  
        //The rotation of the FreeLook camera
        freeLookCamera.m_XAxis.Value += rotationSpeed * Time.deltaTime;
    }
}
