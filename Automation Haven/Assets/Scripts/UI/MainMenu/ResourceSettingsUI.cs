using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceSettingsUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI resourceNameText;
    [SerializeField] private Slider clusterFrequencySlider;
    [SerializeField] private TextMeshProUGUI currentFrequencyText;
    [SerializeField] private Slider clusterSizeSlider;
    [SerializeField] private TextMeshProUGUI currentClusterSizeText;

    private ResourceNodeGenerator.ResourceNodeSettings resourceNodeSettings;

    public void Initialize(ResourceNodeGenerator.ResourceNodeSettings resourceNodeSettings) {
        this.resourceNodeSettings = resourceNodeSettings;

        resourceNameText.text = resourceNodeSettings.resourceItemSO.nameString;
        clusterFrequencySlider.value = resourceNodeSettings.amountOfClustersPer30000Cells;
        currentFrequencyText.text = resourceNodeSettings.amountOfClustersPer30000Cells.ToString();
        clusterSizeSlider.value = resourceNodeSettings.nodesPerClusterMax;
        currentClusterSizeText.text = resourceNodeSettings.nodesPerClusterMax.ToString();

        clusterFrequencySlider.onValueChanged.AddListener((value) => {
            resourceNodeSettings.amountOfClustersPer30000Cells = (int)value;
            currentFrequencyText.text = value.ToString();
        });

        clusterSizeSlider.onValueChanged.AddListener((value) => {
            resourceNodeSettings.nodesPerClusterMax = (int)value;
            resourceNodeSettings.nodesPerClusterMin = (int)value - resourceNodeSettings.nodesPerClusterDifference;
            currentClusterSizeText.text = value.ToString();
        });

        gameObject.SetActive(true);
    }
    
}
