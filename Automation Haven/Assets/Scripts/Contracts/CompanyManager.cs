using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActiveContractsDisplayUI;

public class CompanyManager : MonoBehaviour {

    public static CompanyManager Instance { get; private set; }

    public event Action<Company> OnCompanyBankrupted;

    [SerializeField] private CompanySO unknownCompanySO;

    private List<Company> companies;
    private List<Company> unlockedCompanies;
    private List<Company> bankruptCompanies;
    private List<Company> pausedCompanies;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
    }

    public void UnlockCompany(Company company) {
        unlockedCompanies.Add(company);

        if (company.companySO.reputationUnlockThreshold != 0) {
            MessageBarUI.Instance.CreateMessage("Partnership Established", company.companyName +
                " has established a partnership with you because of your reputation and you can now recieve contracts from them! " +
                "If you do not maintain your reputation they might take back their partnership.", EventType.Positive);
        }
    }

    public void RemoveUnlockedCompany(Company company) {
        unlockedCompanies.Remove(company);

        if (company.companySO.reputationUnlockThreshold != 0) {
            MessageBarUI.Instance.CreateMessage("Partnership Revoked", company.companyName +
                " has revoked their partnership with you because of your reputation and you can no longe recieve contracts from them, " +
                "any active contracts have been canceled. ", EventType.Negative);

            ContractManager.Instance.RemoveContractsForCompany(company, false);
        }
    }

    public void BankruptCompany(Company company) {
        if (!bankruptCompanies.Contains(company)) {
            bankruptCompanies.Add(company);
            unlockedCompanies.Remove(company);

            company.isBankrupt = true;

            OnCompanyBankrupted?.Invoke(company);

        } else {
            Debug.LogWarning("Attempted to bankrupt an already bankrupt company: " + company.companyName);
        }
    }

    public void UnBankruptCompany(Company company) {
        if (bankruptCompanies.Contains(company)) {
            bankruptCompanies.Remove(company);
            unlockedCompanies.Add(company);

            company.isBankrupt = false;
        } else {
            Debug.LogWarning("Attempted to unbankrupt a company that is not bankrupt: " + company.companyName);
        }
    }

    private void HandleCompanyToPlayerReputationChanged(Company company) {
        Debug.Log("Handling company to player reputation changed for: " + company.companyName + " with reputation: " + ReputationManager.Instance.GetPlayerReputation() + " and threshold: " + company.companySO.reputationUnlockThreshold);
        if (ReputationManager.Instance.GetPlayerReputation() >= company.companySO.reputationUnlockThreshold) {
            UnlockCompany(company);
        }

        if (ReputationManager.Instance.GetPlayerReputation() < company.companySO.reputationUnlockThreshold && unlockedCompanies.Contains(company)) {
            RemoveUnlockedCompany(company);
        }

    }

    public Company GetCompany(CompanySO companySO) {
        return companies.Find(company => company.companySO == companySO);
    }

    public List<Company> GetUnlockedCompanies() {
        return unlockedCompanies;
    }

    public List<Company> GetBankruptCompanies() {
        return bankruptCompanies;
    }

    public void PauseCompany(Company company) {
        pausedCompanies.Add(company);
        company.isPaused = true;
        unlockedCompanies.Remove(company);
    }

    public void UnPauseCompany(Company company) {
        unlockedCompanies.Add(company);
        pausedCompanies.Remove(company);
        company.isPaused = false;
    }

    private void SaveManager_OnGameSaved(string obj) {
        ES3.Save("companies", companies, obj);
        ES3.Save("unlockedCompanies", unlockedCompanies, obj);
        ES3.Save("bankruptCompanies", bankruptCompanies, obj);
        ES3.Save("pausedCompanies", pausedCompanies, obj);
    }

    private void SaveManager_OnGameLoaded(string obj) {
        companies = ES3.Load("companies", obj, new List<Company>());
        unlockedCompanies = ES3.Load("unlockedCompanies", obj, new List<Company>());
        bankruptCompanies = ES3.Load("bankruptCompanies", obj, new List<Company>());
        pausedCompanies = ES3.Load("pausedCompanies", obj, new List<Company>());

        if (companies.Count == 0) {
            companies = NewWorldManager.Instance.GetStartingCompanies();

            Company unknownCompany = new Company(unknownCompanySO, "Unknown Company");
            companies.Add(unknownCompany);

            foreach (Company company in companies) {
                HandleCompanyToPlayerReputationChanged(company);
            }
            
        }

        ReputationManager.Instance.InitializeReputationManager(companies);
        ReputationManager.Instance.OnCompanyToPlayerReputationChanged += HandleCompanyToPlayerReputationChanged;
    }
}
