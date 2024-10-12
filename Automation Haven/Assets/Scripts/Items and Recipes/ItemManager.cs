using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour {

    public static ItemManager Instance { get; private set; }

    [SerializeField] private List<ItemSO> startingUnlockedItems;
    [SerializeField] private List<Category> categories;

    private List<ItemSO> allItems;
    private List<ItemSO> unlockedItems;
    private Dictionary<int, List<ItemSO>> itemsInEachLevelDic;

    private void Awake() {
        Instance = this;

        allItems = new List<ItemSO>();
        unlockedItems = new List<ItemSO>();
        itemsInEachLevelDic = new Dictionary<int, List<ItemSO>>();

        

    }

    private void Start() {
        foreach (Category category in categories) {
            allItems.AddRange(category.GetItems());
        }

        unlockedItems.AddRange(startingUnlockedItems);

        RecipeManager.Instance.InitalizeRecipes();
    }

    public List<ItemSO> GetItemsInCategory(ItemSO.ItemCategory itemCategory) {
        foreach (Category category in categories) {
            if (category.category == itemCategory) {
                return category.GetItems();
            }
        }

        return new List<ItemSO>();
    }


    public Sprite GetIconByCategory(ItemSO.ItemCategory itemCategory) {
        foreach (Category category in categories) {
            if (category.category == itemCategory)
                return category.icon;
        }
        return null;
    }

    public void UnlockItemAndRecipe(RecipeSO recipeSO) {
        foreach (ItemAmount itemAmount in recipeSO.output) {
            if (!unlockedItems.Contains(itemAmount.itemSO)) {
                unlockedItems.Add(itemAmount.itemSO);
            }
        }

        RecipeManager.Instance.UnlockRecipe(recipeSO);
    }

    public List<Category> GetCategories() {
        return categories;
    }

    public List<ItemSO> GetAllItemsInGame() {
        return allItems;
    }

    public List<ItemSO> GetUnlockedItems() {
        return unlockedItems;
    }

    public bool TryUnlockItemsInLevel(int level) {
        if (!itemsInEachLevelDic.ContainsKey(level) || itemsInEachLevelDic[level].Count == 0) return false;
        
        foreach (ItemSO itemSO in itemsInEachLevelDic[level]) {
            unlockedItems.Add(itemSO);
        }
        return true;
    }

    public List<ItemSO> GetItemsInLevel(int level) {
        return itemsInEachLevelDic[level];
    }

    private void SaveItems() {
        ES3.Save("unlockedItems", unlockedItems);
        Debug.Log("Saved items");
    }

    private void LoadItems() {
        unlockedItems = ES3.Load("unlockedItems", unlockedItems);
        Debug.Log("Loaded items");
    }

    [System.Serializable]
    public class Category {
        public string name;
        public ItemSO.ItemCategory category;
        public Sprite icon;
        [SerializeField] private List<ItemSO> items;
        private List<RecipeSO> recipes;

        public Category(ItemSO.ItemCategory category, string name) {
            this.category = category;
            items = new List<ItemSO>();
            recipes = new List<RecipeSO>();
            this.name = name;
        }

        public void AddItem(ItemSO item) {
            if (items == null) { items = new List<ItemSO>(); }
            items.Add(item);
        }
        public void AddRecipe(RecipeSO recipe) {
            if (recipes == null) {  recipes = new List<RecipeSO>(); }
            recipes.Add(recipe);
        }
        public List<ItemSO> GetItems() {
            return items;
        }
        public List<RecipeSO> GetRecipes() {
            return recipes;
        }
    }

}
