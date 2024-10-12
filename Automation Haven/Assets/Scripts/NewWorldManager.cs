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
    [SerializeField] private List<CompanySO> startingCompaniesSO;
    [SerializeField] private List<CompanySO> availableCompaniesSO;

    private List<Company> startingCompanies;
    private List<Company> availableCompanies;

    public string playerCompanyName { get; private set; }

    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);

    }

    public void Initialize() {
        startingCompanies = new List<Company>();
        availableCompanies = new List<Company>();

        foreach (CompanySO companySO in startingCompaniesSO) {
            Company company = new Company(companySO, companySO.companyNames[UnityEngine.Random.Range(0, companySO.companyNames.Length)]);
            startingCompanies.Add(company);
        }

        foreach (CompanySO companySO in availableCompaniesSO) {
            availableCompanies.Add(new Company(companySO, companySO.companyNames[UnityEngine.Random.Range(0, companySO.companyNames.Length)]));
        }
    }

    public void CreateNewWorld() {
        if (ES3.FileExists(SaveManager.SavePath + playerCompanyName + ".sav")) {
            Debug.Log("Overwriting previous save file.");
            ES3.DeleteFile(SaveManager.SavePath + playerCompanyName + ".sav");
        }

        SaveManager.Instance.SaveGame(playerCompanyName);
        SaveManager.Instance.LoadGame(playerCompanyName);
    }

    public List<ResourceNodeSettings> GetResourceNodeSettings() {
        return resourceNodeSettings;
    }

    public List<Company> GetStartingCompanies() {
        return startingCompanies;
    }

    public void SetResourceNodeSettings(List<ResourceNodeSettings> settings) {
        resourceNodeSettings = settings;
    }

    public void SetStartingCompanySOs(List<Company> companies) {
        startingCompanies = companies;
    }

    public void SetCompanyName(string companyName) {
        playerCompanyName = companyName;
    }

    public void RemoveCompanyFromStarting(Company company) {
        startingCompanies.Remove(company);
        availableCompanies.Add(company);

        OnStartingCompaniesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddCompanyToStarting(Company company) {
        startingCompanies.Add(company);
        availableCompanies.Remove(company);

        OnStartingCompaniesChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<Company> GetAvailableCompanies() {
        return availableCompanies;
    }

}
