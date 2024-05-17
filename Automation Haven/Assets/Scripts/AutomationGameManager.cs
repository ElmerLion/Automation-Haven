using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomationGameManager : MonoBehaviour {

    public static AutomationGameManager Instance { get; private set; }
    
    [SerializeField] private List<Transform> closableUITransformList;
    private List<BaseUI> uiList = new List<BaseUI>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if (SaveManager.LoadGameOnStart) {
            SaveManager.LoadActiveSaveFile();
        }

        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void GameInput_OnPauseAction(object sender, System.EventArgs e) {


        if (GridBuildingSystem.Instance.IsPlacedObjectTypeSelected()) {
            GridBuildingSystem.Instance.DeselectObjectType();
            return;
        }

        CheckPauseState();
    }

    public void CheckPauseState() {
        foreach (BaseUI ui in UIManager.Instance.GetBaseUIs()) {
            if (ui.isOpen) {
                CameraMovement.Instance.SetPause(true);
                return;
            } 
        }

        if (EscapeMenuUI.Instance.isOpen) {
            CameraMovement.Instance.SetPause(true);
            return;
        }

        CameraMovement.Instance.SetPause(false);
    }
}
