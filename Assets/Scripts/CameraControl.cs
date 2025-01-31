using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    private Camera _camera;
    public Transform character;

    public float zOffset;
    // Start is called before the first frame update
    void Start() {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update() {
        _camera.transform.position = new Vector3(_camera.transform.position.x, _camera.transform.position.y,
            character.transform.position.z - zOffset);
    }
}
