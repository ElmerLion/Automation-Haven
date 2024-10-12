using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class WarningIconsUI : MonoBehaviour {

    public enum WarningType {
        PowerNeeded,
        NoRecipe,
        NoResourceNodes,
    }

    public class WarningIconsData {
        public WarningType warningType;
        public Sprite sprite;
    }

    [SerializeField] private GameObject warningIconPrefab;
    [SerializeField] private List<Sprite> warningSprites;
    [SerializeField] private List<WarningType> validWarningTypes;



    private void Start() {
        GridLayoutGroup gridLayoutGroup = GetComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(1, 0.65f);
        gridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerLeft;
        gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;

        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        foreach (WarningType warningType in validWarningTypes) {
            GameObject warningIcon = Instantiate(warningIconPrefab, transform);

            warningIcon.GetComponent<SingleWarningIcon>().SetWarningType(warningType, warningSprites[(int)warningType]);
        }
    }

}
