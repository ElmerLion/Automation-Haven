using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemParent {

    public List<ItemObject> GetItems();
    public void AddItem(ItemObject item, Vector3 spawnPosition = default);
    public void RemoveItem(ItemObject item);
    public void ClearItems();
    public bool HasItem(ItemObject item);
}
