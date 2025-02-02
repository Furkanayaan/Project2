using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlatformManager : MonoBehaviour {
    public static PlatformManager I;
    public GameObject[] platformPrefabs;
    public GameObject finishPlatformPrefab;
    public Transform initialPlatform;
    public float moveSpeed = 5f;
    public float platformLength;
    public float platformWidth;
    public float xDifferenceBetweenPlatforms;
    public float maxXPlatformSpawnPoint;
    public float minXPlatformSpawnPoint;
    public float totalRequiredPlatforms;

    public List<Transform> activePlatforms = new List<Transform>();
    //It checks if there is an x difference between the moving platform and the previous platform.
    public List<bool> bXDifferences = new List<bool>();
    private float _spawnPositionZ = 7.5f;
    private bool _bPlatformMoving = false;
    private Transform _currentMovingPlatform;
    private bool _bFailPlatform = false;
    private Vector3 _moveDirection;
    private bool _bFinishPlatform;
    private float _finishPlatformXPos;

    private void Start() {
        I = this;
        InitializePlatform();
    }

    private void Update() {
        if (CharacterController.I.HasFailed()) return;

        if (_bPlatformMoving && _currentMovingPlatform != null) {
            MovePlatform();
        }

        if (Input.GetMouseButtonDown(0) && _bPlatformMoving && CharacterController.I.hasStarted) {
            StopPlatformMovement();
        }
    }

    private void InitializePlatform() {
        initialPlatform.localScale = new Vector3(platformWidth, initialPlatform.localScale.y, platformLength);
        _spawnPositionZ = platformLength;
        activePlatforms.Add(initialPlatform);
        bXDifferences.Add(false);
    }

    private void MovePlatform() {
        Vector3 currentPosition = _currentMovingPlatform.position;

        if (currentPosition.x > maxXPlatformSpawnPoint) _moveDirection = Vector3.left;
        if (currentPosition.x < -maxXPlatformSpawnPoint) _moveDirection = Vector3.right;

        _currentMovingPlatform.position += _moveDirection * (moveSpeed + activePlatforms.Count * GameManager.Level / 10f) * Time.deltaTime;
    }

    public void SpawnNextPlatform() {
        if (_bFailPlatform) return;

        float spawnXPosition = DeterminePlatformXPosition();
        bool isFinalPlatform = activePlatforms.Count >= totalRequiredPlatforms + GameManager.Level - 1;
        bool isNormalPlatformComing = activePlatforms.Count + 1 < totalRequiredPlatforms + GameManager.Level - 1;

        Vector3 spawnPosition = new Vector3(
            IsFirstPlatform() ? _finishPlatformXPos : spawnXPosition,
            isFinalPlatform ? 0.5f : 0f,
            _spawnPositionZ
        );

        GameObject newPlatform = Instantiate(
            isFinalPlatform ? finishPlatformPrefab : platformPrefabs[Random.Range(0, platformPrefabs.Length)],
            spawnPosition,
            Quaternion.identity,
            transform
        );

        _bFinishPlatform = isFinalPlatform;
        _currentMovingPlatform = newPlatform.transform;
        
        if (IsFirstPlatform()) 
            InitializeFirstPlatform(newPlatform);
        
        else 
            ConfigurePlatform(newPlatform, isFinalPlatform, isNormalPlatformComing);
        
    }
    
    private float DeterminePlatformXPosition() {
        int chance = Random.Range(0, 2);
        //The possible value range if the moving platform will be positioned to the left of the stationary platform.
        float leftPos = Random.Range(-maxXPlatformSpawnPoint * 10f, -minXPlatformSpawnPoint * 10f) / 10f;
        //The possible value range if the moving platform will be positioned to the right of the stationary platform.
        float rightPos = Random.Range(minXPlatformSpawnPoint * 10f, maxXPlatformSpawnPoint * 10f) / 10f;
        return chance == 0 ? leftPos : rightPos;
    }

    //For the first platform after the level is completed
    private void InitializeFirstPlatform(GameObject platform) {
        SetPlatformScale(platform, platformWidth, platformLength);
        UpdateNextSpawnPosition(platform, true);
        StopPlatformMovement();
    }

    private void ConfigurePlatform(GameObject platform, bool isFinalPlatform, bool isNormalPlatformComing) {
        float width = isFinalPlatform ? 1f : initialPlatform.localScale.x;
        float length = isFinalPlatform ? 2f : platformLength;

        SetPlatformScale(platform, width, length);

        _moveDirection = platform.transform.position.x > initialPlatform.position.x ? Vector3.left : Vector3.right;
        _bPlatformMoving = true;
        
        UpdateNextSpawnPosition(platform, false, isNormalPlatformComing);
    }

    private void SetPlatformScale(GameObject platform, float width, float length) {
        platform.transform.localScale = new Vector3(width, platform.transform.localScale.y, length);
    }

    private void UpdateNextSpawnPosition(GameObject platform, bool isFirstPlatform, bool isNormalPlatformComing = false) {
        float offset = 0;
        if (isFirstPlatform) {
            offset = platform.transform.localScale.z;
        }
        else {
            //Separate values for the finish platform and normal platforms
            offset = isNormalPlatformComing ? platform.transform.localScale.z : platformLength / 2f + 1.8f;
        }
        _spawnPositionZ += offset;
    }

    public void DecreasePlatformWidth() {
        platformWidth -= GameManager.Level / 10f;
    }

    private void StopPlatformMovement() {
        _bPlatformMoving = false;
        HandlePlatformCut();

        //It checks whether the platform has failed or not.
        if (!_bFailPlatform) {
            activePlatforms.Add(_currentMovingPlatform);
            initialPlatform = _currentMovingPlatform;
        }

        //It checks whether the finish platform has arrived or not.
        if (!_bFinishPlatform) SpawnNextPlatform();
    }

    private void HandlePlatformCut() {
        if (_bFinishPlatform) {
            bXDifferences.Add(false);
            SoundManager.I.PlayNoteSound(true);
            _finishPlatformXPos = _currentMovingPlatform.position.x;
            return;
        }
        //Setting the position of the moving platform based on the difference in the x position between the previous stationary platform and the moving platform.
        if (Mathf.Abs(_currentMovingPlatform.position.x - initialPlatform.position.x) < xDifferenceBetweenPlatforms) {
            if(!IsFirstPlatform()) AlignMovingPlatform();
            bXDifferences.Add(false);
            return;
        }
        CutAndSeparatePlatforms();
    }

    //Setting the moving platform exactly in front of the stationary platform.
    private void AlignMovingPlatform() {
        _currentMovingPlatform.position = new Vector3(
            initialPlatform.position.x,
            initialPlatform.position.y,
            initialPlatform.position.z + platformLength
        );
        SoundManager.I.PlayNoteSound(true);
    }

    private void CutAndSeparatePlatforms() {
        bXDifferences.Add(true);
        
        Vector3 remainPlatformPos, cutPlatformPos;
        Vector3 remainScale, cutScale;
        
        if(!CalculatePlatformCut(out remainPlatformPos, out cutPlatformPos, out remainScale, out cutScale)) return;

        GameObject remainPlatform = Instantiate(_currentMovingPlatform.gameObject, remainPlatformPos, Quaternion.identity, transform);
        remainPlatform.transform.localScale = remainScale;

        GameObject cutPlatform = Instantiate(_currentMovingPlatform.gameObject, cutPlatformPos, Quaternion.identity, transform);
        cutPlatform.transform.localScale = cutScale;
        cutPlatform.AddComponent<Rigidbody>();

        Destroy(cutPlatform, 1f);
        Destroy(_currentMovingPlatform.gameObject);

        _currentMovingPlatform = remainPlatform.transform;
        SoundManager.I.PlayNoteSound(false);
    }

    private bool CalculatePlatformCut(out Vector3 remainPos, out Vector3 cutPos, out Vector3 remainScale, out Vector3 cutScale) {
        bool isRightSide = _currentMovingPlatform.position.x > initialPlatform.position.x;
        //The value of the right or left edge midpoint of the stationary platform based on the x position of the moving platform.
        float initialPlatformEdge = isRightSide ? initialPlatform.position.x + initialPlatform.localScale.x / 2f : initialPlatform.position.x - initialPlatform.localScale.x / 2f;
        //The value of the right or left edge midpoint of the moving platform based on the x position of the moving platform.
        float currentPlatformEdge = isRightSide ? _currentMovingPlatform.position.x - _currentMovingPlatform.localScale.x / 2f : _currentMovingPlatform.position.x + _currentMovingPlatform.localScale.x / 2f;

        //The position of the non-falling platform based on the x position of the moving platform.
        remainPos = new Vector3((initialPlatformEdge + currentPlatformEdge) / 2f, 0f, _currentMovingPlatform.position.z);
        //The scale vector of the non-falling platform based on the x position of the moving platform.
        remainScale = new Vector3(Mathf.Abs(initialPlatformEdge - currentPlatformEdge), _currentMovingPlatform.localScale.y, _currentMovingPlatform.localScale.z);
        
        //The value of the right or left edge midpoint of the cutting platform
        float cutPlatformEdge = isRightSide ? _currentMovingPlatform.position.x + _currentMovingPlatform.localScale.x / 2f : _currentMovingPlatform.position.x - _currentMovingPlatform.localScale.x / 2f;
        //The position of the falling platform based on the x position of the moving platform.
        cutPos = new Vector3((cutPlatformEdge + initialPlatformEdge) / 2f, 0f, _currentMovingPlatform.position.z);
        //The scale vector of the non-falling platform based on the x position of the moving platform.
        cutScale = new Vector3(Mathf.Abs(cutPlatformEdge - initialPlatformEdge), _currentMovingPlatform.localScale.y, _currentMovingPlatform.localScale.z);
        
        //If the left edge midpoint of the moving platform is greater than the right edge midpoint of the stationary platform.
        if (isRightSide && currentPlatformEdge > initialPlatformEdge) {
            FailPlatform();
            return false;
        }
        //If the right edge midpoint of the moving platform is smaller than the left edge midpoint of the stationary platform.
        if (!isRightSide && currentPlatformEdge < initialPlatformEdge) {
            FailPlatform();
            return false;
        }

        return true;
    }

    //The moving platform falls straight down.
    private void FailPlatform() {
        _currentMovingPlatform.gameObject.AddComponent<Rigidbody>();
        Debug.Log("Fail");
        _bFailPlatform = true;
    }

    public Transform GetCurrentPlatform(int index) => index >= activePlatforms.Count ? null : activePlatforms[index];

    public Transform GetNextPlatform(int index) => index + 1 >= activePlatforms.Count ? null : activePlatforms[index + 1];

    public bool IsXDifferent(int index) => index + 1 < bXDifferences.Count && bXDifferences[index + 1];

    public bool IsFirstPlatform() => activePlatforms.Count == 0;
    
    public void ResetPlatforms() {
        activePlatforms.Clear();
        bXDifferences.Clear();
        DecreasePlatformWidth();
    }
}

