using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterController : MonoBehaviour {
    public static CharacterController I;
    
    public GameObject tapToStart;
    public float forwardSpeed = 5f;
    public bool bStarted = false;
    public GameObject goldParticle;
    public GameObject starParticle;
    public GameObject diamondParticle;
    
    private Rigidbody _characterRigidbody;
    private Animator _animator;
    //The number of platforms passed.
    private int _currentPlatformIndex = 0;
    //Checks whether the character has fallen or not
    private bool _bFailed;
    //Checks whether the character has reached the finish point
    private bool _bFinished;
    //Checks whether the Success UI is open or not.
    private bool _bSuccessUIOpening = false;
    //The value that measures the time during the celebration.
    private float _celebrationTimer;
    

    private void Awake() {
        _characterRigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Start() {
        I = this;
        InitializeCharacterPosition();
        AdjustForwardSpeed();
    }

    private void Update() {
        HandleGameStart();
        if (!bStarted) return;
        if (_bFinished) HandleSuccessUI();
        else MoveCharacter();
    }

    private void InitializeCharacterPosition() {
        //The position of the character relative to the size of the first platform.
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            -PlatformManager.I.platformLength / 2f + 0.3f
        );
    }

    private void AdjustForwardSpeed() {
        //Speed determination based on the length of the platform
        forwardSpeed += PlatformManager.I.platformLength / 25f;
    }

    //Tap to start
    private void HandleGameStart() {
        if (!bStarted && Input.GetMouseButtonDown(0)) {
            bStarted = true;
            _animator.SetTrigger("Run");
            tapToStart.SetActive(false);
            PlatformManager.I.SpawnNextPlatform();
        }
    }

    //The function where SuccessUI opens.
    private void HandleSuccessUI() {
        if (!_bSuccessUIOpening) {
            _celebrationTimer += Time.deltaTime;
            if (_celebrationTimer >= 5f) {
                UIManager.I.OpenSuccessUI();
                CameraControl.I.StopCelebration();
                _celebrationTimer = 0f;
                _bSuccessUIOpening = true;
            }
        }
    }

    private void MoveCharacter() {
        if (_bFailed || _bFinished) return;

        Vector3 targetPosition = CalculateTargetPosition();
        float targetXPosition = DetermineTargetXPosition();
        SmoothMoveToTarget(targetPosition, targetXPosition);
        CheckForFall();
    }

    private Vector3 CalculateTargetPosition() {
        //Increase the character's speed each time it passes a platform.
        return _characterRigidbody.position + Vector3.forward * (forwardSpeed + _currentPlatformIndex * GameManager.I.CurrentLevel() / 20f) * Time.deltaTime;
    }

    //Determining the midpoint for the next platform
    private float DetermineTargetXPosition() {
        Transform currentPlatform = PlatformManager.I.GetCurrentPlatform(_currentPlatformIndex);
        Transform nextPlatform = PlatformManager.I.GetNextPlatform(_currentPlatformIndex);

        if (currentPlatform != null) {
            //If the platform the character is currently on is the finish platform.
            if(currentPlatform.CompareTag("FinishPlatform")) return currentPlatform.position.x;
            //If a platform has not been spawned in front of the platform the character is currently on.
            if (nextPlatform == null) return currentPlatform.position.x;
            //If a platform has been spawned in front of the platform the character is currently on, and its x value is different.
            if(PlatformManager.I.IsXDifferent(_currentPlatformIndex)) return nextPlatform.position.x;
        }

        return _characterRigidbody.position.x;
    }

    //Smooth transition based on X position
    private void SmoothMoveToTarget(Vector3 targetPosition, float targetXPosition) {
        float smoothedX = Mathf.Lerp(_characterRigidbody.position.x, targetXPosition, Time.deltaTime * 5f);
        Vector3 smoothedPosition = new Vector3(smoothedX, _characterRigidbody.position.y, targetPosition.z);
        _characterRigidbody.MovePosition(smoothedPosition);
    }

    //The falling of the character
    private void CheckForFall() {
        if (transform.position.y <= -2f && !_bFailed) {
            TriggerFailState();
        }
    }

    //If the character has started falling.
    private void TriggerFailState() {
        UIManager.I.OpenFailedUI();
        _characterRigidbody.useGravity = false;
        _characterRigidbody.velocity = Vector3.zero;
        _animator.SetTrigger("Stop");
        _bFailed = true;
    }

    //Transition to the next level after successfully completing a section
    public void RestartMovement() {
        PlatformManager.I.ResetPlatforms();
        _currentPlatformIndex = 0;
        PlatformManager.I.SpawnNextPlatform();
        GameManager.I.IncreaseLevel();
        UIManager.I.HideSuccessUI();
        _animator.SetTrigger("Run");

        _bFinished = false;
        _bFailed = false;
        _bSuccessUIOpening = false;
    }

    //If the character has reached the finish point.
    public void StopMovement() {
        _bFinished = true;
        _characterRigidbody.velocity = Vector3.zero;
        _animator.SetTrigger("Dance");
        CameraControl.I.StartCelebration();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("FinalPoint")) {
            _currentPlatformIndex++;
        }

        if (other.CompareTag("FinishPoint")) {
            StopMovement();
        }

        if (other.CompareTag("Star")) {
            UIManager.I.StarPoolToGo(1, transform.position);
            GameObject particle = Instantiate(starParticle, other.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            Destroy(particle, 0.5f);
        }
        
        if (other.CompareTag("Coin")) {
            UIManager.I.GoldPoolToGo(1, transform.position);
            GameObject particle = Instantiate(goldParticle, other.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            Destroy(particle, 0.5f);
        }
        
        if (other.CompareTag("Diamond")) {
            UIManager.I.DiamondPoolToGo(1, transform.position);
            GameObject particle = Instantiate(diamondParticle, other.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            Destroy(particle, 0.5f);
        }
    }

    public bool HasFailed() {
        return _bFailed;
    }
}
