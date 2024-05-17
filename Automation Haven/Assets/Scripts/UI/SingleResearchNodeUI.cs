using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SingleResearchNodeUI : MonoBehaviour {

    [SerializeField] private Color researchedColor;

    private ResearchManager.ResearchNode researchNode;
    private TextMeshProUGUI queueNumberText;
    private GameObject selectedVisual;

    public void Setup(ResearchNodeSO researchNodeSO) {
        this.researchNode = ResearchManager.Instance.GetResearchNode(researchNodeSO);

        queueNumberText = transform.Find("QueueNumber").GetComponent<TextMeshProUGUI>();
        selectedVisual = transform.Find("SelectedVisual").gameObject;

        queueNumberText.gameObject.SetActive(false);
        selectedVisual.SetActive(false);

        transform.Find("Icon").GetComponent<Image>().sprite = researchNodeSO.icon;

        transform.GetComponent<Button>().onClick.AddListener(OnResearchNodeClicked);

        Vector3 slot0 = new Vector3(0, -133);
        Vector3 slot1 = new Vector3(0, 68);
        Vector3 slot2 = new Vector3(0, 3);
        Vector3 slot3 = new Vector3(0, -61);
        Vector3 slot4 = new Vector3(0, -126);

        switch (researchNodeSO.slotIndex) {
            case 0:
                transform.localPosition = slot0;
                break;
            case 1:
                transform.localPosition = slot1;
                break;
            case 2:
                transform.localPosition = slot2;
                break;
            case 3:
                transform.localPosition = slot3;
                break;
            case 4:
                transform.localPosition = slot4;
                break;
        }

        if (researchNode.isResearched) {
            MarkAsCompleted();
        }
        if (ResearchManager.Instance.DoesResearchQueueContainNode(researchNode)) {
            ShowQueueNumber(ResearchManager.Instance.GetResearchQueue().IndexOf(researchNode) + 1);
        }
    }

    public void OnResearchNodeClicked() {
        ResearchTreeUI.Instance.ShowSideBarInfo(researchNode);
    }

    public void ShowQueueNumber(int queueNumber) {
        queueNumberText.text = queueNumber.ToString();
        selectedVisual.SetActive(true);
        queueNumberText.gameObject.SetActive(true);
    }

    public void MarkAsCompleted() {
        selectedVisual.SetActive(false);
        queueNumberText.gameObject.SetActive(false);
        transform.Find("Background").GetComponent<Image>().color = researchedColor;
    }

    public void RemoveFromQueue() {
        selectedVisual.SetActive(false);
        queueNumberText.gameObject.SetActive(false);
    }

    public ResearchManager.ResearchNode GetResearchNode() {
        return researchNode;
    }
}
