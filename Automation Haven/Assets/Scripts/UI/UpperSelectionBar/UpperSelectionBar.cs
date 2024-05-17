using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpperSelectionBar : MonoBehaviour {

    [SerializeField] private Button openResearchButton;
    [SerializeField] private Button openContractSelectionButton;
    [SerializeField] private Button openActiveContractsButton;

    private void Start() {
        openResearchButton.onClick.AddListener(OpenResearch);
        openContractSelectionButton.onClick.AddListener(OpenContractSelection);
        openActiveContractsButton.onClick.AddListener(OpenActiveContracts);
    }

    private void OpenResearch() {
        ResearchTreeUI.Instance.Show();
    }

    private void OpenContractSelection() {
        ContractManager.Instance.GenerateMaxContracts();
    }

    private void OpenActiveContracts() {
        ActiveContractsDisplayUI.Instance.ShowExpandedContracts();
    }
    
}
