using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UpgradeableMachine;
public class UpgradeableMachine : MonoBehaviour {

    private List<MachineUpgrade> machineUpgrades;

    private BuildingTypeHolder buildingTypeHolder;

    private void Awake() {

        buildingTypeHolder = transform.GetComponent<BuildingTypeHolder>();
        machineUpgrades = new List<MachineUpgrade>();

        foreach (BuildingUpgradeSO buildingUpgradeSO in buildingTypeHolder.buildingType.validBuildingUpgrades) {
            MachineUpgrade machineUpgrade = new MachineUpgrade(buildingUpgradeSO, 0, buildingUpgradeSO.baseCost);
            machineUpgrades.Add(machineUpgrade);
        }    
    }

    public void ApplyUpgrades() {
        if (buildingTypeHolder.transform.TryGetComponent(out CraftingMachine craftingMachine)) {
            foreach (MachineUpgrade machineUpgrade in machineUpgrades) {
                HandleCraftingMachineUpgrade(craftingMachine, machineUpgrade);
            }
        }
    }

    public void ApplyUpgrade(MachineUpgrade machineUpgrade) {
        if (buildingTypeHolder.transform.TryGetComponent(out CraftingMachine craftingMachine)) {
            HandleCraftingMachineUpgrade(craftingMachine, machineUpgrade);
        }
    }

    public void SellUpgrade(MachineUpgrade machineUpgrade) {
        if (buildingTypeHolder.transform.TryGetComponent(out CraftingMachine craftingMachine)) {
            HandleCraftingMachineSellUpgrade(craftingMachine, machineUpgrade);
        }
    }

    private void HandleCraftingMachineSellUpgrade(CraftingMachine craftingMachine, MachineUpgrade machineUpgrade) {
        if (machineUpgrade.upgradeSO.upgradeType == BuildingUpgradeSO.UpgradeType.Productivity) {
            if (machineUpgrade.TrySell()) {
                craftingMachine.AddCraftingTimeMultiplier(-machineUpgrade.upgradeSO.upgradeValue);
                craftingMachine.powerReciever.AddPowerConsumptionMultiplier(-machineUpgrade.upgradeSO.otherAffectedUpgrades[0].value);

                UpdateAffectedUpgrades(machineUpgrade, true);
            }
        }
        if (machineUpgrade.upgradeSO.upgradeType == BuildingUpgradeSO.UpgradeType.EnergyEfficiency) {
            if (machineUpgrade.TrySell()) {
                craftingMachine.powerReciever.AddPowerConsumptionMultiplier(-machineUpgrade.upgradeSO.upgradeValue);
                craftingMachine.AddCraftingTimeMultiplier(-machineUpgrade.upgradeSO.otherAffectedUpgrades[0].value);

                UpdateAffectedUpgrades(machineUpgrade, true);
            }


        }
    }

    private void HandleCraftingMachineUpgrade(CraftingMachine craftingMachine, MachineUpgrade machineUpgrade) {


        if (machineUpgrade.upgradeSO.upgradeType == BuildingUpgradeSO.UpgradeType.Productivity) {
            if (machineUpgrade.TryUpgrade()) {
                craftingMachine.AddCraftingTimeMultiplier(machineUpgrade.upgradeSO.upgradeValue);
                craftingMachine.powerReciever.AddPowerConsumptionMultiplier(machineUpgrade.upgradeSO.otherAffectedUpgrades[0].value);

                UpdateAffectedUpgrades(machineUpgrade);
            }
        }
        if (machineUpgrade.upgradeSO.upgradeType == BuildingUpgradeSO.UpgradeType.EnergyEfficiency) {
            if (machineUpgrade.TryUpgrade()) {
                craftingMachine.powerReciever.AddPowerConsumptionMultiplier(machineUpgrade.upgradeSO.upgradeValue);
                craftingMachine.AddCraftingTimeMultiplier(machineUpgrade.upgradeSO.otherAffectedUpgrades[0].value);

                UpdateAffectedUpgrades(machineUpgrade);
            }


        }
    }

    private void UpdateAffectedUpgrades(MachineUpgrade machineUpgrade, bool isSelling = false) {
        foreach (BuildingUpgradeSO.AffectedUpgrade affectedUpgrade in machineUpgrade.upgradeSO.otherAffectedUpgrades) {
            foreach (MachineUpgrade upgrade in machineUpgrades) {
                if (upgrade.upgradeSO == affectedUpgrade.upgrade) {
                    if (isSelling) {
                        upgrade.Upgrade();
                    } else {
                        upgrade.Downgrade();
                    }
                }
            }
        }
    }

    public List<MachineUpgrade> GetValidMachineUpgrades() {
        return machineUpgrades;
    }

    public class MachineUpgrade {

        public event Action OnUpgradeValueChanged;

        public BuildingUpgradeSO upgradeSO;
        public float currentValue;
        public int currentLevel;
        public int currentPrice;

        public MachineUpgrade(BuildingUpgradeSO upgrade, float currentValue, int currentPrice) {
            this.upgradeSO = upgrade;
            this.currentValue = currentValue;
            this.currentPrice = currentPrice;
            this.currentLevel = 0;
        }

        public bool TryUpgrade() {
            if (currentLevel >= upgradeSO.maxLevel) { return false; }

            
            if (PlayerEconomyManager.Instance.TryRemoveMoney(currentPrice)){
                currentLevel++;
                currentValue += upgradeSO.upgradeValue;
                UpdatePriceAndInvokeEvent();
                return true;
            }

            return false;
                
        }

        public bool TrySell() {
            if (currentLevel <= 0) { return false; }

            currentLevel--;
            currentValue -= upgradeSO.upgradeValue;
            PlayerEconomyManager.Instance.AddMoney(currentPrice);
            UpdatePriceAndInvokeEvent();
            return true;
        }

        public void Upgrade() {
            currentValue += upgradeSO.upgradeValue;
            UpdatePriceAndInvokeEvent();
        }

        public void Downgrade() {
            currentValue -= upgradeSO.upgradeValue;
            UpdatePriceAndInvokeEvent();
        }


        private void UpdatePriceAndInvokeEvent() {
            currentPrice = (int)(upgradeSO.baseCost * (upgradeSO.costMultiplier * currentLevel));
            OnUpgradeValueChanged?.Invoke();
        }

        public string GetUpgradeValuePercent() {
            return (currentValue * 100).ToString("0.00");
        }

        public int GetCurrentLevel() {
            return currentLevel;
        }

        public int GetMaxLevel() {
            return upgradeSO.maxLevel;
        }
    }

}
