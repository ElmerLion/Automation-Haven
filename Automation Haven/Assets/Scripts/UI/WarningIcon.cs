using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningIcon : MonoBehaviour {

    // Den sparas som hidden?

    public enum WarningType {
        PowerNeeded,
        NoRecipe,
        NoResourceNodes,
    }


    [SerializeField] private WarningType warningType;

    private Animator animator;

    private CraftingMachine craftingMachine;
    private PowerReciever powerReciever;
    private ResourceGenerator resourceGenerator;

    private void Awake() {

        switch (warningType) {
            case WarningType.PowerNeeded:
                powerReciever = transform.parent.parent.GetComponent<PowerReciever>();
                powerReciever.OnPowerStatusChanged += PowerReciever_OnPowerStatusChanged;
                Debug.Log("Found PowerNeeded");

                break;
            case WarningType.NoRecipe:
                craftingMachine = transform.parent.parent.GetComponent<CraftingMachine>();
                craftingMachine.OnActiveRecipeNull += CraftingMachine_OnActiveRecipeNull;
                craftingMachine.OnActiveRecipeChanged += CraftingMachine_OnActiveRecipeChanged;
                Debug.Log("Found NoRecipe");
                break;
            case WarningType.NoResourceNodes:
                resourceGenerator = transform.parent.parent.GetComponent<ResourceGenerator>();
                resourceGenerator.OnNoResourceNodesNearby += ResourceGenerator_OnNoResourceNodesNearby;
                Debug.Log("Found NoResourceNodes");
                break;
        }

        HideWarning();

    }

    private void ResourceGenerator_OnNoResourceNodesNearby(object sender, System.EventArgs e) {
        ShowWarning();
    }

    private void PowerReciever_OnPowerStatusChanged(object sender, System.EventArgs e) {
        if (powerReciever.IsPowerNeeded()) {
            ShowWarning();
        } else {
            HideWarning();
        }
    }

    private void CraftingMachine_OnActiveRecipeChanged(object sender, System.EventArgs e) {
        HideWarning();
    }

    private void CraftingMachine_OnActiveRecipeNull(object sender, System.EventArgs e) {
        ShowWarning();
    }


    public void ShowWarning() {
        gameObject.SetActive(true);
    }

    public void HideWarning() {
        gameObject.SetActive(false);
    }

    private void LoadAnimator() {
        animator = GetComponent<Animator>();
    }

    private void OnDestroy() {
        switch (warningType) {
            case WarningType.PowerNeeded:
                powerReciever.OnPowerStatusChanged -= PowerReciever_OnPowerStatusChanged;

                break;
            case WarningType.NoRecipe:
                craftingMachine.OnActiveRecipeNull -= CraftingMachine_OnActiveRecipeNull;
                craftingMachine.OnActiveRecipeChanged -= CraftingMachine_OnActiveRecipeChanged;
                break;
            case WarningType.NoResourceNodes:
                resourceGenerator.OnNoResourceNodesNearby -= ResourceGenerator_OnNoResourceNodesNearby;
                break;
        }

    }

}
