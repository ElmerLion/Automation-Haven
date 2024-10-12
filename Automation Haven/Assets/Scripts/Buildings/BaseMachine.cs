using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMachine : MonoBehaviour, ICanBeClicked {

    protected const string INVENTORY_NAME_INPUT = "Input";
    protected const string INVENTORY_NAME_OUTPUT = "Output";

    protected PlacedObjectTypeSO buildingTypeSO;
    protected Dictionary<string, Inventory> nameInventoryDic;
    protected PowerReciever powerReciever;

    public virtual void OnClick() {
        Debug.LogWarning("BaseMachine OnClick");
    }

    public virtual void Awake() {
        buildingTypeSO = transform.GetComponent<BuildingTypeHolder>().buildingType;
        powerReciever = transform.GetComponent<PowerReciever>();
    }

    public virtual void Start() {
        if (nameInventoryDic == null) {
            nameInventoryDic = new Dictionary<string, Inventory>();
            //foreach (PlacedObjectTypeSO.InventoryConfig inventoryConfig in buildingTypeSO.inventoryConfigs) {
                //nameInventoryDic.Add(inventoryConfig.name, new Inventory(inventoryConfig.slotsPerItem));
            //}
        }
    }

    public Inventory GetInventory(string inventoryName) {
        if (nameInventoryDic.ContainsKey(inventoryName)) {
            return nameInventoryDic[inventoryName];
        }
        return null;
    }

    public List<Inventory> GetInventories() {
        List<Inventory> inventories = new List<Inventory>();
        foreach (KeyValuePair<string, Inventory> keyValuePair in nameInventoryDic) {
            inventories.Add(keyValuePair.Value);
        }
        return inventories;
    }

    public bool TryAddItemObjectToInventory(string inventoryName, ItemObject itemObject) {
        if (nameInventoryDic.ContainsKey(inventoryName)) {
            return nameInventoryDic[inventoryName].TryAddItemObject(itemObject);
        }
        Debug.LogError("Inventory Not Found " + inventoryName + " For " + name);
        return false;
    }
}
