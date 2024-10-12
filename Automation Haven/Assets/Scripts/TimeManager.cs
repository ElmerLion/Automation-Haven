using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {
    public static TimeManager Instance { get; private set; }

    public event Action OnHourChanged;
    public event Action OnDayChanged;
    public event EventHandler OnMonthChanged;

    [SerializeField] private float timeScale = 60; // 1 real second equals 60 in-game seconds
    [SerializeField] private int startHour = 6; // Game starts at 6:00

    private int gameSpeed;

    public int Hour { get; private set; }
    public int Day { get; private set; } = 1;
    public int Month { get; private set; } = 1;
    public int Year { get; private set; } = 2015;

    private float timeCounter;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
    }

    private void Update() {
        UpdateTime();
    }

    void UpdateTime() {
        timeCounter += Time.deltaTime * timeScale;

        if (timeCounter >= 3600) // 3600 seconds in an hour
        {
            timeCounter -= 3600;
            Hour++;

            OnHourChanged?.Invoke();

            if (Hour > 23) // End of day
            {

                Hour = 0;
                OnHourChanged?.Invoke();

                Day++;
                OnDayChanged?.Invoke();

                if (Day > 10) // End of month
                {
                    Month++;
                    OnMonthChanged?.Invoke(this, EventArgs.Empty);
                    if (Month > 12) // End of year
                    {
                        Month = 1;
                        OnMonthChanged?.Invoke(this, EventArgs.Empty);
                        Year++;
                    }
                }

            }

        }
    }

    public void SetGameSpeed(int speed) {
        Time.timeScale = speed;
        gameSpeed = speed;
    }

    private string GetCurrentMonthString() {
        switch (Month) {
            case 1:
                return "January";
            case 2:
                return "February";
            case 3:
                return "March";
            case 4:
                return "April";
            case 5:
                return "May";
            case 6:
                return "June";
            case 7:
                return "July";
            case 8:
                return "August";
            case 9:
                return "September";
            case 10:
                return "October";
            case 11:
                return "November";
            case 12:
                return "December";
            default:
                return "Invalid Month";
        }
    }

    private string GetDaySuffix() {
        switch (Day) {
            case 1:
            case 21:
            case 31:
                return "st";
            case 2:
            case 22:
                return "nd";
            case 3:
            case 23:
                return "rd";
            default:
                return "th";
        }
    }

    public string GetCurrentTimeString() {
        string timeString = $"{Hour}:00 | {Day}{GetDaySuffix()} {GetCurrentMonthString()} | {Year}";
        return timeString;
    }

    private void SaveManager_OnGameSaved(string obj) {
        ES3.Save("Hour", Hour, obj);
        ES3.Save("Day", Day, obj);
        ES3.Save("Month", Month, obj);
        ES3.Save("Year", Year, obj);

        ES3.Save("gameSpeed", gameSpeed, obj);
    }

    private void SaveManager_OnGameLoaded(string obj) {
        Hour = ES3.Load("Hour", obj, startHour);
        OnHourChanged?.Invoke();

        Day = ES3.Load("Day", obj, Day);
        OnDayChanged?.Invoke();

        Month = ES3.Load("Month", obj, Month);
        OnMonthChanged?.Invoke(this, EventArgs.Empty);

        Year = ES3.Load("Year", obj, Year);
        

        gameSpeed = ES3.Load("gameSpeed", obj, 1);
        SetGameSpeed(gameSpeed);
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded -= SaveManager_OnGameLoaded;
    }
}

