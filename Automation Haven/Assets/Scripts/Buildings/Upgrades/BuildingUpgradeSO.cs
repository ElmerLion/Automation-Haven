using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/BuildingUpgradeSO")]
public class BuildingUpgradeSO : ScriptableObject {

    public enum UpgradeType {
        Productivity,
        EnergyEfficiency,
    }

    [Header("General Info")]
    public string nameString;
    [TextArea] public string description;
    public UpgradeType upgradeType;
    public Sprite icon;

    [Header("Upgrade Values")]
    public int baseCost;
    public float costMultiplier;
    public float upgradeValue;
    public int maxLevel;

    [Header("Affected Upgrades")]
    public List<AffectedUpgrade> otherAffectedUpgrades;

    [System.Serializable]
    public class AffectedUpgrade {
        public BuildingUpgradeSO upgrade;
        public float value;
    }
}
