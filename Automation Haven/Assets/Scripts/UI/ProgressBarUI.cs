using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour {

    private IHasProgress hasProgress;
    [SerializeField] private Image progressImage;
    [SerializeField] private TextMeshProUGUI remainingTimeText;


    private void Start() {
        hasProgress = transform.parent.GetComponent<IHasProgress>();
        if (hasProgress == null) {
            return;
        }
        hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;

        transform.GetComponent<ProgressBar>();

        gameObject.SetActive(false);
    }

    private void HasProgress_OnProgressChanged() {
        UpdateVisual();
    }

    private void UpdateVisual() {
        float progress = hasProgress.GetProgressNormalized();
        if (progress <= 0) {
            progressImage.fillAmount = progress;
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        progressImage.fillAmount = progress;
    }

    public void UpdateVisual(float progress) {
        progressImage.fillAmount = progress;
    }

    public void UpdateRemainingTime(float remainingTime) {
        remainingTimeText.text = remainingTime.ToString("F1") + "s";
    }
}
