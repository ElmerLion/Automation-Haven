using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Company", menuName = "ScriptableObjects/CompanySO")]
public class CompanySO : ScriptableObject {

    public string companyName;
    public string companyDescription;
    public int unlockedAtLevel = 1;
    public Sprite companyLogo;
    public float playerReputationMultiplier;
    public float playerRewardMultiplier;
    public float companyWorldReputation;

    [Tooltip("The higher the number, the more time this company will give.")]
    [Range(0.5f, 2)] public float timeModifier;

    public List<ItemSO> possibleNeededItems;


}
