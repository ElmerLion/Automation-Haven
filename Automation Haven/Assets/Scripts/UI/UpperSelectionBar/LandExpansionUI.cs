using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandExpansionUI : BaseUI {

    public static LandExpansionUI Instance { get; private set; }

    [SerializeField] private Transform landContainer;
    [SerializeField] private Transform landTemplate;
    [SerializeField] private Transform uiBackground;

    private List<PlotUI> landUIList;

    private void Awake() {
        Instance = this;
    }

    public void SetupPlotUI(List<Plot> plots, int widthConstraint) {
        landUIList = new List<PlotUI>();
        landTemplate.gameObject.SetActive(false);

        landContainer.GetComponent<GridLayoutGroup>().constraintCount = widthConstraint;

        foreach (Plot plot in plots) {
            Transform landTransform = Instantiate(landTemplate, landContainer);

            Button button = landTransform.Find("LandButton").GetComponent<Button>();

            switch (plot.state) {
                case Plot.State.NotUnlocked:
                    Debug.Log("Setting " + plot.gameObject.name + " to not unlocked.");
                    button.interactable = false;
                    button.GetComponent<Image>().color = Color.gray;
                    break;
                case Plot.State.CanBePurchased:
                    Debug.Log("Setting " + plot.gameObject.name + " to can be purchased.");
                    button.interactable = true;
                    button.GetComponent<Image>().color = Color.green;
                    break;
                case Plot.State.Purchased:
                    Debug.Log("Setting " + plot.gameObject.name + " to purchased.");
                    button.interactable = false;
                    button.GetComponent<Image>().color = Color.red;
                    break;
            }

            button.onClick.AddListener(() => HandleLandButtonClicked(plot));

            PlotUI plotUI = new PlotUI(plot, landTransform, button);
            landUIList.Add(plotUI);

            landTransform.gameObject.SetActive(true);
        }

        Plot.OnPlotPurchased += Land_OnLandPurchased;

        Hide();
    }

    private void Land_OnLandPurchased(Plot obj) {
        PlotUI landUI = null;
        foreach (PlotUI landUIItem in landUIList) {
            if (landUIItem.land == obj) {
                landUI = landUIItem;
                break;
            }
        }

        if (landUI == null) return;

        Transform buttonTransform = landUI.button.transform;

        landUI.button.interactable = false;
        buttonTransform.GetComponent<Image>().color = Color.red;
    }

    private void HandleLandButtonClicked(Plot land) {
        if (land.state == Plot.State.CanBePurchased) {
            land.PurchasePlot();
        }
    }
    
    public class PlotUI {
        public Plot land;
        public Transform transform;
        public Button button;

        public PlotUI(Plot plot, Transform transform, Button button) {
            this.land = plot;
            this.transform = transform;
            this.button = button;
        }
    }

}
