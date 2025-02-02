using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlatformManager : MonoBehaviour {
    public static PlatformManager I;
    public Transform initialPlatform;
    public float moveSpeed = 5f;
    public float platformLength;
    public float platformWidth;
    //The maximum x-distance for perfect timing between the stationary and moving platforms.
    public float xDifferenceBetweenPlatforms;
    //The platform value that can be generated at the max X position.
    public float maxXPlatformSpawnPoint;
    //The platform value that can be generated at the min X position.
    public float minXPlatformSpawnPoint;
    //The number of platforms to be generated until the finish platform.
    public float totalRequiredPlatforms;

    public GameObject starPrefab;
    public GameObject goldPrefab;
    public GameObject diamondPrefab;

    //Platforms present in the scene.
    private List<Transform> _activePlatforms = new List<Transform>();
    //It checks if there is an x difference between the moving platform and the previous platform.
    private List<bool> _bXDifferences = new List<bool>();
    //The z-distance between platforms.
    private float _spawnPositionZ = 7.5f;
    private bool _bPlatformMoving = false;
    //Moving platform
    private Transform _currentMovingPlatform;
    //Checks whether the entire platform has fallen down
    private bool _bFailPlatform = false;
    //Determines whether the platform to be generated will move to the right or to the left.
    private Vector3 _moveDirection;
    //Checks whether the finish platform has appeared.
    private bool _bFinishPlatform;
    //The x-position of the finish platform.
    private float _finishPlatformXPos;
    //The list holding the platforms to be deactivated.
    private List<Transform> _deactivePlatforms = new List<Transform>();
    
    //The class where we activate the platforms we need and deactivate those we don't.
    [Serializable]
    public class ObjectPool {
        public GameObject[] platformPrefabs;
        public GameObject finishPlatformPrefab;
        public Transform activeChild;
        public Transform activeFinishChild;
        public Transform deactiveChild;
        public Transform deactiveFinishChild;

        
        //A function that allows us to fetch an platform from the deactivated parent.
        public Transform GetPooledObject(bool isFinish) {
            if ((!isFinish && deactiveChild.childCount <= 0) || (isFinish && deactiveFinishChild.childCount <= 0)) {
                int randomPlatform = Random.Range(0, platformPrefabs.Length);
                Instantiate(isFinish ? finishPlatformPrefab : platformPrefabs[randomPlatform], Vector3.zero, Quaternion.identity, isFinish ? deactiveFinishChild : deactiveChild);
                return GetPooledObject(isFinish);
            }
            
            Transform obj = isFinish ? deactiveFinishChild.GetChild(0) : deactiveChild.GetChild(0);
            
            obj.transform.SetParent(isFinish ? activeFinishChild.transform  : activeChild.transform);
            return obj;
        }
        //The function where we assign the platform as a child of the deactivated parent and deactivate it
        public void ReturnToPool(GameObject obj) {
            
            if (!obj.CompareTag("FinishPlatform")) obj.transform.SetParent(deactiveChild.transform);
            
            else obj.transform.SetParent(deactiveFinishChild.transform);
            
            
        }
    }

    public ObjectPool CobjectPool = new();

    private void Start() {
        I = this;
        InitializePlatform();
    }

    private void Update() {
        if (CharacterController.I.HasFailed()) return;

        if (_bPlatformMoving && _currentMovingPlatform != null) {
            MovePlatform();
        }

        if (Input.GetMouseButtonDown(0) && _bPlatformMoving && CharacterController.I.bStarted) {
            StopPlatformMovement();
        }
    }

    //The initialize of the first platform.
    private void InitializePlatform() {
        initialPlatform.localScale = new Vector3(platformWidth, initialPlatform.localScale.y, platformLength);
        _spawnPositionZ = platformLength;
        _activePlatforms.Add(initialPlatform);
        _bXDifferences.Add(false);
    }

    private void MovePlatform() {
        Vector3 currentPosition = _currentMovingPlatform.position;

        if (currentPosition.x > maxXPlatformSpawnPoint) _moveDirection = Vector3.left;
        if (currentPosition.x < -maxXPlatformSpawnPoint) _moveDirection = Vector3.right;

        _currentMovingPlatform.position += _moveDirection * (moveSpeed + _activePlatforms.Count * GameManager.I.CurrentLevel() / 10f) * Time.deltaTime;
    }

    public void SpawnNextPlatform() {
        if (_bFailPlatform) return;

        float spawnXPosition = DeterminePlatformXPosition();
        
        //Checking whether the finish platform will appear or not.
        bool isFinalPlatform = _activePlatforms.Count >= totalRequiredPlatforms + GameManager.I.CurrentLevel() - 1;
        
        //Checking if the next platform is the finish platform.
        bool isNormalPlatformComing = _activePlatforms.Count + 1 < totalRequiredPlatforms + GameManager.I.CurrentLevel() - 1;

        Vector3 spawnPosition = new Vector3(
            IsFirstPlatform() ? _finishPlatformXPos : spawnXPosition,
            isFinalPlatform ? 0.5f : 0f,
            _spawnPositionZ
        );
        Transform newPlatform = CobjectPool.GetPooledObject(isFinalPlatform);
        newPlatform.position = spawnPosition;

        _bFinishPlatform = isFinalPlatform;
        _currentMovingPlatform = newPlatform;
        
        if (IsFirstPlatform()) 
            InitializeFirstPlatform(newPlatform);
        
        else 
            ConfigurePlatform(newPlatform, isFinalPlatform, isNormalPlatformComing);
        
    }
    
    //Determining the x-position of the platform to be spawned.
    private float DeterminePlatformXPosition() {
        int chance = Random.Range(0, 2);
        //The possible value range if the moving platform will be positioned to the left of the stationary platform.
        float leftPos = Random.Range(-maxXPlatformSpawnPoint * 10f, -minXPlatformSpawnPoint * 10f) / 10f;
        //The possible value range if the moving platform will be positioned to the right of the stationary platform.
        float rightPos = Random.Range(minXPlatformSpawnPoint * 10f, maxXPlatformSpawnPoint * 10f) / 10f;
        return chance == 0 ? leftPos : rightPos;
    }

    //For the first platform after the level is completed
    private void InitializeFirstPlatform(Transform platform) {
        SetPlatformScale(platform, platformWidth, platformLength);
        UpdateNextSpawnPosition(platform, true);
        StopPlatformMovement();
    }

    private void ConfigurePlatform(Transform platform, bool isFinalPlatform, bool isNormalPlatformComing) {
        float width = isFinalPlatform ? 1f : initialPlatform.localScale.x;
        float length = isFinalPlatform ? 2f : platformLength;

        SetPlatformScale(platform, width, length);

        _moveDirection = platform.position.x > initialPlatform.position.x ? Vector3.left : Vector3.right;
        _bPlatformMoving = true;
        
        UpdateNextSpawnPosition(platform, false, isNormalPlatformComing);
    }

    private void SetPlatformScale(Transform platform, float width, float length) {
        platform.localScale = new Vector3(width, platform.localScale.y, length);
    }

    private void UpdateNextSpawnPosition(Transform platform, bool isFirstPlatform, bool isNormalPlatformComing = false) {
        float offset = 0;
        if (isFirstPlatform) {
            offset = platform.localScale.z;
        }
        else {
            //Separate values for the finish platform and normal platforms
            offset = isNormalPlatformComing ? platform.localScale.z : platformLength / 2f + 1.8f;
        }
        _spawnPositionZ += offset;
    }

    public void DecreasePlatformWidth() {
        platformWidth -= GameManager.I.CurrentLevel() / 10f;
    }

    private void StopPlatformMovement() {
        _bPlatformMoving = false;
        HandlePlatformCut();
        //It checks whether the platform has failed or not.
        if(_bFailPlatform) return;
        
        _activePlatforms.Add(_currentMovingPlatform);
        initialPlatform = _currentMovingPlatform;
        
        //It checks whether the finish platform has arrived or not.
        SpawnCurrencies(_bFinishPlatform);
    }

    public void SpawnCurrencies(bool bDiamond) {
        if (!bDiamond) {
            //The size of the platform along the Z-axis is rounded down if it is a float.
            int platformHalfScale = Mathf.FloorToInt(_currentMovingPlatform.localScale.z/2f);
            //Determine the maximum Z position offset.
            int maxZpos = platformHalfScale - 1;
            //Create a list to store all possible Z positions.
            List<int> allZValue = new();
            for (int i = -maxZpos; i <= maxZpos; i++) {
                //Add each Z position within the range to the list.
                allZValue.Add(i);
            }
            
            for (int i = 0; i < allZValue.Count; i++) {
                //Randomly determine what to spawn (gold, star, or nothing).
                int determineWhichCurrency = Random.Range(1, 4);
                //Spawn gold if divisible by 3.
                bool bGold = determineWhichCurrency % 3 == 0;
                //Leave empty if remainder is 2.
                bool bEmpty = determineWhichCurrency % 3 == 2;
                //Skip this position if it is empty.
                if(bEmpty) continue;
                //Calculate the spawn position relative to the platform's current Z position.
                Vector3 spawnPos = new Vector3(_currentMovingPlatform.position.x, 0.5f, _currentMovingPlatform.position.z + allZValue[i]);
                //Spawn gold or star prefab based on the boolean condition.
                Instantiate(bGold ? goldPrefab : starPrefab, spawnPos, Quaternion.identity);
            } 
            
            SpawnNextPlatform();
        }
        else 
            Instantiate(diamondPrefab, _currentMovingPlatform.position, Quaternion.identity);
        
        
    }

    private void HandlePlatformCut() {
        //Setting the position of the moving platform based on the difference in the x position between the previous stationary platform and the moving platform.
        if (Mathf.Abs(_currentMovingPlatform.position.x - initialPlatform.position.x) < xDifferenceBetweenPlatforms) {
            if(!IsFirstPlatform()) AlignMovingPlatform();
            _bXDifferences.Add(false);
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
        
        
        Vector3 remainPlatformPos, cutPlatformPos, remainScale, cutScale;
        
        
        if(!CalculatePlatformCut(out remainPlatformPos, out cutPlatformPos, out remainScale, out cutScale)) return;
        if (_bFinishPlatform) {
            _bXDifferences.Add(false);
            SoundManager.I.PlayNoteSound(true);
            _finishPlatformXPos = _currentMovingPlatform.position.x;
            return;
        }
        _bXDifferences.Add(true);
        GameObject remainPlatform = Instantiate(_currentMovingPlatform.gameObject, remainPlatformPos, Quaternion.identity, CobjectPool.activeChild);
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
        //If the incoming platform is the finish platform, the left edge point at the 1st child and the right edge point at the 2nd child are checked.
        if (_bFinishPlatform) {
            currentPlatformEdge = isRightSide
                ? _currentMovingPlatform.GetChild(1).position.x // left edge
                : _currentMovingPlatform.GetChild(2).position.x; // right edge
        }
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
        if (isRightSide && (currentPlatformEdge > initialPlatformEdge) ) {
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
        _bFailPlatform = true;
    }

    public Transform GetCurrentPlatform(int index) => index >= _activePlatforms.Count ? null : _activePlatforms[index];

    public Transform GetNextPlatform(int index) => index + 1 >= _activePlatforms.Count ? null : _activePlatforms[index + 1];

    public bool IsXDifferent(int index) => index + 1 < _bXDifferences.Count && _bXDifferences[index + 1];

    public bool IsFirstPlatform() => _activePlatforms.Count == 0;
    
    public void ResetPlatforms() {
        //The platforms from 2 levels ago are deactivated.
        for (int i = 0; i < _deactivePlatforms.Count; i++) {
            CobjectPool.ReturnToPool(_deactivePlatforms[i].gameObject);
        }
        DeActivePlatforms();
        _activePlatforms.Clear();
        _bXDifferences.Clear();
        DecreasePlatformWidth();
    }

    //The function that adds currently active but future-unused platforms to the list.
    public void DeActivePlatforms() {
        _deactivePlatforms.Clear();
        for (int i = 0; i < _activePlatforms.Count; i++) {
            if (!_deactivePlatforms.Contains(_activePlatforms[i])) _deactivePlatforms.Add(_activePlatforms[i]);
        }
    }
}

