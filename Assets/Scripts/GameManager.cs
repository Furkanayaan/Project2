using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour {
    public static GameManager I;
    private int _level;
    private int _star;
    private int _diamond;
    private int _gold;
    void Start() {
        I = this;
        _level = 1;
    }

    public  void IncreaseLevel() {
        _level++;
    }

    public void IncreaseStar(int count) {
        _star += count;
    }
    
    public void IncreaseDiamond(int count) {
        _diamond += count;
    }
    
    public void IncreaseGold(int count) {
        _gold += count;
    }

    public int CurrentLevel() => _level;
    public int StarCount() => _star;
    public int DiamondCount() => _diamond;
    public int GoldCount() => _gold;


}
