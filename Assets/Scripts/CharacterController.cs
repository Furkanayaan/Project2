using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private bool _isMoving = true;
    public float _forwardSpeed = 5f;
    private Rigidbody _rigidbody;
    public int currentPlatformIndex = 0;
    public bool characterXMovingToPlatform = false;

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start() {
        //The position of the character relative to the size of the first platform.
        transform.position = new Vector3(transform.position.x, transform.position.x,
            0 - (PlatformManager.I.platformLength / 2f) + 0.3f);
    }

    public void StartMoving() {
        _isMoving = true;
    }

    public void StopMoving() {
        _isMoving = false;
        _rigidbody.velocity = Vector3.zero;
    }

    private void Update() {
        if (_isMoving) {
            Vector3 targetPosition = _rigidbody.position + Vector3.forward * _forwardSpeed * Time.deltaTime;
            float targetXPosition = 0f;
            if (PlatformManager.I.GetNextPlatform(currentPlatformIndex) != null && PlatformManager.I.IsXDifferent(currentPlatformIndex)) {
                targetXPosition = PlatformManager.I.GetNextPlatform(currentPlatformIndex).position.x;
            }
            else {
                targetXPosition = _rigidbody.position.x;
            }
            float smoothedX = Mathf.Lerp(_rigidbody.position.x, targetXPosition, Time.deltaTime * 3f);
            _rigidbody.MovePosition(new Vector3(smoothedX, _rigidbody.position.y, targetPosition.z));
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("FinalPoint")) {
            currentPlatformIndex++;
        }
    }
}
