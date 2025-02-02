using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class CharacterController : MonoBehaviour {
    public static CharacterController I;
    private bool isMoving = true;
    public float forwardSpeed = 5f;
    private Rigidbody characterRigidbody;
    private Animator animator;

    public int currentPlatformIndex = 0;
    private bool hasFailed;
    private bool hasFinished;
    private bool isSuccessUIOpening = false;
    public bool hasStarted = false;

    public float celebrationTimer;
    public GameObject tapToStart;

    private void Awake() {
        characterRigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        I = this;
        InitializeCharacterPosition();
        AdjustForwardSpeed();
    }

    private void Update() {
        HandleGameStart();
        if (!hasStarted) return;
        if (!isMoving) HandleSuccessUI();
        else MoveCharacter();
    }

    private void InitializeCharacterPosition() {
        ////The position of the character relative to the size of the first platform.
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
        if (!hasStarted && Input.GetMouseButtonDown(0)) {
            hasStarted = true;
            animator.SetTrigger("Run");
            tapToStart.SetActive(false);
            PlatformManager.I.SpawnNextPlatform();
        }
    }

    private void HandleSuccessUI() {
        if (!isSuccessUIOpening) {
            celebrationTimer += Time.deltaTime;
            if (celebrationTimer >= 5f) {
                UIManager.I.OpenSuccessUI();
                CameraControl.I.StopCelebration();
                celebrationTimer = 0f;
                isSuccessUIOpening = true;
            }
        }
    }

    private void MoveCharacter() {
        if (hasFailed || !isMoving) return;

        Vector3 targetPosition = CalculateTargetPosition();
        float targetXPosition = DetermineTargetXPosition();
        SmoothMoveToTarget(targetPosition, targetXPosition);
        CheckForFall();
    }

    private Vector3 CalculateTargetPosition() {
        //Increase the character's speed each time it passes a platform.
        return characterRigidbody.position + Vector3.forward * (forwardSpeed + currentPlatformIndex * GameManager.Level / 20f) * Time.deltaTime;
    }

    //Determining the midpoint for the next platform
    private float DetermineTargetXPosition() {
        Transform currentPlatform = PlatformManager.I.GetCurrentPlatform(currentPlatformIndex);

        if (currentPlatform != null && currentPlatform.CompareTag("FinishPlatform")) {
            return currentPlatform.position.x;
        }

        Transform nextPlatform = PlatformManager.I.GetNextPlatform(currentPlatformIndex);
        if (nextPlatform != null && PlatformManager.I.IsXDifferent(currentPlatformIndex)) {
            return nextPlatform.position.x;
        }

        return characterRigidbody.position.x;
    }

    //Smooth transition based on X position
    private void SmoothMoveToTarget(Vector3 targetPosition, float targetXPosition) {
        float smoothedX = Mathf.Lerp(characterRigidbody.position.x, targetXPosition, Time.deltaTime * 5f);
        characterRigidbody.MovePosition(new Vector3(smoothedX, characterRigidbody.position.y, targetPosition.z));
    }

    //The falling of the character
    private void CheckForFall() {
        if (transform.position.y <= -2f && !hasFailed) {
            TriggerFailState();
        }
    }

    private void TriggerFailState() {
        UIManager.I.OpenFailedUI();
        characterRigidbody.useGravity = false;
        characterRigidbody.velocity = Vector3.zero;
        animator.SetTrigger("Stop");
        hasFailed = true;
    }

    //Transition to the next level after successfully completing a section
    public void RestartMovement() {
        PlatformManager.I.ResetPlatforms();
        currentPlatformIndex = 0;
        PlatformManager.I.SpawnNextPlatform();
        GameManager.I.IncreaseLevel();
        UIManager.I.HideSuccessUI();
        animator.SetTrigger("Run");

        isMoving = true;
        hasFailed = false;
        isSuccessUIOpening = false;
    }

    public void StopMovement() {
        isMoving = false;
        characterRigidbody.velocity = Vector3.zero;
        animator.SetTrigger("Dance");
        CameraControl.I.StartCelebration();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("FinalPoint")) {
            currentPlatformIndex++;
        }

        if (other.CompareTag("FinishPoint")) {
            StopMovement();
        }
    }

    public bool HasFailed() {
        return hasFailed;
    }
}
