using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager I; 
    public GameObject successUI;
    public Button successButton;
    public GameObject failedUI;

    public Button failButton;
    // Start is called before the first frame update
    void Start() {
        I = this;
        failButton.onClick.AddListener(ClickFailedButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenSuccessUI() {
        successUI.SetActive(true);
    }
    
    public void OpenFailedUI() {
        failedUI.SetActive(true);
    }

    public void ClickFailedButton() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
}
