using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReputationManager : MonoBehaviour {

    // Player har en reputation och baserat på den reputationen får playern olika contracts
    // Companies har en fast reputation som inte går att ändra
    // Companies har en specifik reputation med playern

    public static ReputationManager Instance { get; private set; }

    private const string COMPANY_REPUTATION_WITH_PLAYER_DIC_KEY = "companyReputationWithPlayerDic";
    private const string PLAYER_REPUTATION_KEY = "playerReputation";

    [SerializeField] private CompanyListSO companyListSO;
    [SerializeField] private float playerReputation;

    private List<CompanySO> validCompanyList; 

    private Dictionary<CompanySO, float> companyReputationWithPlayerDic = new Dictionary<CompanySO, float>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameLoaded += LoadReputationData;
        SaveManager.OnGameSaved += SaveReputationData;
    }

    public void ChangeCompanyReputationWithPlayer(CompanySO company, float amount) {
        companyReputationWithPlayerDic[company] += amount;
        Mathf.Clamp(companyReputationWithPlayerDic[company], -100, 100);
    }

    public void ChangePlayerReputation(float amount) {
        playerReputation += amount;
        Mathf.Clamp(playerReputation, -100, 100);
    }

    public int GetReputationPenaltyPlayerToCompany(CompanySO company) {
        float companyReputationWithPlayer = companyReputationWithPlayerDic[company];

        float penaltyPercentage = 1 - Mathf.Clamp(companyReputationWithPlayer / 100, 0, 1); 
        int reputationPenalty = Mathf.RoundToInt((penaltyPercentage * 10) + 1); 

        return reputationPenalty;
    }

    public int GetReputationRewardPlayerToCompany(CompanySO company) {
        float companyReputationWithPlayer = companyReputationWithPlayerDic[company];

        float rewardPercentage = Mathf.Clamp((companyReputationWithPlayer + 100) / 200, 0, 1); 
        int reputationReward = Mathf.RoundToInt((rewardPercentage * 10) + 1); 

        return reputationReward;
    }

    public CompanySO GetRandomCompanyBasedOnPlayerReputation(List<CompanySO> validCompanies) {
        List<CompanySO> possibleCompanies = new List<CompanySO>();
        List<float> weights = new List<float>();

        validCompanyList = validCompanies;

        foreach (CompanySO company in validCompanies) {
            float reputationDifference = Mathf.Abs(playerReputation - company.companyWorldReputation);
            float weight = 1 / (reputationDifference + 1) + 0.1f;
            //Debug.Log("Reputation difference: " + reputationDifference + " Weight: " + weight + " for: " + company.companyName);

            possibleCompanies.Add(company);
            weights.Add(weight);
        }

        return SelectRandomCompany(possibleCompanies, weights);
    }

    private CompanySO SelectRandomCompany(List<CompanySO> companies, List<float> weights) {
        float totalWeight = 0;
        for (int i = 0; i < weights.Count; i++) {
            totalWeight += weights[i];
        }

        float randomValue = Random.Range(0, totalWeight);
        for (int i = 0; i < companies.Count; i++) {
            if (randomValue < weights[i]) {
                return companies[i];
            }
            randomValue -= weights[i];
        }

        return null; 
    }

    private void LoadReputationData(string saveName) {
        if (ES3.KeyExists(COMPANY_REPUTATION_WITH_PLAYER_DIC_KEY)) {
            companyReputationWithPlayerDic = ES3.Load(COMPANY_REPUTATION_WITH_PLAYER_DIC_KEY, saveName, new Dictionary<CompanySO, float>());
        } else {
            companyReputationWithPlayerDic = new Dictionary<CompanySO, float>();
            foreach (CompanySO company in validCompanyList) {
                companyReputationWithPlayerDic.Add(company, Random.Range(-100, 100));
            }
        }

        playerReputation = ES3.Load<float>(PLAYER_REPUTATION_KEY, saveName, 0);
    }

    private void SaveReputationData(string saveName) {
        ES3.Save(COMPANY_REPUTATION_WITH_PLAYER_DIC_KEY, companyReputationWithPlayerDic, saveName);
        ES3.Save(PLAYER_REPUTATION_KEY, playerReputation, saveName);
    }



}
