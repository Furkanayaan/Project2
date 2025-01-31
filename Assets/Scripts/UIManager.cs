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

    public TMP_Text level;
    // Start is called before the first frame update
    void Start() {
        I = this;
        failButton.onClick.AddListener(ClickFailedButton);
        
    }

    // Update is called once per frame
    void Update() {
        level.text = "Level: " + GameManager.Level.ToString();
    }

    public void OpenSuccessUI() {
        successButton.onClick.RemoveAllListeners();
        successButton.onClick.AddListener(CharacterController.I.StartMoving);
        successUI.SetActive(true);
    }
    
    public void OpenFailedUI() {
        failedUI.SetActive(true);
    }

    public void ClickFailedButton() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
}
