using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineUpgradesUI : MonoBehaviour {

    [SerializeField] private Transform singleUpgradeButtonUIPrefab;
    [SerializeField] private Transform upgradeButtonContainer;
    private Dictionary<UpgradeableMachine.MachineUpgrade, SingleUpgradeButtonUI> activeUpgradeButtons;
    private UpgradeableMachine machine;


    public void Setup(UpgradeableMachine machine) {
        singleUpgradeButtonUIPrefab.gameObject.SetActive(false);
        this.machine = machine;
        List<UpgradeableMachine.MachineUpgrade> machineUpgrades = machine.GetValidMachineUpgrades();

        if (activeUpgradeButtons == null) {
            activeUpgradeButtons = new Dictionary<UpgradeableMachine.MachineUpgrade, SingleUpgradeButtonUI>();
        }

        foreach (KeyValuePair<UpgradeableMachine.MachineUpgrade, SingleUpgradeButtonUI> activeUpgradeButton in activeUpgradeButtons) {
            if (!machineUpgrades.Contains(activeUpgradeButton.Key)) {
                activeUpgradeButton.Value.gameObject.SetActive(false);
                continue;
            }
            activeUpgradeButton.Value.gameObject.SetActive(true);
        }

        foreach (UpgradeableMachine.MachineUpgrade machineUpgrade in machineUpgrades) {

            if (activeUpgradeButtons.ContainsKey(machineUpgrade)) {
                activeUpgradeButtons[machineUpgrade].Setup(machineUpgrade, machine);
                continue;
            }

            SingleUpgradeButtonUI singleUpgradeButtonUI = Instantiate(singleUpgradeButtonUIPrefab, upgradeButtonContainer).GetComponent<SingleUpgradeButtonUI>();
            singleUpgradeButtonUI.transform.Find("Icon").GetComponent<Image>().sprite = machineUpgrade.upgradeSO.icon;
            singleUpgradeButtonUI.transform.name = machineUpgrade.upgradeSO.nameString;
            singleUpgradeButtonUI.Setup(machineUpgrade, machine);
            activeUpgradeButtons.Add(machineUpgrade,singleUpgradeButtonUI);

            singleUpgradeButtonUI.gameObject.SetActive(true);
        }
    }
    
}
