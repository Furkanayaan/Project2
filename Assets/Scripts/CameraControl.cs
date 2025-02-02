using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    public static CameraControl I;
    public CinemachineFreeLook freeLookCamera;
    public CinemachineVirtualCamera followCamera;
    //The camera's rotation speed
    public float rotationSpeed = 30f;

    private bool _bCelebrating = false;

    void Start() {
        I = this;
        //At the start, the FreeLook camera's priority is low, and the virtual camera's priority is high.
        freeLookCamera.Priority = 0;
        followCamera.Priority = 10;
    }

    private void Update() {
        if (_bCelebrating) {
            ////The rotation of the FreeLook camera
            freeLookCamera.m_XAxis.Value += rotationSpeed * Time.deltaTime;
        }
        else {
            freeLookCamera.m_XAxis.Value = freeLookCamera.m_XAxis.Value;
        }
    }

    public void StartCelebration() {
        _bCelebrating = true;
        //The FreeLook camera is activated
        freeLookCamera.Priority = 10;
        //The Virtual camera is deactivated
        followCamera.Priority = 0;  
    }
    
    

    public void StopCelebration() {
        _bCelebrating = false;
        //The FreeLook camera is deactivated
        freeLookCamera.Priority = 0;
        //The Virtual camera is activated
        followCamera.Priority = 10;
    }
}
