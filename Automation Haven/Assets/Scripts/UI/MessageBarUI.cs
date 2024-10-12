using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBarUI : BaseUI {

    public static MessageBarUI Instance { get; private set; }

    [SerializeField] private Transform expandedMessageBox;
    [SerializeField] private Transform messageTemplate;
    [SerializeField] private Transform messageContainer;
    [SerializeField] private Button closeButton;

    [SerializeField] private Color neutralColor;
    [SerializeField] private Color negativeColor;
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color completionColor;

    private List<Transform> activeMessages;
    private List<MessageData> messageDataList;
    private Transform openedMessageTransform;
    private MessageData openedMessageData;


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;

        activeMessages = new List<Transform>();

        messageTemplate.gameObject.SetActive(false);

        closeButton.onClick.AddListener(Hide);
    }

    private void SaveManager_OnGameSaved(string obj) {
        ES3.Save("messageDataList", messageDataList, obj);
        ES3.Save("nextMessageId", MessageData.nextId, obj);
    }

    private void SaveManager_OnGameLoaded(string obj) {
        if (ES3.KeyExists("messageDataList", obj)) {
            messageDataList = ES3.Load<List<MessageData>>("messageDataList", obj);

            List<MessageData> messageDataListCopy = new List<MessageData>(messageDataList);
            foreach (MessageData messageData in messageDataListCopy) {
                CreateMessageWithoutCreatingData(messageData.title, messageData.message, messageData.messageType, messageData);
            }
        } else {
            messageDataList = new List<MessageData>();
        }

        MessageData.nextId = ES3.Load("nextMessageId", obj, 0);

        if (messageDataList.Count <= 0) {
            CreateTestMessages();
        }

        Hide();
    }

    public void CreateMessage(string title, string message, EventType messageType) {
        Transform messageTransform = Instantiate(messageTemplate, messageContainer);

        Image background = messageTransform.Find("Background").GetComponent<Image>();
        TextMeshProUGUI titleText = messageTransform.Find("Title").GetComponent<TextMeshProUGUI>();

        switch (messageType) {
            case EventType.Neutral:
                background.color = neutralColor;
                break;
            case EventType.Negative:
                background.color = negativeColor;
                break;
            case EventType.Positive:
                background.color = positiveColor;
                break;
            case EventType.Completion:
                background.color = completionColor;
                break;
        }

        titleText.text = title;

        activeMessages.Add(messageTransform);

         MessageData newMessageData = new MessageData(title, message, messageType);
         messageDataList.Add(newMessageData);
        

        messageTransform.GetComponent<SingleMessageUI>().Setup(newMessageData, message);

        messageTransform.gameObject.SetActive(true);
    }

    private void CreateMessageWithoutCreatingData(string title, string message, EventType messageType, MessageData messageData) {
        Transform messageTransform = Instantiate(messageTemplate, messageContainer);

        Image background = messageTransform.Find("Background").GetComponent<Image>();
        TextMeshProUGUI titleText = messageTransform.Find("Title").GetComponent<TextMeshProUGUI>();

        switch (messageType) {
            case EventType.Neutral:
                background.color = neutralColor;
                break;
            case EventType.Negative:
                background.color = negativeColor;
                break;
            case EventType.Positive:
                background.color = positiveColor;
                break;
            case EventType.Completion:
                background.color = completionColor;
                break;
        }

        titleText.text = title;

        activeMessages.Add(messageTransform);

        messageTransform.GetComponent<SingleMessageUI>().Setup(messageData, message);

        messageTransform.gameObject.SetActive(true);
    }

    public void ShowMessageExpanded(string message, Transform messageTransform, MessageData messageData) {
        expandedMessageBox.Find("Message").GetComponent<TextMeshProUGUI>().text = message;
        openedMessageTransform = messageTransform;
        openedMessageData = messageData;

        Show();
    }

    private void CreateTestMessages() {
        CreateMessage("Neutral", "This is a test neutral message", EventType.Neutral);
        CreateMessage("Negative", "This is a test negative message", EventType.Negative);
        CreateMessage("Positive", "This is a test positive message", EventType.Positive);
        CreateMessage("Completion", "This is a test completion message", EventType.Completion);
    }

    public override void Show() {
        expandedMessageBox.gameObject.SetActive(true);
        isOpen = true;

        UIManager.Instance.CloseOtherUIs(this);
        AutomationGameManager.Instance.CheckPauseState();
    }

    public void CloseMessage(Transform messageTransform, MessageData messageData) {
        activeMessages.Remove(messageTransform);
        messageDataList.Remove(messageData);
        Destroy(messageTransform.gameObject);
    }

    public override void Hide() {
        expandedMessageBox.gameObject.SetActive(false);
        isOpen = false;

        if (openedMessageTransform != null) {
            activeMessages.Remove(openedMessageTransform);
            messageDataList.Remove(openedMessageData);
            Destroy(openedMessageTransform.gameObject);
            openedMessageTransform = null;
        }

        InterfaceToolTipUI.Instance.Hide();
        AutomationGameManager.Instance.CheckPauseState();
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded -= SaveManager_OnGameLoaded;
    }



    public class MessageData {
        public static int nextId = 0;

        public int id;
        public string title;
        public string message;
        public EventType messageType;

        public MessageData(string title, string message, EventType messageType) {
            this.title = title;
            this.message = message;
            this.messageType = messageType;
            id = nextId;
            nextId++;
        }
    }

}
