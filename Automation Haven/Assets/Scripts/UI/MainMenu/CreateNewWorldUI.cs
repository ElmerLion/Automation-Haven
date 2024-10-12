using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateNewWorldUI : MonoBehaviour {

    public static CreateNewWorldUI Instance { get; private set; }

    [SerializeField] private Button createNewWorldButton;
    [SerializeField] private Button backButton;

    [Header("Resource Settings")]
    [SerializeField] private GameObject resourceView;
    [SerializeField] private Button resourceSettingsButton;
    [SerializeField] private GameObject resourceSettingsContainer;
    [SerializeField] private GameObject resourceSettingsPrefab;
    private List<ResourceNodeGenerator.ResourceNodeSettings> resourceNodeSettings;

    [Header("Company Settings")]
    [SerializeField] private GameObject companyView;
    [SerializeField] private Button companySettingsButton;
    [SerializeField] private Button addCompanyButton;
    [SerializeField] private Transform addCompanyParent;
    [SerializeField] private GameObject companySettingsContainer;
    [SerializeField] private GameObject companySettingsPrefab;
    [SerializeField] private Transform addCompanyContainer;
    [SerializeField] private Transform addCompanyTemplate;
    private List<Company> startingCompanies;

    [Header("Are you Sure UI")]
    [SerializeField] private GameObject areYouSureUI;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Other")]
    [SerializeField] private Transform companyNamingTransform;
    [SerializeField] private TMP_InputField companyNameInputField;
    [SerializeField] private Button startGameButton;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Hide();
    }

    public void Initialize() {
        NewWorldManager.Instance.OnStartingCompaniesChanged += NewWorldManager_OnStartingCompaniesChanged;

        resourceView.SetActive(false);
        companyView.SetActive(true);
        areYouSureUI.SetActive(false);

        resourceNodeSettings = NewWorldManager.Instance.GetResourceNodeSettings();
        startingCompanies = NewWorldManager.Instance.GetStartingCompanies();

        companyNamingTransform.gameObject.SetActive(false);

        createNewWorldButton.onClick.AddListener(() => {
            companyNamingTransform.gameObject.SetActive(true);

            NewWorldManager.Instance.SetResourceNodeSettings(resourceNodeSettings);
            NewWorldManager.Instance.SetStartingCompanySOs(startingCompanies);
        });

        startGameButton.onClick.AddListener(() => {
            if (ES3.FileExists(SaveManager.SavePath + companyNameInputField.text + ".sav")) {
                areYouSureUI.SetActive(true);

                yesButton.onClick.AddListener(() => {
                    areYouSureUI.SetActive(false);
                    NewWorldManager.Instance.SetCompanyName(companyNameInputField.text);
                    NewWorldManager.Instance.CreateNewWorld();
                });

                noButton.onClick.AddListener(() => {
                    areYouSureUI.SetActive(false);
                });
                return;
            }

            NewWorldManager.Instance.SetCompanyName(companyNameInputField.text);
            NewWorldManager.Instance.CreateNewWorld();
        });

        backButton.onClick.AddListener(() => {
            Hide();
        });

        InitializeResourceSettings();
        InitializeCompanySettings();

    }

    private void NewWorldManager_OnStartingCompaniesChanged(object sender, System.EventArgs e) {
        UpdateStartingCompanies();
        UpdateAvailableCompanies();

    }

    private void InitializeResourceSettings() {
        resourceSettingsPrefab.SetActive(false);

        resourceSettingsButton.onClick.AddListener(() => {
            resourceView.SetActive(true);
            companyView.SetActive(false);

        });

        foreach (ResourceNodeGenerator.ResourceNodeSettings settings in resourceNodeSettings) {
            GameObject resourceSettings = Instantiate(resourceSettingsPrefab, resourceSettingsContainer.transform);
            resourceSettings.GetComponent<ResourceSettingsUI>().Initialize(settings);
        }
    }

    private void InitializeCompanySettings() {
        companySettingsPrefab.SetActive(false);
        addCompanyTemplate.gameObject.SetActive(false);

        companySettingsButton.onClick.AddListener(() => {
            companyView.SetActive(true);
            resourceView.SetActive(false);
        });

        foreach (Company company in startingCompanies) {
            GameObject companySettings = Instantiate(companySettingsPrefab, companySettingsContainer.transform);
            companySettings.GetComponent<SingleCompanySettingsUI>().Initialize(company);
        }

        addCompanyParent.SetAsLastSibling();

        addCompanyButton.onClick.AddListener(() => {
            if (NewWorldManager.Instance.GetAvailableCompanies().Count == 0) return;
            addCompanyContainer.gameObject.SetActive(true);
            UpdateAvailableCompanies();
        });

    }

    private void UpdateStartingCompanies() {
        foreach (Transform child in companySettingsContainer.transform) {
            if (child.gameObject == companySettingsPrefab || child.transform == addCompanyParent.transform) continue;
            Destroy(child.gameObject);
        }

        foreach (Company company in startingCompanies) {
            GameObject companySettings = Instantiate(companySettingsPrefab, companySettingsContainer.transform);
            companySettings.GetComponent<SingleCompanySettingsUI>().Initialize(company);
        }

        addCompanyParent.SetAsLastSibling();
    }

    private void UpdateAvailableCompanies() {

        foreach (Transform child in addCompanyContainer.transform) {
            if (child.gameObject == addCompanyTemplate.gameObject) continue;
            Destroy(child.gameObject);
        }

        foreach (Company company in NewWorldManager.Instance.GetAvailableCompanies()) {
            GameObject companySettings = Instantiate(addCompanyTemplate.gameObject, addCompanyContainer.transform);
            companySettings.GetComponent<SingleCompanySettingsUI>().Initialize(company);
        }
    }

    public void HideAddCompanies() {
        addCompanyContainer.gameObject.SetActive(false);
       }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    
}
