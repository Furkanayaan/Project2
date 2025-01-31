using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class CharacterController : MonoBehaviour {
    public static CharacterController I;
    private bool _isMoving = true;
    public float _forwardSpeed = 5f;
    private Rigidbody _rigidbody;
    public int currentPlatformIndex = 0;
    public bool characterXMovingToPlatform = false;
    private bool _bFailed;
    private Animator anim;
    private bool _bFinished;

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    private void Start() {
        I = this;
        //The position of the character relative to the size of the first platform.
        transform.position = new Vector3(transform.position.x, transform.position.x,
            0 - (PlatformManager.I.platformLength / 2f) + 0.3f);
        _forwardSpeed += PlatformManager.I.platformLength / 25f;
    }
    

    private void Update() {
        if (!_isMoving) {
            CameraControl.I.StartCelebration();
            return;
        }
        if (transform.position.y <= -2f) {
            UIManager.I.OpenFailedUI();
            _rigidbody.useGravity = false;
            _rigidbody.velocity = Vector3.zero;
            anim.SetTrigger("Stop");
            _bFailed = true;
            return;
        }
        
        //Increase the character's speed each time it passes a platform.
        Vector3 targetPosition = _rigidbody.position + Vector3.forward * (_forwardSpeed + currentPlatformIndex* 0.1f) * Time.deltaTime;
        float targetXPosition = 0f;

        if (PlatformManager.I.GetCurrentPlatform(currentPlatformIndex) != null && PlatformManager.I.GetCurrentPlatform(currentPlatformIndex).CompareTag("FinishPlatform")) {
            targetXPosition = PlatformManager.I.GetCurrentPlatform(currentPlatformIndex).position.x;
        }

        else {
            if (PlatformManager.I.GetNextPlatform(currentPlatformIndex) != null &&
                PlatformManager.I.IsXDifferent(currentPlatformIndex)) {
                targetXPosition = PlatformManager.I.GetNextPlatform(currentPlatformIndex).position.x;
            }
        
            else targetXPosition = _rigidbody.position.x;
        }
        

        float smoothedX = Mathf.Lerp(_rigidbody.position.x, targetXPosition, Time.deltaTime * 3f);
        _rigidbody.MovePosition(new Vector3(smoothedX, _rigidbody.position.y, targetPosition.z));
        
    }
    
    public void StartMoving() {
        _isMoving = true;
        anim.SetTrigger("Run");
    }

    public void StopMoving() {
        _isMoving = false;
        _rigidbody.velocity = Vector3.zero;
        anim.SetTrigger("Dance");
        
    }
    

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("FinalPoint")) {
            currentPlatformIndex++;
        }

        if (other.CompareTag("FinishPoint")) {
            StopMoving();
        }
    }

    public bool IsFailed() {
        return _bFailed;
    }
}
