using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RecipeSO")]

public class RecipeSO : ScriptableObject {

    public string nameString;
    public float craftingTime;
    public CraftingMachine.Type craftingMachineType;
    public List<ItemAmount> input;
    public List<ItemAmount> output;

}
