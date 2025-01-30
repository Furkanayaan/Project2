using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlatformManager : MonoBehaviour {
    public static PlatformManager I;
    public GameObject platformPrefab;
    public Transform initialPlatform;
    public float moveSpeed = 5f;
    public float platformLength;
    public float xDiffencesBetweenPlatforms;

    public List<Transform> _activePlatforms = new List<Transform>();
    public List<bool> _bXDifferences = new List<bool>();
    private float _spawnPositionZ = 7.5f;
    private bool _bPlatformMoving = false;
    private Transform _currentMovingPlatform;
    private bool _bFail = false;
    private Vector3 _moveDirection;

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
        if (_bPlatformMoving && _currentMovingPlatform != null) {
            Vector3 currentPosition = _currentMovingPlatform.position;
            if (_currentMovingPlatform.position.x > 10f) _moveDirection = Vector3.left;
            if(_currentMovingPlatform.position.x < -10f) _moveDirection = Vector3.right;
            
            _currentMovingPlatform.position = currentPosition + _moveDirection * moveSpeed * Time.deltaTime;
            
        }

        if (Input.GetMouseButtonDown(0) && _bPlatformMoving) {
            StopPlatform();
        }
    }

    public void SpawnNextPlatform() {
        if(_bFail) return;
        int chance = Random.Range(0, 2);
        //The possible value range if the moving platform will be positioned to the left of the stationary platform.
        float leftPos = Random.Range(-100f, -49f) / 10f;
        //The possible value range if the moving platform will be positioned to the right of the stationary platform.
        float rightPos = Random.Range(50f, 101f) / 10f;
        float determinePos = chance == 0 ? leftPos : rightPos;
        
        Vector3 spawnPosition = new Vector3(determinePos, 0, _spawnPositionZ);
        GameObject newPlatform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity, transform);
        
        newPlatform.transform.localPosition = new Vector3(newPlatform.transform.localPosition.x, 0f,
            newPlatform.transform.localPosition.z);
        
        newPlatform.transform.localScale = new Vector3(initialPlatform.localScale.x, newPlatform.transform.localScale.y,
            platformLength);
        
        
        _currentMovingPlatform = newPlatform.transform;
        
        _moveDirection = spawnPosition.x > initialPlatform.position.x ? Vector3.left : Vector3.right;
        _bPlatformMoving = true;
        _spawnPositionZ += newPlatform.transform.localScale.z;
    }

    private void StopPlatform() {
        _bPlatformMoving = false;
        
        CutPlatform();
        if (!_bFail) {
            _activePlatforms.Add(_currentMovingPlatform);
            initialPlatform = _currentMovingPlatform.transform;
        }
        
        SpawnNextPlatform();
    }

    public void CutPlatform() {
        //Setting the position of the moving platform based on the difference in the x position between the previous stationary platform and the moving platform.
        if (Mathf.Abs(_currentMovingPlatform.position.x - initialPlatform.position.x) < xDiffencesBetweenPlatforms) {
            //Setting the moving platform exactly in front of the stationary platform.
            _currentMovingPlatform.position =
                new Vector3(initialPlatform.position.x, initialPlatform.position.y, initialPlatform.position.z+platformLength);
            _bXDifferences.Add(false);
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
        
        GameObject remainPlatform = Instantiate(platformPrefab, remainPlatformPos, Quaternion.identity, transform);
        remainPlatform.transform.localScale = remainScale;

        GameObject cutPlatform = Instantiate(platformPrefab, cutPlatformPos, Quaternion.identity, transform);
        cutPlatform.transform.localScale = cutScale;
        
        cutPlatform.AddComponent<Rigidbody>();
        Destroy(cutPlatform, 0.5f);
        Destroy(_currentMovingPlatform.gameObject);
        _currentMovingPlatform = remainPlatform.transform;
        
    }
    
    private void FailPlatform() {
        _currentMovingPlatform.gameObject.AddComponent<Rigidbody>();
        Debug.Log("Fail");
        _bFail = true;
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
        if (index + 1 >= _bXDifferences.Count) return false;
        return _bXDifferences[index + 1];
    }
    
}
