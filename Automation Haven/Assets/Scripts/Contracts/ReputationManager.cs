using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReputationManager : MonoBehaviour {

    // Player har en reputation och baserat på den reputationen får playern olika contracts
    // Companies har en fast reputation som inte går att ändra
    // Companies har en specifik reputation med playern

    public static ReputationManager Instance { get; private set; }

    private const string PLAYER_REPUTATION_KEY = "playerReputation";

    public event Action<Company> OnCompanyToPlayerReputationChanged;

    [SerializeField] private CompanyListSO companyListSO;
    [SerializeField] private float playerReputation;

    private List<Company> validCompanyList; 

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameSaved += SaveReputationData;
        ContractManager.Instance.OnContractCompleted += OnContractCompleted;
    }

    public void InitializeReputationManager(List<Company> validCompanies) {
        validCompanyList = validCompanies;

        LoadReputationData(SaveManager.CurrentSaveFileName);
    }

    public void ChangeCompanyReputationWithPlayer(Company company, float amount) {
        company.reputationWithPlayer += amount;
        Mathf.Clamp(company.reputationWithPlayer, -100, 100);

        OnCompanyToPlayerReputationChanged?.Invoke(company);
    }

    public void ChangePlayerReputation(float amount) {
        playerReputation += amount;
        Mathf.Clamp(playerReputation, -100, 100);
    }

    public int GetReputationPenaltyPlayerToCompany(Company company) {
        float companyReputationWithPlayer = company.reputationWithPlayer;

        float penaltyPercentage = 1 - Mathf.Clamp(companyReputationWithPlayer / 100, 0, 1); 
        int reputationPenalty = Mathf.RoundToInt((penaltyPercentage * 10) + 1); 

        return reputationPenalty;
    }

    public int GetReputationRewardPlayerToCompany(Company company) {
        float companyReputationWithPlayer = company.reputationWithPlayer;

        float rewardPercentage = Mathf.Clamp((companyReputationWithPlayer + 100) / 200, 0, 1); 
        int reputationReward = Mathf.RoundToInt((rewardPercentage * 10) + 1); 

        return reputationReward;
    }

    public Company GetRandomCompanyBasedOnPlayerReputation() {
        List<Company> possibleCompanies = new List<Company>();
        List<float> weights = new List<float>();

        foreach (Company company in validCompanyList) {
            float reputationDifference = Mathf.Abs(playerReputation - company.companySO.companyWorldReputation);
            float weight = 1 / (reputationDifference + 1) + 0.1f;
            //Debug.Log("Reputation difference: " + reputationDifference + " Weight: " + weight + " for: " + company.companyName);

            possibleCompanies.Add(company);
            weights.Add(weight);
        }

        return SelectRandomCompany(possibleCompanies, weights);
    }

    private Company SelectRandomCompany(List<Company> companies, List<float> weights) {
        float totalWeight = 0;
        for (int i = 0; i < weights.Count; i++) {
            totalWeight += weights[i];
        }

        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        for (int i = 0; i < companies.Count; i++) {
            if (randomValue < weights[i]) {
                return companies[i];
            }
            randomValue -= weights[i];
        }

        return null; 
    }

    private void OnContractCompleted(ContractManager.Contract contract) {
        Company company = contract.company;
        float reputationReward = GetReputationRewardPlayerToCompany(company);
        ChangeCompanyReputationWithPlayer(company, reputationReward);

        float reputationChange = company.companySO.companyType == CompanyType.Trustworthy ? UnityEngine.Random.Range(1, 3) : UnityEngine.Random.Range(-1, -3);

        ChangePlayerReputation(reputationChange);
    }



    private void LoadReputationData(string saveName) {
        playerReputation = ES3.Load<float>(PLAYER_REPUTATION_KEY, saveName, 0);
    }

    private void SaveReputationData(string saveName) {
        ES3.Save(PLAYER_REPUTATION_KEY, playerReputation, saveName);
    }

    public float GetPlayerReputation() {
        return playerReputation;
    }



}
