using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

[Serializable]
public class BuildingCategoryData  {

    public enum Category {
        Production,
        Automation,
        Storage,
        Power,
    }

    public static Sprite GetCategoryIconByBuildingType(PlacedObjectTypeSO buildingType) {
        return GetCategoryIcon(buildingType.categoryData.category);
    }

    public static Sprite GetCategoryIcon(Category category) {
        switch (category) {
            case Category.Production:
                return Resources.Load<Sprite>("ProductionIcon");
            case Category.Automation:
                return Resources.Load<Sprite>("AutomationIcon");
            case Category.Storage:
                return Resources.Load<Sprite>("StorageIcon");
            case Category.Power:
                return Resources.Load<Sprite>("PowerIcon");

        }
        return null;
    }

    public Category category;

}
