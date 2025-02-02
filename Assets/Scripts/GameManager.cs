using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager I;
    public static int Level;
    public static int Star;
    public static int Diamond;
    public static int Gold;
    void Start() {
        I = this;
        Level = 1;
    }

    public  void IncreaseLevel() {
        Level++;
    }

    public void IncreaseStar(int count) {
        Star += count;
    }
    
    public void IncreaseDiamond(int count) {
        Diamond += count;
    }
    
    public void IncreaseGold(int count) {
        Gold += count;
    }

    
}
