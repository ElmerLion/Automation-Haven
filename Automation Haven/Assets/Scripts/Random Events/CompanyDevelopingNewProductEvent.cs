using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanyDevelopingNewProductEvent : Event {

    private bool isActive;
    private Dictionary<Company, System.Action> companyEventHandlers = new Dictionary<Company, System.Action>();

    public override bool TryTriggerEvent() {
        if (isActive) {
            return false;
        }

        List<Company> companies = CompanyManager.Instance.GetUnlockedCompanies();
        Company company = companies[Random.Range(0, companies.Count)];

        isActive = true;
        CompanyManager.Instance.PauseCompany(company);

        string message = StringUtility.ReplacePlaceholders(eventDescription, company);
        MessageBarUI.Instance.CreateMessage(eventName, message, eventType);

        HandleProductDevelopment(company);

        return base.TryTriggerEvent();
    }

    private void HandleProductDevelopment(Company company) {
        System.Action handler = null;
        handler = () => {
            if (isActive) {
                company.productDevelopmentProgress += Random.Range(1, 3);

                if (company.productDevelopmentProgress >= 100) {
                    company.productDevelopmentProgress = 0;
                    company.productDevelopmentCost = 0;
                    company.productDevelopmentTime = 0;

                    ItemSO newProduct = company.companySO.unlockableProducts[Random.Range(0, company.companySO.unlockableProducts.Count)];
                    company.AddContractItem(newProduct);

                    CompanyManager.Instance.UnPauseCompany(company);

                    string newMessage = $"{company.companyName} has developed a new product and will now request it in contracts. This is an exclusive product that is usually worth more than other products.\n\n{newProduct.nameString}";
                    newMessage = StringUtility.ReplacePlaceholders(newMessage, company);

                    MessageBarUI.Instance.CreateMessage($"{company.companyName} New Product", newMessage, EventType.Positive);

                    isActive = false;

                    // Remove the handler
                    TimeManager.Instance.OnDayChanged -= handler;
                    companyEventHandlers.Remove(company);
                }
            }
        };

        // Ensure the handler is registered only once
        if (!companyEventHandlers.ContainsKey(company)) {
            TimeManager.Instance.OnDayChanged += handler;
            companyEventHandlers[company] = handler;
        }
    }

}
