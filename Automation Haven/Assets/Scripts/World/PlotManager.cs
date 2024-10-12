using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlotManager : MonoBehaviour {

    public static PlotManager Instance { get; private set; }

    [Header("Plot Settings")]
    [SerializeField] private Vector3 startingOffset;
    [SerializeField] private int plotSize;
    [SerializeField] private int plotAmountWidth;
    [SerializeField] private int plotAmountHeight;
    [SerializeField] private GameObject plotPrefab;
    [SerializeField] private GameObject plotContainer;
    [SerializeField] private GameObject floorPivot;

    [Header("World Borders")]
    [SerializeField] private GameObject positiveZBorder;
    [SerializeField] private GameObject negativeZBorder;
    [SerializeField] private GameObject positiveXBorder;
    [SerializeField] private GameObject negativeXBorder;
    [SerializeField] private GameObject yBorder;

    private List<GameObject> plotGameObjects;
    private Plot[,] plots;

    private void Awake() {
        Instance = this;

        GridBuildingSystem.Instance.SetupGrid(plotSize * plotAmountWidth, plotSize * plotAmountHeight);
    }

    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;

        floorPivot.transform.localScale = new Vector3(plotAmountWidth * plotSize, 0, plotAmountHeight * plotSize);

        LandExpansionUI.Instance.Hide();
    }

    private void SaveManager_OnGameSaved(string obj) {
        Debug.Log("Saving plot game objects: " + plotGameObjects.Count);
        ES3.Save("plotGameObjects", plotGameObjects, obj);
    }

    private void SaveManager_OnGameLoaded(string obj) {
        plotGameObjects = ES3.Load("plotGameObjects", obj, new List<GameObject>());
        plots = new Plot[plotAmountWidth, plotAmountHeight];

        Debug.Log("Loaded plot game objects: " + plotGameObjects.Count);

        if (plotGameObjects.Count <= 0) {
            GeneratePlots();
            AssignNeighbors();

            int middleX = plots.GetLength(0) / 2;
            int middleY = plots.GetLength(1) / 2;
            
            Plot middlePlot = plots[middleX, middleY];
            middlePlot.ForcePurchasePlot();

            CameraMovement.Instance.SetPosition(middlePlot.transform.position + new Vector3(0, 5f, 0));

        } else {
            for (int i = 0, x = 0; x < plotAmountWidth; x++) {
                for (int y = 0; y < plotAmountHeight; y++, i++) {
                    GameObject plotGameObject = plotGameObjects[i];
                    Plot plot = plotGameObject.GetComponent<Plot>();
                    plot.Setup(plotSize, plotSize);
                    plots[x, y] = plot;

                    
                }
            }

            AssignNeighbors();
        }


        SetupWorldBorders();
    }

    private void GeneratePlots() {
        for (int x = 0; x < plotAmountWidth; x++) {
            for (int y = 0; y < plotAmountHeight; y++) {
                Vector3 position = new Vector3(x * plotSize, 0, y * plotSize) + startingOffset;
                GameObject plotObj = Instantiate(plotPrefab, position, Quaternion.identity, plotContainer.transform);
                Plot plot = plotObj.GetComponent<Plot>();
                plot.Setup(plotSize, plotSize);
                plots[x, y] = plot;

                plotGameObjects.Add(plotObj);
            }
        }
    }

    private void AssignNeighbors() {
        for (int x = 0; x < plotAmountWidth; x++) {
            for (int y = 0; y < plotAmountHeight; y++) {
                List<Plot> neighbors = new List<Plot>();
                // Check each possible direction
                if (x > 0) neighbors.Add(plots[x - 1, y]); // Left
                if (x < plotAmountWidth - 1) neighbors.Add(plots[x + 1, y]); // Right
                if (y > 0) neighbors.Add(plots[x, y - 1]); // Down
                if (y < plotAmountHeight - 1) neighbors.Add(plots[x, y + 1]); // Up

                if (x > 0 && y > 0) neighbors.Add(plots[x - 1, y - 1]); // Bottom-left
                if (x < plotAmountWidth - 1 && y > 0) neighbors.Add(plots[x + 1, y - 1]); // Bottom-right
                if (x > 0 && y < plotAmountHeight - 1) neighbors.Add(plots[x - 1, y + 1]); // Top-left
                if (x < plotAmountWidth - 1 && y < plotAmountHeight - 1) neighbors.Add(plots[x + 1, y + 1]); // Top-right

                plots[x, y].neighbors = neighbors;
            }
        }
    }

    private void SetupWorldBorders() {
        float borderHeight = 200;
        float borderSize = 10;
        float offset = borderSize;

        positiveZBorder.transform.localScale = new Vector3(plotAmountWidth * plotSize, borderHeight, borderSize);
        positiveZBorder.transform.position = new Vector3(plotAmountWidth * plotSize / 2, borderHeight / 2, plotAmountHeight * plotSize + offset);

        negativeZBorder.transform.localScale = new Vector3(plotAmountWidth * plotSize, borderHeight, borderSize);
        negativeZBorder.transform.position = new Vector3(plotAmountWidth * plotSize / 2, borderHeight / 2, -offset);

        positiveXBorder.transform.localScale = new Vector3(borderSize, borderHeight, plotAmountHeight * plotSize);
        positiveXBorder.transform.position = new Vector3(plotAmountWidth * plotSize + offset, borderHeight / 2, plotAmountHeight * plotSize / 2);

        negativeXBorder.transform.localScale = new Vector3(borderSize, borderHeight, plotAmountHeight * plotSize);
        negativeXBorder.transform.position = new Vector3(-offset, borderHeight / 2, plotAmountHeight * plotSize / 2);

        yBorder.transform.localScale = new Vector3(plotAmountWidth * plotSize, borderSize, plotAmountHeight * plotSize);
    }

    public int GetPlotSize() {
        return plotSize;
    }

    public int GetPlotAmountWidth() {
        return plotAmountWidth;
    }

    public int GetPlotAmountHeight() {
        return plotAmountHeight;
    }
    
}
