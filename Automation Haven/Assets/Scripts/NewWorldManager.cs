using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ResourceNodeGenerator;

public class NewWorldManager : MonoBehaviour {

    public static NewWorldManager Instance { get; private set; }

    public event EventHandler OnStartingCompaniesChanged;

    [SerializeField] private List<ResourceNodeSettings> resourceNodeSettings;
    [SerializeField] private List<CompanySO> startingCompanySOs;
    [SerializeField] private List<CompanySO> availableCompanies;

    public string playerCompanyName { get; private set; }

    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);

    }

    public void CreateNewWorld() {
        SaveManager.Instance.SaveGame(playerCompanyName);
        SaveManager.Instance.LoadGame(playerCompanyName);
    }

    public List<ResourceNodeSettings> GetResourceNodeSettings() {
        return resourceNodeSettings;
    }

    public List<CompanySO> GetStartingCompanySOs() {
        return startingCompanySOs;
    }

    public void SetResourceNodeSettings(List<ResourceNodeSettings> settings) {
        resourceNodeSettings = settings;
    }

    public void SetStartingCompanySOs(List<CompanySO> companies) {
        startingCompanySOs = companies;
    }

    public void SetCompanyName(string companyName) {
        playerCompanyName = companyName;
    }

    public void RemoveCompanyFromStarting(CompanySO company) {
        startingCompanySOs.Remove(company);
        availableCompanies.Add(company);

        OnStartingCompaniesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddCompanyToStarting(CompanySO company) {
        startingCompanySOs.Add(company);
        availableCompanies.Remove(company);

        OnStartingCompaniesChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<CompanySO> GetAvailableCompanies() {
        return availableCompanies;
    }

}
