using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EscapeMenuUI : MonoBehaviour {

    public static EscapeMenuUI Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Transform resumeButton;
    [SerializeField] private Transform quitButton;
    [SerializeField] private Transform optionsButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button mainMenuButton;

    public bool isOpen { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        resumeButton.GetComponent<Button>().onClick.AddListener(() => {
            Hide();
        });
        quitButton.GetComponent<Button>().onClick.AddListener(() => {
            AreYouSureUI.Instance.ShowAreYouSure("Are you sure you want to quit?", Application.Quit);

        });
        optionsButton.GetComponent<Button>().onClick.AddListener(() => Debug.Log("Options button clicked"));

        saveButton.onClick.AddListener(() => {
            SaveGameUI.Instance.Show();
        });

        loadButton.onClick.AddListener(() => {
            LoadGameUI.Instance.Show();
        });

        mainMenuButton.onClick.AddListener(() => {
            if (SaveManager.Instance == null) {
                Loader.Load(Loader.Scene.MainMenuScene);
                return;
            }

            if (SaveManager.Instance.GetMinutesFromLastSave() > 0) {
                AreYouSureUI.Instance.ShowAreYouSure($"Last save was {SaveManager.Instance.GetMinutesFromLastSave()} mins ago.\n You will lose all unsaved progress.", LoadMainMenu);
            } else {
                Loader.Load(Loader.Scene.MainMenuScene);
            }

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

    private void LoadMainMenu() {
        Loader.Load(Loader.Scene.MainMenuScene);
    }



}
