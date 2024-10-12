using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour {

    public static RecipeManager Instance { get; private set; }

    [SerializeField] private List<RecipeSO> allRecipes;

    private Dictionary<int, List<RecipeSO>> recipesInEachLevelDic;
    private List<RecipeSO> unlockedRecipes;

    private void Awake() {
        Instance = this;
        unlockedRecipes = new List<RecipeSO>();
        recipesInEachLevelDic = new Dictionary<int, List<RecipeSO>>();

    }

    public void InitalizeRecipes() {

        foreach (ItemManager.Category category in ItemManager.Instance.GetCategories()) {
            foreach (RecipeSO recipeSO in allRecipes) {
                if (category.category == recipeSO.output[0].itemSO.itemCategory) {
                    category.AddRecipe(recipeSO);
                }
            }

        }

        foreach (ItemSO itemSO in ItemManager.Instance.GetUnlockedItems()) {
            foreach (RecipeSO recipeSO in allRecipes) {
                if (recipeSO.output[0].itemSO == itemSO) {
                    unlockedRecipes.Add(recipeSO);
                }
            }
        }
    }

    public void UnlockRecipe(RecipeSO recipeSO) {
        unlockedRecipes.Add(recipeSO);
    }

    public void UnlockRecipeByItem(ItemSO itemSO) {
        foreach (RecipeSO recipeSO in allRecipes) {
            if (recipeSO.output[0].itemSO == itemSO) {
                if (!unlockedRecipes.Contains(recipeSO)) {
                    unlockedRecipes.Add(recipeSO);
                }
            }
        }
    }

    public bool TryUnlockRecipesInLevel(int level) {
        if (!recipesInEachLevelDic.ContainsKey(level) || recipesInEachLevelDic[level].Count == 0) return false;

        foreach (RecipeSO recipeSO in recipesInEachLevelDic[level]) {
            unlockedRecipes.Add(recipeSO);
        }
        return true;
    }

    public List<RecipeSO> GetRecipesInLevel(int level) {
        return recipesInEachLevelDic[level];
    }

    public List<RecipeSO> GetAllRecipes() {
        return allRecipes;
    }

    public List<RecipeSO> GetUnlockedRecipes() {
        return unlockedRecipes;
    }

    public RecipeSO GetRecipeSOForItem(ItemSO itemSO) {
        return null;
    }



}
