using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RecipeListSO")]
public class RecipeListSO : ScriptableObject {

    public List<RecipeSO> list;

}