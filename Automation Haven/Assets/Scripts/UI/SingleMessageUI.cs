using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleMessageUI : MonoBehaviour, IPointerClickHandler {

    private MessageBarUI.MessageData messageData;

    public void Setup(MessageBarUI.MessageData messageData, string message) {
        this.messageData = messageData;

        Button button = transform.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            MessageBarUI.Instance.ShowMessageExpanded(message, transform, messageData);
        });
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            MessageBarUI.Instance.CloseMessage(transform, messageData);
        }
    }
}
