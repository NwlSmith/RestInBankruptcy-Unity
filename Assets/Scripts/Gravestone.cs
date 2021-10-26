using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GravestoneInfo
{
    public string id;
    public string name;
    public DateTime startTime;
    public DateTime endTime;
}

public class Gravestone : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text lifetimeText;
    private GravestoneInfo _gravestoneInfo;
    public Transform observationCameraPos;
    
    
    public void SetupGravestone(string idString, string nameString, DateTime startTime, DateTime endTime)
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
    }

    public GravestoneInfo GetInfo()
    {
        return _gravestoneInfo;
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
