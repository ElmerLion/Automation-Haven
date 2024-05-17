
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ContractManager : MonoBehaviour {

    // Delivery point selectar man vilket contract den ska delivera till
    // Fixa så man kan delivera till olika delivery points

    public static ContractManager Instance { get; private set; }

    public event Action<Contract> OnContractCompleted;
    public event Action<Contract> OnContractAccepted;
    public event Action<Contract> OnNewContractCreated;
    public event Action OnShowContractSelection;

    [Header("Contract Data")]
    [SerializeField] private List<CompanySO> possibleCompanies;

    [Header("Item Amounts")]
    [SerializeField] private int minItemAmount = 1;
    [SerializeField] private int maxItemAmount = 100;

    private List<Contract> activeContracts;
    private float maxActiveContracts = 6f;
    private int generatedContracts = 0;
    private int maxGeneratedContracts = 3;
    private int completedContracts = 0; // Använd för att kalkylera contract difficulty

    private int baseContractTime = 10;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameLoaded += LoadActiveContracts;
        SaveManager.OnGameSaved += SaveActiveContracts;
    }

    public void GenerateMaxContracts() {
        generatedContracts = 0;
        for (int i = 0; i < maxGeneratedContracts; i++) {
            GenerateContract();
        }
    }

    private void GenerateContract() {
        if (activeContracts.Count >= possibleCompanies.Count || activeContracts.Count >= maxActiveContracts) {
            Debug.LogWarning("Used up all available companies for contracts."); 
            return;
        }

        int amountOfItemsContractDifficulty = 1 + LevelingManager.Instance.Level / 2;
        int itemAmountDifficulty = 1 + completedContracts / 2;

        CompanySO companySO = ReputationManager.Instance.GetRandomCompanyBasedOnPlayerReputation(possibleCompanies);
        foreach (Contract activeContract in activeContracts) {
            if (activeContract.companySO == companySO) {
                GenerateContract();
                return;
            }
        }

        int reputationPenaltyCompany = ReputationManager.Instance.GetReputationPenaltyPlayerToCompany(companySO);
        int reputationRewardCompany = ReputationManager.Instance.GetReputationRewardPlayerToCompany(companySO);
        

        Contract contract = new Contract(companySO, reputationPenaltyCompany, reputationRewardCompany);

        for (int i = 0; i < amountOfItemsContractDifficulty; i++) {
            int randomItemIndex = UnityEngine.Random.Range(0, companySO.possibleNeededItems.Count);
            ItemSO randomItem = companySO.possibleNeededItems[randomItemIndex];

            float divide = 2 + randomItem.rarity;
            int randomAmount = Mathf.RoundToInt(UnityEngine.Random.Range(minItemAmount, maxItemAmount) * itemAmountDifficulty / divide);

            ItemAmount itemAmount = new ItemAmount(randomItem, randomAmount);
            contract.AddNeededItemAmount(itemAmount);
        }

        int time = CalculateContractTime(contract);
        contract.UpdateTime(time);

        contract.UpdateReward(companySO.playerRewardMultiplier);

        OnNewContractCreated?.Invoke(contract);

        generatedContracts++;
        if (generatedContracts == maxGeneratedContracts) {
            OnShowContractSelection?.Invoke();
        }
    }

    public void DeliverItemsToContract(Contract contract, ItemAmount itemAmount, out ItemAmount excessItemAmount) {
        ItemAmount exisitingItem = contract.neededItemAmount.Find(item => item.itemSO == itemAmount.itemSO);
        excessItemAmount = null;
        if (exisitingItem != null) {

            exisitingItem.amount -= itemAmount.amount;
            contract.InvokeProgressChangedEvent(contract);

            if (exisitingItem.amount <= 0) {
                if (exisitingItem.amount < 0) {
                    excessItemAmount = new ItemAmount(exisitingItem.itemSO, Mathf.Abs(exisitingItem.amount));
                }
                contract.neededItemAmount.Remove(exisitingItem);
                contract.InvokeProgressChangedEvent(contract);

                if (contract.neededItemAmount.Count == 0) {
                    CompleteContract(contract);
                }
            }
        }
    }

    public void DeliverItemsToContract(Contract contract, ItemSO itemSO, int amount) {
        ItemAmount exisitingItem = contract.neededItemAmount.Find(item => item.itemSO == itemSO);
        if (exisitingItem != null) {

            exisitingItem.amount -= amount;
            contract.InvokeProgressChangedEvent(contract);

            if (exisitingItem.amount <= 0) {
                contract.neededItemAmount.Remove(exisitingItem);
                contract.InvokeProgressChangedEvent(contract);

                if (contract.neededItemAmount.Count == 0) {
                    CompleteContract(contract);
                }
            }
        }
    }

    public bool DoesContractNeedItemSO(Contract contract, ItemSO itemSO) {
        foreach (ItemAmount itemAmount in contract.neededItemAmount) {
            if (itemAmount.itemSO == itemSO) {
                return true;
            }
        }
        return false;
    }

    public void AcceptContract(Contract contract) {
        generatedContracts = 0;
        activeContracts.Add(contract);
        OnContractAccepted?.Invoke(contract);
        Debug.Log("Contract Accepted " + contract.companySO.companyName + " " + contract.reward);
    }

    private void CompleteContract(Contract contract) {
        activeContracts.Remove(contract);
        contract.isCompleted = true;

        completedContracts++;

        OnContractCompleted?.Invoke(contract);
        Debug.Log("Contract Completed");
    }

    private int CalculateContractTime(Contract contract) {
        float time = baseContractTime;

        int totalItemsTime = 0;
        foreach (ItemAmount itemAmount in contract.neededItemAmount) {
            totalItemsTime += Mathf.RoundToInt(itemAmount.amount * itemAmount.itemSO.rarity);
        }
        time += totalItemsTime / 2;

        float companyTimeModifier = contract.companySO.timeModifier;
        time *= companyTimeModifier;

        float playerProgressScaling = GetPlayerProgressScaling();
        time *= playerProgressScaling;

        float timeVariationRange = LevelingManager.Instance.Level;

        float randomTimeVariation = UnityEngine.Random.Range(-timeVariationRange, timeVariationRange);
        time += randomTimeVariation;

        return Mathf.RoundToInt(time);
    }

    private float GetPlayerProgressScaling() {
        float playerProgressScaling = 1f / (1 + LevelingManager.Instance.Level / 10);
        return playerProgressScaling;
    }


    public int GetGeneratedContractsAmount() {
        return generatedContracts;
    }
    public int GetMaxGeneratedContractsAmount() {
        return maxGeneratedContracts;
    }

    private void SaveActiveContracts(string filePath) {
        ES3.Save("activeContracts", activeContracts, filePath);
    }

    private void LoadActiveContracts(string filePath) {
        possibleCompanies = NewWorldManager.Instance.GetStartingCompanySOs();
        activeContracts = ES3.Load("activeContracts", filePath, new List<Contract>());
    }

    public void SetPossibleCompanies(List<CompanySO> companySOList) {
        possibleCompanies = companySOList;
    }


    public class Contract {
        public event System.Action<Contract> OnContractProgressChanged;

        public CompanySO companySO;
        public int reward;
        public int time;
        public int timeLeft;
        public int reputationPenalty;
        public bool isCompleted;
        public bool isFailed;
        public int reputationReward;

        public List<ItemAmount> neededItemAmount;

        public Contract(CompanySO company, int reputationPenalty, int reputationReward) {
            this.companySO = company;
            this.timeLeft = time;
            this.reputationPenalty = reputationPenalty;
            this.isCompleted = false;
            this.isFailed = false;
            this.neededItemAmount = new List<ItemAmount>();
            this.reputationReward = reputationReward;
        }

        public void AddNeededItemAmount(ItemAmount itemAmountToAdd) {
            ItemAmount existingItemAmount = neededItemAmount.Find(item => item.itemSO == itemAmountToAdd.itemSO);

            if (existingItemAmount != null) {
                existingItemAmount.amount += itemAmountToAdd.amount;
            } else {
                neededItemAmount.Add(itemAmountToAdd);
            }
        }

        public void UpdateReward(float playerRewardMultiplier) {
            int newReward = 0;
            foreach (ItemAmount itemAmount in neededItemAmount) {
                newReward += MarketManager.Instance.GetItemPrice(itemAmount.itemSO) * itemAmount.amount;
            }
            newReward = Mathf.RoundToInt(newReward  * playerRewardMultiplier);
            reward = newReward;
        }

        public void UpdateTime(int newTime) {
            time = newTime;
            timeLeft = time;
        }

        public void InvokeProgressChangedEvent(Contract contract) {
            OnContractProgressChanged?.Invoke(contract);
        }
    } 

    public List<Contract> GetActiveContracts() {
        return activeContracts;
    }

    

}
