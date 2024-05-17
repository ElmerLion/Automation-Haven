using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadGameUI : BaseUI {

    public static LoadGameUI Instance { get; private set; }

    [SerializeField] private Transform saveFilesContainer;
    [SerializeField] private Transform saveFilePrefab;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        saveFilePrefab.gameObject.SetActive(false);

        Hide();
    }

    public override void Show() {
        AddAllSaveFiles();

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
            saveFileInstance.Find("Button").GetComponent<Button>().onClick.AddListener(() => LoadGame(saveName));
            saveFileInstance.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() => DeleteSaveGame(saveName));
            saveFileInstance.Find("Date").GetComponent<TextMeshProUGUI>().text = file.Item2.ToString("g");

            saveFileInstance.gameObject.SetActive(true);
        }
    }

    private void DeleteSaveGame(string saveName) {
        SaveManager.Instance.DeleteSaveGame(saveName);
        AddAllSaveFiles();
    }

    private void LoadGame(string saveName) {
        SaveManager.Instance.LoadGame(saveName);
    }
    
}
