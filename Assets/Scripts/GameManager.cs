using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager I;
    public static int Level;
    void Start() {
        I = this;
    }

    // Update is called once per frame
    void Update() {
        
    }

    public  void EnhanceLevel() {
        Level++;
    }
}
