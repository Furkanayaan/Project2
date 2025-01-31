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
    public float _celebrateTimer;
    public bool _bOpeningSuccessUI= false;
    public bool bStart = false;

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
        if (!bStart && Input.GetMouseButtonDown(0)) {
            bStart = true;
            anim.SetTrigger("Run");
        }
        if(!bStart) return;
        if (!_isMoving) {
            if (!_bOpeningSuccessUI) {
                _celebrateTimer += Time.deltaTime;
                if (_celebrateTimer >= 5f) {
                    UIManager.I.OpenSuccessUI();
                    CameraControl.I.StopCelebration();
                    _celebrateTimer = 0f;
                    _bOpeningSuccessUI = true;
                }
            }
            return;
        }
        CharacterMove();
    }

    public void CharacterMove() {
        if(_bFailed || !_isMoving) return;
        
        //Increase the character's speed each time it passes a platform.
        Vector3 targetPosition = _rigidbody.position + Vector3.forward * (_forwardSpeed + currentPlatformIndex * GameManager.Level/20f) * Time.deltaTime;
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
        

        float smoothedX = Mathf.Lerp(_rigidbody.position.x, targetXPosition, Time.deltaTime * 5f);
        _rigidbody.MovePosition(new Vector3(smoothedX, _rigidbody.position.y, targetPosition.z));
        
        //The falling of the character
        if (transform.position.y <= -2f && !_bFailed) {
            FailLevel();
        }
    }

    public void FailLevel() {
        UIManager.I.OpenFailedUI();
        _rigidbody.useGravity = false;
        _rigidbody.velocity = Vector3.zero;
        anim.SetTrigger("Stop");
        _bFailed = true;
    }
    
    public void StartMoving() {
        
        PlatformManager.I._activePlatforms.Clear();
        PlatformManager.I._bXDifferences.Clear();
        PlatformManager.I.DecreasePlatformWidth();
        currentPlatformIndex = 0;
        PlatformManager.I.SpawnNextPlatform();
        GameManager.I.EnhanceLevel();
        UIManager.I.successUI.SetActive(false);
        bStart = false;
        _isMoving = true;
        _bFailed = false;
        _bOpeningSuccessUI = false;
        //StartCoroutine(WaitAndStartMoving());

    }

    IEnumerator WaitAndStartMoving() {
        yield return new WaitForSeconds(0.5f);
        //anim.SetTrigger("Run");
        _isMoving = true;
        _bFailed = false;
        _bOpeningSuccessUI = false;
    }

    public void StopMoving() {
        _isMoving = false;
        _rigidbody.velocity = Vector3.zero;
        anim.SetTrigger("Dance");
        CameraControl.I.StartCelebration();
        
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
