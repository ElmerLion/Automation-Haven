using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour {

    public static SaveManager Instance { get; private set; }

    public static string SavePath;
    public static string CurrentSaveFilePath;
    public static string CurrentSaveFileName;
    public static bool LoadGameOnStart = false;

    public static event Action<string> OnGameSaved;
    public static event Action<string> OnGameLoaded;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SavePath = Application.persistentDataPath + "/Saves/";

        DontDestroyOnLoad(gameObject);
    }


    public void SaveGame(string saveFile) {
        ES3.Save("SavedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), SavePath + saveFile + ".sav");
        OnGameSaved?.Invoke(SavePath + saveFile + ".sav");
    }

    public void LoadGame(string saveFile) {
        CurrentSaveFilePath = SavePath + saveFile + ".sav";
        CurrentSaveFileName = saveFile.Replace(".sav", "");
        LoadGameOnStart = true;
        SceneManager.LoadScene("GameScene");
    }

    public void DeleteSaveGame(string saveFile) {
        ES3.DeleteFile(SavePath + "/" + saveFile + ".sav");
    }

    public static void LoadActiveSaveFile() {
        Debug.Log("Loading Active Save File: " + CurrentSaveFilePath);
        OnGameLoaded?.Invoke(CurrentSaveFilePath);
    }

}
