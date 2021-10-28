using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GravestoneInfo
{
    public string ID = "0";
    public string Name = "test name";
    public DateTime StartTime;
    public DateTime EndTime;
    public int NumFlowers = 0;

    public void FromGravestoneData(LevelLoader.GravestoneData data)
    {
        ID = data.id;
        Name = data.name;
        StartTime = new DateTime(data.startTime, 1, 1);
        EndTime = new DateTime(data.endTime, 1, 1);
        NumFlowers = data.numFlowers;
    }
}

public class Gravestone : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text lifetimeText;
    private GravestoneInfo _gravestoneInfo;
    public Transform observationCameraPos;

    private void Awake()
    {
        // REMOVE THIS
        SetupGravestone("0", "test name", DateTime.Today - new TimeSpan(365*5, 23, 59, 59), DateTime.Today, 0);
    }


    public void SetupGravestone(string idString, string nameString, DateTime startTime, DateTime endTime, int numFlowers)
    {
        _gravestoneInfo = new GravestoneInfo();
        _gravestoneInfo.ID = idString;
        _gravestoneInfo.Name = nameString;
        _gravestoneInfo.StartTime = startTime;
        _gravestoneInfo.EndTime = endTime;
        _gravestoneInfo.NumFlowers = numFlowers;
        
        SetupGravestoneUI();
    }
    
    public void SetupGravestone(LevelLoader.GravestoneData gravestoneData)
    {
        _gravestoneInfo = new GravestoneInfo();
        _gravestoneInfo.FromGravestoneData(gravestoneData);
        SetupGravestoneUI();
    }

    private void SetupGravestoneUI()
    {
        id = _gravestoneInfo.ID;
        nameText.text = _gravestoneInfo.Name;
        string lifetimeString = $"{_gravestoneInfo.StartTime.Year} - {_gravestoneInfo.EndTime.Year}";
        lifetimeText.text = lifetimeString;
    }

    public GravestoneInfo GetInfo()
    {
        return _gravestoneInfo;
    }

    public void IncrementNumFlowers()
    {
        _gravestoneInfo.NumFlowers++;
    }

    public void StartHighlighting()
    {
        Debug.Log($"Highlighting gravestone of id {id}");
    }

    public void StopHighlighting()
    {
        Debug.Log($"Stopped highlighting gravestone of id {id}");
    }
}
