using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    private void Start() {
        continueButton.onClick.AddListener(ContinueButtonPressed);
        newGameButton.onClick.AddListener(NewGameButtonPressed);
        loadGameButton.onClick.AddListener(LoadGameButtonPressed);
        optionsButton.onClick.AddListener(OptionsButtonPressed);
        quitButton.onClick.AddListener(QuitButtonPressed);
        
    }

    public void ContinueButtonPressed() {
        Debug.Log("Continue Button Pressed");
        Loader.Load(Loader.Scene.GameScene);
    }

    public void NewGameButtonPressed() {
        NewWorldManager.Instance.Initialize();
        CreateNewWorldUI.Instance.Initialize();
        CreateNewWorldUI.Instance.Show();
    }

    public void LoadGameButtonPressed() {
        LoadGameUI.Instance.Show();
    }

    public void OptionsButtonPressed() {
        Debug.Log("Options button pressed");
    }

    public void QuitButtonPressed() {
        Application.Quit();
    }

}
