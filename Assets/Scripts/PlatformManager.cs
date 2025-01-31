using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlatformManager : MonoBehaviour {
    public static PlatformManager I;
    public GameObject[] platformPrefab;
    public GameObject finishPlatformPrefab;
    public Transform initialPlatform;
    public float moveSpeed = 5f;
    public float platformLength;
    public float xDiffencesBetweenPlatforms;
    public float maxXPlatformSpawnPoint;
    public float minXPlatformSpawnPoint;

    public List<Transform> _activePlatforms = new List<Transform>();
    public List<bool> _bXDifferences = new List<bool>();
    private float _spawnPositionZ = 7.5f;
    private bool _bPlatformMoving = false;
    private Transform _currentMovingPlatform;
    private bool _bFailPlatform = false;
    private Vector3 _moveDirection;
    private bool _bFinishPlatform;

    void Start() {
        I = this;
        initialPlatform.localScale = new Vector3(initialPlatform.localScale.x, initialPlatform.localScale.y, platformLength);
        _spawnPositionZ = platformLength;
        _activePlatforms.Add(initialPlatform);
        _bXDifferences.Add(false);
        SpawnNextPlatform();
    }

    void Update()
    {
        if(CharacterController.I.IsFailed()) return;
        
        if (_bPlatformMoving && _currentMovingPlatform != null) {
            Vector3 currentPosition = _currentMovingPlatform.position;
            if (_currentMovingPlatform.position.x > maxXPlatformSpawnPoint) _moveDirection = Vector3.left;
            if(_currentMovingPlatform.position.x < -maxXPlatformSpawnPoint) _moveDirection = Vector3.right;
            
            _currentMovingPlatform.position = currentPosition + _moveDirection * (moveSpeed + _activePlatforms.Count * 0.05f)  * Time.deltaTime;
            
        }

        if (Input.GetMouseButtonDown(0) && _bPlatformMoving) {
            StopPlatform();
        }
    }

    public void SpawnNextPlatform() {
        if(_bFailPlatform) return;
        
        
        int chance = Random.Range(0, 2);
        //The possible value range if the moving platform will be positioned to the left of the stationary platform.
        float leftPos = Random.Range(maxXPlatformSpawnPoint * -10f, minXPlatformSpawnPoint * -10f) / 10f;
        //The possible value range if the moving platform will be positioned to the right of the stationary platform.
        float rightPos = Random.Range(minXPlatformSpawnPoint * 10f, maxXPlatformSpawnPoint * 10f) / 10f;
        float determinePos = chance == 0 ? leftPos : rightPos;
        
        
        int randomPlatform = Random.Range(0, platformPrefab.Length);
        bool bFinalPlatform = _activePlatforms.Count >= 15 + GameManager.Level ;
        Vector3 spawnPosition = new Vector3(determinePos, bFinalPlatform ? 0.5f : 0f, _spawnPositionZ);
        
        GameObject newPlatform = Instantiate(!bFinalPlatform ? platformPrefab[randomPlatform] : finishPlatformPrefab, spawnPosition, Quaternion.identity, transform);

        _bFinishPlatform = bFinalPlatform;
        
        
        
        newPlatform.transform.localScale = new Vector3(!bFinalPlatform ? initialPlatform.localScale.x : 1f, newPlatform.transform.localScale.y,
            !bFinalPlatform ? platformLength : 2f);
        _currentMovingPlatform = newPlatform.transform;
        
        _moveDirection = spawnPosition.x > initialPlatform.position.x ? Vector3.left : Vector3.right;
        _bPlatformMoving = true;
        
        float finalPlatformOffset = platformLength / 2f + 1.8f; 
        
        //Separate values for the finish platform and normal platforms
        _spawnPositionZ += _activePlatforms.Count + 1 < 15 + GameManager.Level
            ? newPlatform.transform.localScale.z
            : finalPlatformOffset;

    }

    private void StopPlatform() {
        _bPlatformMoving = false;
        
        CutPlatform();
        if (!_bFailPlatform) {
            _activePlatforms.Add(_currentMovingPlatform);
            initialPlatform = _currentMovingPlatform.transform;
        }
        
        if(!_bFinishPlatform) SpawnNextPlatform();
    }

    public void CutPlatform() {
        if (_bFinishPlatform) {
            _currentMovingPlatform.position = _currentMovingPlatform.position;
            _bXDifferences.Add(false);
            return;
        }
        
        //Setting the position of the moving platform based on the difference in the x position between the previous stationary platform and the moving platform.
        if (Mathf.Abs(_currentMovingPlatform.position.x - initialPlatform.position.x) < xDiffencesBetweenPlatforms) {
            //Setting the moving platform exactly in front of the stationary platform.
            _currentMovingPlatform.position =
                new Vector3(initialPlatform.position.x, initialPlatform.position.y, initialPlatform.position.z+platformLength);
            _bXDifferences.Add(false);
            SoundManager.I.PlayNoteSound(true);
            return;
        }
        _bXDifferences.Add(true);
        //The value of the right or left edge midpoint of the stationary platform based on the x position of the moving platform.
        float initialPlatformEdge = 0f;
        
        //The value of the right edge midpoint of the moving platform
        float currentPlatformRightPoint = _currentMovingPlatform.position.x + _currentMovingPlatform.localScale.x / 2f;
        
        //The value of the left edge midpoint of the moving platform
        float currentPlatformLeftPoint = _currentMovingPlatform.position.x - _currentMovingPlatform.localScale.x / 2f;
        
        //The x position value of the non-falling platform based on the x position of the moving platform.
        float remainPlatformPosX = 0f;
        
        //The x position value of the falling platform based on the x position of the moving platform.
        float cutPlatformPosX = 0f;
        
        //The scale vector of the non-falling platform based on the x position of the moving platform.
        Vector3 remainScale = _currentMovingPlatform.localScale;
        
        //The scale vector of the falling platform based on the x position of the moving platform.
        Vector3 cutScale = _currentMovingPlatform.localScale;
        
        //If the moving platform is to the right of the stationary platform.
        if (_currentMovingPlatform.transform.position.x > initialPlatform.position.x) {
            
            //Right edge midpoint
            initialPlatformEdge = initialPlatform.position.x + initialPlatform.localScale.x / 2f;
            
            //If the left edge midpoint of the moving platform is greater than the right edge midpoint of the stationary platform.
            if (currentPlatformLeftPoint > initialPlatformEdge) {
                FailPlatform();
                return;
            }
            remainPlatformPosX = (initialPlatformEdge + currentPlatformLeftPoint) / 2f;
            cutPlatformPosX = (currentPlatformRightPoint + initialPlatformEdge) / 2f;
            remainScale.x = initialPlatformEdge - currentPlatformLeftPoint;
            cutScale.x = currentPlatformRightPoint - initialPlatformEdge;

        }
        //If the moving platform is to the left of the stationary platform.
        else {
            
            //Left edge midpoint
            initialPlatformEdge = initialPlatform.position.x - initialPlatform.localScale.x / 2f;
            
            //If the right edge midpoint of the moving platform is smaller than the left edge midpoint of the stationary platform.
            if (currentPlatformRightPoint < initialPlatformEdge) {
                FailPlatform();
                return;
            }
            
            remainPlatformPosX = (initialPlatformEdge + currentPlatformRightPoint) / 2f;
            cutPlatformPosX = (currentPlatformLeftPoint + initialPlatformEdge) / 2f;
            remainScale.x = Mathf.Abs(initialPlatformEdge - currentPlatformRightPoint);
            cutScale.x = Mathf.Abs(currentPlatformLeftPoint - initialPlatformEdge);
            
        }

        Vector3 remainPlatformPos = new Vector3(remainPlatformPosX, 0f, _currentMovingPlatform.position.z);
        Vector3 cutPlatformPos = new Vector3(cutPlatformPosX, 0f, _currentMovingPlatform.position.z);
        
        GameObject remainPlatform = Instantiate(_currentMovingPlatform.gameObject, remainPlatformPos, Quaternion.identity, transform);
        remainPlatform.transform.localScale = remainScale;

        GameObject cutPlatform = Instantiate(_currentMovingPlatform.gameObject, cutPlatformPos, Quaternion.identity, transform);
        cutPlatform.transform.localScale = cutScale;
        
        cutPlatform.AddComponent<Rigidbody>();
        Destroy(cutPlatform, 0.5f);
        Destroy(_currentMovingPlatform.gameObject);
        _currentMovingPlatform = remainPlatform.transform;
        SoundManager.I.PlayNoteSound(false);
        
    }
    
    private void FailPlatform() {
        _currentMovingPlatform.gameObject.AddComponent<Rigidbody>();
        Debug.Log("Fail");
        _bFailPlatform = true;
    }

    public Transform GetCurrentPlatform(int index) {
        if (index >= _activePlatforms.Count) return null;
        return _activePlatforms[index].transform;
    }
    public Transform GetNextPlatform(int index) {
        if (index+1 >= _activePlatforms.Count) return null;
        return _activePlatforms[index+1].transform;
    }

    public bool IsXDifferent(int index) {
        if (index+1 >= _bXDifferences.Count) return false;
        return _bXDifferences[index+1];
    }
    
}
