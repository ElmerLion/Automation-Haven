using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour { 

    public static UIManager Instance { get; private set; }

    [SerializeField] private List<Transform> baseUITransformList;
    private List<BaseUI> uiList = new List<BaseUI>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        foreach (Transform closableUITransform in baseUITransformList) {
            BaseUI closableUI = closableUITransform.GetComponent<BaseUI>();
            if (closableUI != null) {
                uiList.Add(closableUI);
            }
        }

        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void GameInput_OnPauseAction(object sender, System.EventArgs e) {
        foreach (BaseUI ui in uiList) {
            if (ui.isOpen) {
                ui.Hide();
                return;
            }
        }

        if (EscapeMenuUI.Instance != null) {
            EscapeMenuUI.Instance.ToggleVisbility();
            return;
        }
    }

    public void CloseOtherUIs(BaseUI sender) {
        foreach (BaseUI ui in uiList) {
            if (ui.isOpen && ui != sender) {
                ui.Hide();
            }
        }
    }

    public List<BaseUI> GetBaseUIs() {
        return uiList;
    }


    private void OnDestroy() {
        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
        Instance = null;
    }
}
