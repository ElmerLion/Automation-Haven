using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;

public class SingleResearchNodeUI : MonoBehaviour {

    [SerializeField] private Color researchedColor;
    [SerializeField] private Transform connectedLinePrefab;
    [SerializeField] private Transform connectedLinesContainer;


    private ResearchManager.ResearchNode researchNode;
    private TextMeshProUGUI queueNumberText;
    private GameObject selectedVisual;
    private List<Transform> connectedLines;

    public void Setup(ResearchNodeSO researchNodeSO) {
        this.researchNode = ResearchManager.Instance.GetResearchNode(researchNodeSO);

        queueNumberText = transform.Find("QueueNumber").GetComponent<TextMeshProUGUI>();
        selectedVisual = transform.Find("SelectedVisual").gameObject;

        queueNumberText.gameObject.SetActive(false);
        selectedVisual.SetActive(false);

        transform.Find("Icon").GetComponent<Image>().sprite = researchNodeSO.icon;

        transform.GetComponent<Button>().onClick.AddListener(OnResearchNodeClicked);

        Vector3[] slots = {
            new Vector3(0, -133),
            new Vector3(0, 68),
            new Vector3(0, 3),
            new Vector3(0, -61),
            new Vector3(0, -126)
        };

        transform.localPosition = slots[researchNodeSO.slotIndex];

        if (researchNode.isResearched) {
            MarkAsCompleted();
        }
        if (ResearchManager.Instance.DoesResearchQueueContainNode(researchNode)) {
            ShowQueueNumber(ResearchManager.Instance.GetResearchQueue().IndexOf(researchNode) + 1);
        }

        CoroutineHandler.Instance.StartHandledCoroutine(ActivatePrerequisitesAndConnect());
    }

    private IEnumerator ActivatePrerequisitesAndConnect() {

        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        foreach (ResearchNodeSO prerequisiteNode in researchNode.researchNodeSO.prerequisiteResearchList) {
            Transform prerequisiteTransform = ResearchTreeUI.Instance.GetResearchNodeSOTransform(prerequisiteNode);
            prerequisiteTransform.gameObject.SetActive(true);
        }

        gameObject.SetActive(true);

        ConnectToPrerequisites();
    }

    public void ConnectToPrerequisites() {
        if (connectedLines == null || connectedLines.Count == 0) {
            connectedLines = new List<Transform>();

            foreach (ResearchNodeSO prerequisiteNode in researchNode.researchNodeSO.prerequisiteResearchList) {

                Transform connectingLine = Instantiate(connectedLinePrefab, connectedLinesContainer);

                RectTransform connectingLineRectTransform = connectingLine.GetComponent<RectTransform>();
                RectTransform currentRectTransform = GetComponent<RectTransform>();
                RectTransform prerequisiteNodeRectTransform = ResearchTreeUI.Instance.GetResearchNodeSOTransform(prerequisiteNode).GetComponent<RectTransform>();

                // Calculate the world positions of the rect transforms
                Vector3 startPointWorld = currentRectTransform.transform.position;
                Vector3 endPointWorld = prerequisiteNodeRectTransform.transform.position;

                // Log the world positions for debugging

                // Calculate the direction and distance between the nodes in world space
                Vector2 direction = endPointWorld - startPointWorld;
                float distance = Vector2.Distance(startPointWorld, endPointWorld);

                // Convert the midpoint position back to local space of the connecting line's parent

                Vector3 midpointWorld = (startPointWorld + endPointWorld) / 2;
                Vector2 midpointLocal = ((RectTransform)connectedLinesContainer).InverseTransformPoint(midpointWorld);


                // Set the line position to the midpoint
                connectingLineRectTransform.anchoredPosition = midpointLocal;

                // Adjust the size to match the distance
                connectingLineRectTransform.sizeDelta = new Vector2(distance, 10);


                // Adjust the rotation to match the direction

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                connectingLineRectTransform.localRotation = Quaternion.Euler(0, 0, angle);


                connectingLine.gameObject.SetActive(false);
                connectedLines.Add(connectingLine);

            }
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

    private void OnDisable() {
        if (connectedLines == null) return;

        foreach (Transform connectedLine in connectedLines) {
            connectedLine.gameObject.SetActive(false);
        }
    }

    private void OnEnable() {
        if (connectedLines == null) return;

        foreach (Transform connectedLine in connectedLines) {
            connectedLine.gameObject.SetActive(true);
        }
    }
}
