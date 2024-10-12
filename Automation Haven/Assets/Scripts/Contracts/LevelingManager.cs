using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelingManager : MonoBehaviour {

    public static LevelingManager Instance { get; private set; }

    public class OnMilestoneAchievedEventArgs : EventArgs {
        public int level;
        public int achievedExperienceAmount;
        public List<ItemSO> unlockedItems;
        public List<RecipeSO> unlockedRecipes;
    }

    //public event EventHandler<OnMilestoneAchievedEventArgs> OnMilestoneAchieved;

    public int Level { get; private set; }
    public int CurrentExperience { get; private set; }
    public int ExperienceToNextLevel { get; private set; }

    private float experienceMultiplier = 1.25f;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Level = 1;
        ExperienceToNextLevel = 100;

        ContractManager.Instance.OnContractCompleted += ContractManager_OnContractCompleted;
    }

    private void ContractManager_OnContractCompleted(ContractManager.Contract obj) {
        AddExperience(obj.reward);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            AddExperience(100);
        }
    }

    public void AddExperience(int amount) {
        CurrentExperience += amount;
        if (CurrentExperience >= ExperienceToNextLevel) {
            LevelUp();
        }
    }

    private void LevelUp() {
        Level++;
        CurrentExperience -= ExperienceToNextLevel;
        int achievedExperienceAmount = ExperienceToNextLevel;

        ExperienceToNextLevel = (int)(ExperienceToNextLevel * experienceMultiplier);

        List<ItemSO> unlockedItems = new List<ItemSO>();
        List<RecipeSO> unlockedRecipes = new List<RecipeSO>();

        /*if (RecipeManager.Instance.TryUnlockRecipesInLevel(Level)) {
            unlockedRecipes = RecipeManager.Instance.GetRecipesInLevel(Level);
        }
        if (ItemManager.Instance.TryUnlockItemsInLevel(Level)) {
            unlockedItems = ItemManager.Instance.GetItemsInLevel(Level);
        }

        if (unlockedItems.Count > 0 || unlockedRecipes.Count > 0) {
            OnMilestoneAchieved?.Invoke(this, new OnMilestoneAchievedEventArgs { 
                level = Level, 
                achievedExperienceAmount = achievedExperienceAmount,
                unlockedItems = unlockedItems,
                unlockedRecipes = unlockedRecipes 
            });
        }*/

        Debug.Log("Level Up!");
        Debug.Log("Experience To Next Level: " + ExperienceToNextLevel);

        if (CurrentExperience >= ExperienceToNextLevel) {
            LevelUp();
        } 
    }
    
}
