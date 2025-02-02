using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager I; 
    public GameObject successUI;
    public Button successButton;
    public GameObject failedUI;
    public Button failButton;
    public TMP_Text levelText;
    public TMP_Text starText;
    public TMP_Text goldText;
    public TMP_Text diamondText;

    public Transform toTheStarUI;
    public Transform toTheGoldUI;
    public Transform toTheDiamondUI;
    // Start is called before the first frame update
    void Start() {
        I = this;
        failButton.onClick.AddListener(ClickFailedButton);
        
    }

    // Update is called once per frame
    void Update() {
        levelText.text = "Level: " + GameManager.Level;
        starText.text = "x" + GameManager.Star;
        goldText.text = "x" + GameManager.Gold;
        diamondText.text = "x" + GameManager.Diamond;
    }

    public void OpenSuccessUI() {
        successButton.onClick.RemoveAllListeners();
        successButton.onClick.AddListener(CharacterController.I.RestartMovement);
        successUI.SetActive(true);
    }

    public void HideSuccessUI() {
        successUI.SetActive(false);
    }
    
    public void OpenFailedUI() {
        failedUI.SetActive(true);
    }

    public void ClickFailedButton() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    //Function that makes the star currency move to the UI.
    public void StarPoolToGo(int quantity, Vector3 currentPos) {
        CurrencyPool.I.CurrencyAllocation(quantity, CurrencyPool.PoolType.Star, toTheStarUI, currentPos);
    }
    
    //Function that makes the gold currency move to the UI
    public void GoldPoolToGo(int quantity, Vector3 currentPos) {
        CurrencyPool.I.CurrencyAllocation(quantity, CurrencyPool.PoolType.Gold, toTheGoldUI, currentPos);
    }
    
    //Function that makes the diamond currency move to the UI
    public void DiamondPoolToGo(int quantity, Vector3 currentPos) {
        CurrencyPool.I.CurrencyAllocation(quantity, CurrencyPool.PoolType.Diamond, toTheDiamondUI, currentPos);
    }
    
}
