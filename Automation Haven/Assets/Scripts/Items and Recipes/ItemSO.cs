using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ItemSO")]
public class ItemSO : ScriptableObject {

    [Header("Item Settings")]
    public string nameString;
    public bool isLiquid;

    [Tooltip("The chemical symbol of the item, if there is one.")]
    public string chemicalSymbol;

    [Tooltip("The prefab that will be spawned when the item is spawned and moved.")]
    public Transform prefab;
    public Sprite sprite;

    [Tooltip("The higher the number, the less of that item will be needed in contracts.")]
    [Range(0, 100)] public float rarity;
    [Tooltip("How much space the item takes up in a box.")]
    public float weight;
    public int price = 10;
    public int stackSize = 100;


    public ItemCategory itemCategory;

    //
    [Header("Resource Node Settings")]
    public GameObject resourceNodePrefab;

    public enum  ItemCategory {
        
        Weapons,
        Components,
        RawResources,
        Ingots,
        TechProducts,
        Manufactured,
        Furniture,
        VehicleParts,

    }

}
