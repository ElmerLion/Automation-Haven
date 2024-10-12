using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ResearchNodeSO")]
public class ResearchNodeSO : ScriptableObject {

    public string nameString;
    [TextArea(3, 10)]
    public string description;
    public int researchCost;
    [Tooltip("0-4")] public int slotIndex;
    public ResearchManager.ResearchCategory researchCategory;
    public Sprite icon;
    [ES3NonSerializable] public List<PlacedObjectTypeSO> unlockedBuildings;
    [ES3NonSerializable] public List<RecipeSO> unlockedRecipes; 
    [ES3NonSerializable] public List<ResearchNodeSO> prerequisiteResearchList;

}
