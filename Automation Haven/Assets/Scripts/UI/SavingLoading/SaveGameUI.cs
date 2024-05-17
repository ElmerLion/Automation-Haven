using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class SaveGameUI : BaseUI {

    public static SaveGameUI Instance { get; private set; }

    [SerializeField] private Transform saveFilesContainer;
    [SerializeField] private Transform saveFilePrefab;
    [SerializeField] private Button newSaveButton;
    [SerializeField] private TMP_InputField saveNameInputField;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        saveFilePrefab.gameObject.SetActive(false);

        newSaveButton.onClick.AddListener(NewSaveButtonPressed);

        Hide();
    }

    public override void Show() {
        AddAllSaveFiles();

        saveNameInputField.text = SaveManager.CurrentSaveFileName;

        base.Show();
    }

    private void AddAllSaveFiles() {
        foreach (Transform child in saveFilesContainer) {
            if (child == saveFilePrefab) continue;
            Destroy(child.gameObject);
        }

        string[] saveFiles = new string[0];
        try {
            saveFiles = ES3.GetFiles();
        } catch (System.Exception e) {
            Debug.LogError("Error getting save files: " + e.Message);
        }

        List<Tuple<string, DateTime>> filesWithDates = new List<Tuple<string, DateTime>>();

        foreach (string saveFile in saveFiles) {
            string path = SaveManager.SavePath + saveFile;
            DateTime saveDate;
            if (DateTime.TryParse(ES3.Load("SavedDate", path, DateTime.MinValue.ToString()), out saveDate)) {
                filesWithDates.Add(new Tuple<string, DateTime>(saveFile, saveDate));
            }
        }

        // Sort the list by date descending
        filesWithDates.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        foreach (Tuple<string, DateTime> file in filesWithDates) {

            string saveFile = file.Item1;
            Transform saveFileInstance = Instantiate(saveFilePrefab, saveFilesContainer);

            string saveName = saveFile.Replace(".sav", "");

            saveFileInstance.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = saveName;
            saveFileInstance.Find("Button").GetComponent<Button>().onClick.AddListener(() => SaveGame(saveName));
            saveFileInstance.Find("Date").GetComponent<TextMeshProUGUI>().text = ES3.Load("SavedDate", SaveManager.SavePath + saveFile, "No Date");
            saveFileInstance.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() => DeleteSaveGame(saveName));

            saveFileInstance.gameObject.SetActive(true);
        }
    }

    private void NewSaveButtonPressed() {
        string saveName = saveNameInputField.text;
        if (saveName == "") {
            Debug.LogError("Save name cannot be empty.");
            return;
        }

        SaveGame(saveName);
    }

    private void DeleteSaveGame(string saveName) {
        SaveManager.Instance.DeleteSaveGame(saveName);
        AddAllSaveFiles();
    }

    private void SaveGame(string saveFile) {
        SaveManager.Instance.SaveGame(saveFile);

        Hide();
    }

}
