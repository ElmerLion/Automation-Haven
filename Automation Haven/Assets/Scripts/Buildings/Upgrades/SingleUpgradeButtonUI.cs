using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleUpgradeButtonUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    private UpgradeableMachine.MachineUpgrade machineUpgrade;
    private UpgradeableMachine upgradeableMachine;
    private TextMeshProUGUI currentBonusText;


    public void Setup(UpgradeableMachine.MachineUpgrade machineUpgrade, UpgradeableMachine upgradeableMachine) {
        this.machineUpgrade = machineUpgrade;
        this.upgradeableMachine = upgradeableMachine;
        currentBonusText = transform.Find("CurrentBonusText").GetComponent<TextMeshProUGUI>();
        UpdateVisual();

        machineUpgrade.OnUpgradeValueChanged += UpdateVisual;

        Button button = transform.GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() => {
            upgradeableMachine.ApplyUpgrade(machineUpgrade);
            ShowTooltip();

        });


    }

    private void UpdateVisual() {
        currentBonusText.text = "Owned: " + machineUpgrade.GetCurrentLevel();
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            upgradeableMachine.SellUpgrade(machineUpgrade);
            ShowTooltip();
        }
    }

    private void ShowTooltip() {
        InterfaceToolTipUI.Instance.ShowMachineUpgradeToolTip(machineUpgrade);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData) {
        InterfaceToolTipUI.Instance.Hide();
    }

}

