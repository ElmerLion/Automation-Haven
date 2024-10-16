using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlacedObjectTypeSO")]
public class PlacedObjectTypeSO : ScriptableObject {

    public static Dir GetNextDir(Dir dir) {
        switch (dir) {
            default:
            case Dir.Down: return Dir.Left;
            case Dir.Left: return Dir.Up;
            case Dir.Up: return Dir.Right;
            case Dir.Right: return Dir.Down;
        }
    }

    public enum Dir {
        Down,
        Left,
        Up,
        Right,
    }

    [Header("Grid and Building")]
    public string nameString;
    public int width;
    public int height;
    public bool hasInput;
    public bool hasOutput;

    [Space(10)]

    [Header("Building Visuals")]
    public Sprite buildingSprite;
    public Transform prefab;
    public Transform visual;

    [Space(10)]
    [Header("Building Specific Data")]
    //public List<InventoryConfig> inventoryConfigs;
    public CraftingMachine.Type craftingMachineType;
    public bool hasSpecificInputPoint;
    public float tierMultiplier;
    public float range;

    [Space(10)]
    public StorageData storageData;
    public List<BuildingUpgradeSO> validBuildingUpgrades;

    [Space(10)]
    [Header("Building Data")]
    public BuildingCategoryData categoryData;
    public BuildingCostList buildingCostList;

    [Space(10)]
    public PowerData powerData;

    [Space(10)]
    [Header("MinerData")]
    public ResourceGeneratorData resourceGeneratorData;
    

    public class InventoryConfig {
        public string name;
        public int inventorySlots;
        public int slotsPerItem;
    }

    public int GetRotationAngle(Dir dir) {
        switch (dir) {
            default:
            case Dir.Down: return 0;
            case Dir.Left: return 90;
            case Dir.Up: return 180;
            case Dir.Right: return 270;
        }
    }

    public Vector2Int GetRotationOffset(Dir dir) {
        switch (dir) {
            default:
            case Dir.Down: return new Vector2Int(0, 0);
            case Dir.Left: return new Vector2Int(0, width);
            case Dir.Up: return new Vector2Int(width, height);
            case Dir.Right: return new Vector2Int(height, 0);
        }
    }

    public List<Vector2Int> GetGridPositionList(Vector2Int offset, Dir dir) {
        List<Vector2Int> gridPositionList = new List<Vector2Int>();
        switch (dir) {
            default:
            case Dir.Down:
            case Dir.Up:
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
            case Dir.Left:
            case Dir.Right:
                for (int x = 0; x < height; x++) {
                    for (int y = 0; y < width; y++) {
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
        }
        return gridPositionList;
    }

}