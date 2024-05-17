using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscapeMenuUI : MonoBehaviour {

    public static EscapeMenuUI Instance { get; private set; }

    [SerializeField] private Transform resumeButton;
    [SerializeField] private Transform quitButton;
    [SerializeField] private Transform optionsButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button mainMenuButton;

    public bool isOpen { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        resumeButton.GetComponent<Button>().onClick.AddListener(() => {
            Hide();
        });
        quitButton.GetComponent<Button>().onClick.AddListener(() => Application.Quit());
        optionsButton.GetComponent<Button>().onClick.AddListener(() => Debug.Log("Options button clicked"));

        saveButton.onClick.AddListener(() => {
            SaveGameUI.Instance.Show();
        });

        mainMenuButton.onClick.AddListener(() => {
            SceneManager.LoadScene("MainMenuScene");
        });

        Hide();
    }

    public void ToggleVisbility() {
        if (gameObject.activeSelf) {
            Hide();
        } else {
            Show();
        }
    }

    public void Show() {
        gameObject.SetActive(true);
        isOpen = true;
        AutomationGameManager.Instance.CheckPauseState();
    }

    public void Hide() {
        gameObject.SetActive(false);
        isOpen = false;
        AutomationGameManager.Instance.CheckPauseState();
    }

}
