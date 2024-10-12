using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExclusiveContractEvent : Event {

    [SerializeField] private CompanySO unknownCompanySO;

    private bool isActive;

    public override bool TryTriggerEvent() {

        if (isActive) return false;
        if (ContractManager.Instance.GetActiveContracts().Count >= ContractManager.Instance.GetMaxGeneratedContractsAmount()) return false;

        ItemSO requestedItem = unknownCompanySO.startingContractItems[Random.Range(0, unknownCompanySO.startingContractItems.Count)];
        int requestedAmount = (int)Random.Range(50, 400) / (int)requestedItem.rarity;

        List<ItemAmount> amounts = new List<ItemAmount> {
            new ItemAmount(requestedItem, requestedAmount)
        };

        Company company = CompanyManager.Instance.GetCompany(unknownCompanySO);

        ContractManager.Contract contract = ContractManager.Instance.AddContract(company, amounts, 0, 0);

        string newEventDescription = eventDescription + "\n\n" + requestedAmount + " " + requestedItem.nameString;

        MessageBarUI.Instance.CreateMessage(eventName, newEventDescription, eventType);

        isActive = true;

        ContractManager.Instance.OnContractCompleted += (completedContract) => {
            if (completedContract == contract) {
                isActive = false;
            }
        };
        

        return base.TryTriggerEvent();

    }

}
