using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MilestoneAchievedUI : BaseUI {

    public static MilestoneAchievedUI Instance { get; private set; }


    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI achievedEXPText;
    [SerializeField] private Transform newUnlocksContainer;
    [SerializeField] private Transform newUnlockTemplate;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        newUnlockTemplate.gameObject.SetActive(false);

        //LevelingManager.Instance.OnMilestoneAchieved += LevelingManager_OnMilestoneAchieved;

        Hide();
    }


    private void LevelingManager_OnMilestoneAchieved(object sender, LevelingManager.OnMilestoneAchievedEventArgs e) {
        currentLevelText.text = "Level " + e.level;
        achievedEXPText.text = e.achievedExperienceAmount + "/" + e.achievedExperienceAmount;

        foreach (Transform child in newUnlocksContainer) {
            if (child == newUnlockTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (ItemSO itemSO in e.unlockedItems) {
            Transform newUnlockTransform = Instantiate(newUnlockTemplate, newUnlocksContainer);
            newUnlockTransform.gameObject.SetActive(true);
            newUnlockTransform.Find("Name").GetComponent<TextMeshProUGUI>().text = itemSO.nameString;
            newUnlockTransform.Find("Icon").GetComponent<Image>().sprite = itemSO.sprite;
            newUnlockTransform.Find("TypeText").GetComponent<TextMeshProUGUI>().text = "Contract Item";

            SingleNewUnlockUI singleNewUnlockUI = newUnlockTransform.GetComponent<SingleNewUnlockUI>();
            singleNewUnlockUI.SetItem(itemSO);
        }

        foreach (RecipeSO recipeSO in e.unlockedRecipes) {
            Transform newUnlockTransform = Instantiate(newUnlockTemplate, newUnlocksContainer);
            newUnlockTransform.gameObject.SetActive(true);
            newUnlockTransform.Find("Name").GetComponent<TextMeshProUGUI>().text = recipeSO.output[0].itemSO.nameString;
            newUnlockTransform.Find("Icon").GetComponent<Image>().sprite = recipeSO.output[0].itemSO.sprite;
            newUnlockTransform.Find("TypeText").GetComponent<TextMeshProUGUI>().text = "Recipe";

            SingleNewUnlockUI singleNewUnlockUI = newUnlockTransform.GetComponent<SingleNewUnlockUI>();
            singleNewUnlockUI.SetRecipe(recipeSO);
        }

        Show();
    }



}
