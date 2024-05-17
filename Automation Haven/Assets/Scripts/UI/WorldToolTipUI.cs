using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldToolTipUI : MonoBehaviour {

    [SerializeField] private Transform toolTipTransform;
    [SerializeField] private TextMeshProUGUI toolTipText;
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private RectTransform backgroundRectTransform;

    private bool showMore = false;

    private void Start() {

        GameInput.Instance.OnShowMorePerformedAction += GameInput_OnShowMoreAction;
        GameInput.Instance.OnShowMoreCanceledAction += GameInput_OnShowMoreCanceledAction;

        Hide();
        
    }

    private void GameInput_OnShowMoreCanceledAction(object sender, System.EventArgs e) {
        showMore = false;
        Hide();
    }

    private void GameInput_OnShowMoreAction(object sender, System.EventArgs e) {
        showMore = true;
    }

    private void Update() {
        if (showMore) {
            UpdateMousePosition();

            ShowToolTip();
        }
    }

    private void UpdateMousePosition() {
        Vector3 mousePosition = Input.mousePosition;

        float offsetX = 120;
        float offsetY = 65;
        mousePosition.x += offsetX;
        mousePosition.y += offsetY;

        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, mousePosition, null, out Vector2 localPoint);
        transform.localPosition = localPoint;
    }

    private bool RaycastFromMousePosition(out Transform transform) {
        Vector3 mouseScreenPosition = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            Transform hitTransform = hit.transform;
            if (hitTransform.GetComponent<IShowWorldTooltip>() != null) {
                transform = hit.transform;
                return true; 
            }
            
        }
        transform = null;
        return false;
    }


    private void ShowToolTip() {
        if (RaycastFromMousePosition(out Transform transform)) {
            IShowWorldTooltip showWorldTooltip = transform.GetComponent<IShowWorldTooltip>();

            string tooltipTextString = showWorldTooltip.GetTooltipInfo();

            if (transform.TryGetComponent(out Box box)) {
                tooltipTextString += "\n" + box.GetTooltipInfo();
            }
            toolTipText.text = tooltipTextString;   

            toolTipTransform.gameObject.SetActive(true);
            UpdateBackgroundSize(toolTipText.preferredHeight, toolTipText.preferredWidth, 15, 25);
        } else {
            Hide();
        }

    }

    private void UpdateBackgroundSize(float totalHeight, float totalWidth, float paddingHeight, float paddingWidth) {
        totalHeight += paddingHeight;
        totalWidth += paddingWidth;

        Vector2 backgroundSize = backgroundRectTransform.sizeDelta;
        backgroundSize.x = totalWidth;
        backgroundSize.y = totalHeight;
        backgroundRectTransform.sizeDelta = backgroundSize;
    }

    private void Hide() {
        toolTipTransform.gameObject.SetActive(false);
    }
}
