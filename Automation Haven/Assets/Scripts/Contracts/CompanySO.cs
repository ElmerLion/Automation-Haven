using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Company", menuName = "ScriptableObjects/CompanySO")]
public class CompanySO : ScriptableObject {

    public string[] companyNames;
    public string companyDescription;
    public Sprite companyLogo;
    public float playerReputationMultiplier;
    public float playerRewardMultiplier;
    public float companyWorldReputation;
    public float reputationUnlockThreshold;
    public float startingReputationWithPlayer;
    public CompanyType companyType;

    [Tooltip("The higher the number, the more time this company will allow.")]
    [Range(0.5f, 2)] public float timeModifier;

    public List<ItemSO> startingContractItems;
    public List<ItemSO> unlockableProducts;

    [Space(10)]
    [TextArea(5, 10)]
    public string notes;


}

public enum CompanyType {
    Trustworthy,
    Sketchy,
}

public class Company {

    public CompanySO companySO;
    public string companyName;
    public float reputationWithPlayer;
    public bool isPaused;
    public bool isBankrupt;
    public int daysSinceLastContractCompleted;

    public int productDevelopmentProgress;
    public int productDevelopmentCost;
    public int productDevelopmentTime;

    public List<ItemSO> contractItems;

    public Company(CompanySO companySO, string companyName) {
        this.companySO = companySO;
        this.companyName = companyName;
        reputationWithPlayer = companySO.startingReputationWithPlayer;
        contractItems = new List<ItemSO>(companySO.startingContractItems);
        daysSinceLastContractCompleted = 0;
    }

    public void AddContractItem(ItemSO item) {
        contractItems.Add(item);
    }

    public bool HasDevelopedNewProduct() {
        foreach (ItemSO item in companySO.unlockableProducts) {
            if (contractItems.Contains(item)) return true;
        }
        return false;
    }

    public void SetTimeSinceLastContractCompleted(int time) {
        daysSinceLastContractCompleted = time;
    }

    public float GetTimeSinceLastContractCompleted() {
        return daysSinceLastContractCompleted;
    }

}
