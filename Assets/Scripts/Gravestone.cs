using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GravestoneInfo
{
    public string id = "0";
    public string name = "test name";
    public DateTime startTime;
    public DateTime endTime;
    public int numFlowers = 0;
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
        id = idString;
        nameText.text = nameString;
        string lifetimeString = $"{startTime.Year} - {endTime.Year}";
        lifetimeText.text = lifetimeString;

        _gravestoneInfo = new GravestoneInfo();
        _gravestoneInfo.id = idString;
        _gravestoneInfo.name = nameString;
        _gravestoneInfo.startTime = startTime;
        _gravestoneInfo.endTime = endTime;
        _gravestoneInfo.numFlowers = numFlowers;
    }

    public GravestoneInfo GetInfo()
    {
        return _gravestoneInfo;
    }

    public void IncrementNumFlowers()
    {
        _gravestoneInfo.numFlowers++;
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
