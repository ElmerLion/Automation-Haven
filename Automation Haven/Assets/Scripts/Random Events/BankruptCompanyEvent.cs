using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankruptCompanyEvent : Event {

    [Range(0, 100)] [SerializeField] private int fakeBankruptcyChance = 10;
    [SerializeField] private int minBankrupcyDays = 30;
    [SerializeField] private int maxBankrupcyDays = 90;
    [SerializeField] private int maxBankruptCompanies = 2;

    private Dictionary<Company, System.Action> companyEventHandlers = new Dictionary<Company, System.Action>();

    public override bool TryTriggerEvent() {
        if (CompanyManager.Instance.GetBankruptCompanies().Count >= maxBankruptCompanies) return false;

        List<Company> companies = CompanyManager.Instance.GetUnlockedCompanies();
        Company company = companies[Random.Range(0, companies.Count)];

        if (company.daysSinceLastContractCompleted < 30) return false;
        if (company.isPaused) return false;
        if (company.HasDevelopedNewProduct()) return false;

        string newEventDescription = StringUtility.ReplacePlaceholders(eventDescription, company);
        string newEventName = StringUtility.ReplacePlaceholders(eventName, company);

        CompanyManager.Instance.BankruptCompany(company);

        newEventDescription += "\n\nThe following items have been affected:";
        foreach (ItemSO itemSO in company.contractItems) {
            if (Random.Range(0, 2) == 0) continue;
            MarketManager.Instance.TriggerPriceFluctuationForItem(itemSO, Random.Range(-itemSO.price / 10, -itemSO.price / 8), false, out string message, out EventType eventType);
            newEventDescription += $"\n{message}";
        }

        MessageBarUI.Instance.CreateMessage(newEventName, newEventDescription, eventType);

        HandleFakeBankruptcy(company);

        return base.TryTriggerEvent();
    }

    private void HandleFakeBankruptcy(Company company) {
        if (Random.Range(0, fakeBankruptcyChance) == 0) {
            float fakeBankruptcyDuration = Random.Range(minBankrupcyDays, maxBankrupcyDays);
            float fakeBankruptcyTime = fakeBankruptcyDuration;

            System.Action handler = null;
            handler = () => {
                fakeBankruptcyTime--;

                if (fakeBankruptcyTime <= 0) {
                    CompanyManager.Instance.UnBankruptCompany(company);

                    string newEventDescription = $"{company.companyName} has made a sudden reentry into the market, coming back from their previous bankruptcy! You can now receive contracts again from them.";
                    string newEventName = $"{company.companyName} Reentry";
                    MessageBarUI.Instance.CreateMessage(newEventName, newEventDescription, EventType.Positive);

                    // Remove the handler
                    TimeManager.Instance.OnDayChanged -= handler;
                    companyEventHandlers.Remove(company);
                }
            };

            // Ensure the handler is registered only once
            if (!companyEventHandlers.ContainsKey(company)) {
                TimeManager.Instance.OnDayChanged += handler;
                companyEventHandlers[company] = handler;
            }
        }
    }

}
