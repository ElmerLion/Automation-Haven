using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecipeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RecipeSO recipe;
    private CraftingMachine craftingMachine;

    public void Initialize(RecipeSO recipeSO, CraftingMachine craftingMachine) {
        this.craftingMachine = craftingMachine;
        recipe = recipeSO;
        Button button = GetComponent<Button>();
        if (button != null) {
            button.onClick.AddListener(SelectRecipe);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        InterfaceToolTipUI.Instance.ShowRecipeToolTip(recipe);
    }

    public void OnPointerExit(PointerEventData eventData) {

        InterfaceToolTipUI.Instance.Hide();
        
    }

    private void SelectRecipe() {
        craftingMachine.SetActiveRecipe(recipe);
        CraftingMachineUI.Instance.Hide();
    }
}
