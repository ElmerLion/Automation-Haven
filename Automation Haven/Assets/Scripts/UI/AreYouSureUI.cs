using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AreYouSureUI : BaseUI {

    public static AreYouSureUI Instance { get; private set; }

    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI areYouSureText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Hide();
    }

    public void ShowAreYouSure(string text, Action onYesPressed, Action onNoPressed = null) {
        areYouSureText.text = text;

        background.rectTransform.sizeDelta = new Vector2(areYouSureText.preferredWidth + 15, areYouSureText.preferredHeight + yesButton.GetComponent<Image>().rectTransform.sizeDelta.y + 40);
        Show();

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() => {
            onYesPressed?.Invoke();
            Hide();
        });

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(() => {
            if (onNoPressed != null) {
                onNoPressed?.Invoke();
            }
            Hide();
        });
    }

    private void OnDestroy() {
        Instance = null;
    }

    public override void Hide() {
        base.Hide();

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
    }

}
