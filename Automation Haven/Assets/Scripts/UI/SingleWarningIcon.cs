using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using UnityEngine.ProBuilder.Shapes;

[RequireComponent(typeof(Animator))]
public class SingleWarningIcon : MonoBehaviour {

    // Den sparas som hidden?

    [SerializeField] private RuntimeAnimatorController animatorController;

    [SerializeField] private WarningIconsUI.WarningType warningType;

    private Animator animator;
    private Image icon;

    private CraftingMachine craftingMachine;
    private PowerReciever powerReciever;
    private ResourceGenerator resourceGenerator;

    private void Start() {
        transform.SetParent(transform.parent, false);
    }

    public void SetWarningType(WarningIconsUI.WarningType warningType, UnityEngine.Sprite sprite) {
        this.warningType = warningType;

        icon = transform.Find("Icon").GetComponent<Image>();

        icon.sprite = sprite;

        HideWarning();

        switch (warningType) {
            case WarningIconsUI.WarningType.PowerNeeded:
                powerReciever = transform.parent.parent.GetComponent<PowerReciever>();
                if (powerReciever == null) Debug.LogWarning("PowerReciever is null");

                powerReciever.OnPowerStatusChanged += PowerReciever_OnPowerStatusChanged;

                powerReciever.CheckPowerStatus();

                break;
            case WarningIconsUI.WarningType.NoRecipe:
                craftingMachine = transform.parent.parent.GetComponent<CraftingMachine>();
                if (craftingMachine == null) Debug.LogWarning("CraftingMachine is null");

                craftingMachine.OnActiveRecipeNull += CraftingMachine_OnActiveRecipeNull;
                craftingMachine.OnActiveRecipeChanged += CraftingMachine_OnActiveRecipeChanged;

                craftingMachine.CheckActiveRecipeSOStatus();
                break;
            case WarningIconsUI.WarningType.NoResourceNodes:
                resourceGenerator = transform.parent.parent.GetComponent<ResourceGenerator>();
                if (resourceGenerator == null) Debug.LogWarning("ResourceGenerator is null");

                resourceGenerator.OnNoResourceNodesNearby += ResourceGenerator_OnNoResourceNodesNearby;

                resourceGenerator.CheckResourceNearbyStatus();
                break;
        }

        LoadAnimator();
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

        animator.runtimeAnimatorController = animatorController;
    }

    private void OnDestroy() {
        switch (warningType) {
            case WarningIconsUI.WarningType.PowerNeeded:
                if (powerReciever == null) return;
                powerReciever.OnPowerStatusChanged -= PowerReciever_OnPowerStatusChanged;

                break;
            case WarningIconsUI.WarningType.NoRecipe:
                if (craftingMachine == null) return;
                craftingMachine.OnActiveRecipeNull -= CraftingMachine_OnActiveRecipeNull;
                craftingMachine.OnActiveRecipeChanged -= CraftingMachine_OnActiveRecipeChanged;
                break;
            case WarningIconsUI.WarningType.NoResourceNodes:
                if (resourceGenerator == null) return;
                resourceGenerator.OnNoResourceNodesNearby -= ResourceGenerator_OnNoResourceNodesNearby;
                break;
        }

    }

}
