using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour {

    [Header("Game Speed")]
    [SerializeField] private Transform pauseButton;
    [SerializeField] private Transform speedUp1xButton;
    [SerializeField] private Transform speedUp2xButton;
    [SerializeField] private Transform speedUp3xButton;

    [Header("Current Time")]
    [SerializeField] private TextMeshProUGUI currentTimeText;

    private void Start() {
        pauseButton.GetComponent<Button>().onClick.AddListener(() => TimeManager.Instance.SetGameSpeed(0));
        speedUp1xButton.GetComponent<Button>().onClick.AddListener(() => TimeManager.Instance.SetGameSpeed(1));
        speedUp2xButton.GetComponent<Button>().onClick.AddListener(() => TimeManager.Instance.SetGameSpeed(2));
        speedUp3xButton.GetComponent<Button>().onClick.AddListener(() => TimeManager.Instance.SetGameSpeed(3));

        UpdateCurrentTimeText();

        TimeManager.Instance.OnHourChanged += UpdateCurrentTimeText;
    }

    private void UpdateCurrentTimeText() {
        currentTimeText.text = TimeManager.Instance.GetCurrentTimeString();
    }
}
