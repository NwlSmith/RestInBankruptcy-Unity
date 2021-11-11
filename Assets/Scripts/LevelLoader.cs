using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq; // REMOVE ME
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

/*
 * Date created: 10/26/2021
 * Creator: Nate Smith
 * 
 * Description: Level loader utility.
 * Handles UI for initial scene.
 * Loads scene, retrieves input from server, constructs graveyard, and disappears.
 */
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private int sceneToLoad = 1;
    [SerializeField] private Canvas canvas = null;
    [SerializeField] private Image overlay = null;
    [SerializeField] private Image img = null;
    [SerializeField] private Button startButton = null;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressSlider;

    private string databaseURL = "http://18.220.179.6:8080/govinfo/dummydata";
    private string databaseServerURL = "http://18.220.179.6:8080/dynamoDB/docs/RestInDatabase/courtState/New%20York";
    
    [System.Serializable]
    public class GravestoneData
    {
        public string id = "0";
        public string name = "test name";
        public int startTime;
        public int endTime;
        public int numFlowers = 0;
        public string description = "";
        public string numEmployeesString = "";
        // Add in comments
    }

    private void Start()
    {
        //canvas.enabled = false;
        progressText.enabled = false;
        progressSlider.gameObject.SetActive(false);
        TEMPSerializeJSON();
    }

    public void OnPressStart()
    {
        StartCoroutine(LoadScene());
    }

    public void OnPressStartServer()
    {
        StartCoroutine(LoadSceneServerData());
    }

    private IEnumerator LoadScene()
    {
        /* --- Startup --- */
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(canvas);

        canvas.enabled = true;
        startButton.enabled = false;
        progressText.enabled = true;
        progressSlider.gameObject.SetActive(true);
        
        /* --- Load scene --- */
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneToLoad);

        progressText.text = "Loading scene...";

        while (!loading.isDone)
        {
            float prog = Mathf.Clamp01(loading.progress / .9f);
            progressSlider.value = prog / 4f;
            yield return null;
        }

        yield return new WaitForSeconds(.1f);

        InputManager.Instance.inputActive = false;

        /* --- Get data from server --- */
        progressText.text = "Retrieving data from server...";
        progressSlider.value = 1f / 4f;
        
        UnityWebRequest webRequest = UnityWebRequest.Get(databaseURL);
        // Request and wait for the desired page.
        yield return webRequest.SendWebRequest();

        string webRequestResponse = "";

        if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
            webRequest.result == UnityWebRequest.Result.ProtocolError ||
            webRequest.result == UnityWebRequest.Result.DataProcessingError) {
            progressText.text = "Error: " + webRequest.error;
            progressSlider.value = 0f;
            yield return new WaitForSeconds(200f);
        }
        else {
            webRequestResponse = webRequest.downloadHandler.text;
            string startOfResponse = "{\"Items\":";
            webRequestResponse = startOfResponse + webRequestResponse + "}";
            Debug.Log(webRequestResponse);
            
            progressSlider.value = 2f / 4f;
        }
        
        // REMOVE THIS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //webRequestResponse = GetDummyDataFromFile();

        /* --- Get data from JSON --- */
        
        progressText.text = "Parsing data...";
        GravestoneData[] parsedDataArray = DeserializeJsonDummy(webRequestResponse);
        progressSlider.value = 3f / 5f;

        /* --- Construct scene --- */
        
        progressText.text = "Constructing graveyard...";

        Gravestone[] gravestones = FindObjectsOfType<Gravestone>();

        int numAssignedGravestones;
        for (numAssignedGravestones = 0; numAssignedGravestones < gravestones.Length && numAssignedGravestones < parsedDataArray.Length; numAssignedGravestones++)
        {
            gravestones[numAssignedGravestones].SetupGravestone(parsedDataArray[numAssignedGravestones]);
            progressSlider.value = (3f + numAssignedGravestones / gravestones.Length) / 4f;
        }

        if (numAssignedGravestones < gravestones.Length)
        {
            // get rid of extra gravestones
            for (; numAssignedGravestones < gravestones.Length; numAssignedGravestones++)
            {
                Debug.LogError($"Num gravestones in scene: {gravestones.Length}. Num gravestone data recieved: {parsedDataArray.Length}. Too many gravestones.");
                gravestones[numAssignedGravestones].gameObject.SetActive(false);
            }
        }
        else if (numAssignedGravestones < parsedDataArray.Length)
        {
            Debug.LogError($"Num gravestones in scene: {gravestones.Length}. Num gravestone data recieved: {parsedDataArray.Length}. Too few gravestones.");
        }
        progressSlider.value = 1;

        /* --- Cleanup --- */
        
        canvas.enabled = false;
        overlay.enabled = false;
        Destroy(gameObject);
        InputManager.Instance.inputActive = true;
    }
    
    
    private IEnumerator LoadSceneServerData()
    {
        /* --- Startup --- */
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(canvas);

        canvas.enabled = true;
        startButton.enabled = false;
        progressText.enabled = true;
        progressSlider.gameObject.SetActive(true);
        
        /* --- Load scene --- */
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneToLoad);

        progressText.text = "Loading scene...";

        while (!loading.isDone)
        {
            float prog = Mathf.Clamp01(loading.progress / .9f);
            progressSlider.value = prog / 4f;
            yield return null;
        }

        yield return new WaitForSeconds(.1f);

        InputManager.Instance.inputActive = false;

        /* --- Get data from server --- */
        progressText.text = "Retrieving data from server...";
        progressSlider.value = 1f / 4f;
        
        UnityWebRequest webRequest = UnityWebRequest.Get(databaseServerURL);
        // Request and wait for the desired page.
        yield return webRequest.SendWebRequest();

        string webRequestResponse = "";

        if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
            webRequest.result == UnityWebRequest.Result.ProtocolError ||
            webRequest.result == UnityWebRequest.Result.DataProcessingError) {
            progressText.text = "Error: " + webRequest.error;
            progressSlider.value = 0f;
            yield return new WaitForSeconds(200f);
        }
        else {
            webRequestResponse = webRequest.downloadHandler.text;
            Debug.Log(webRequestResponse);
            
            progressSlider.value = 2f / 4f;
        }
        
        // REMOVE THIS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //webRequestResponse = GetDummyDataFromFile();

        /* --- Get data from JSON --- */
        
        progressText.text = "Parsing data...";
        GravestoneData[] parsedDataArray = DeserializeJson(webRequestResponse);
        progressSlider.value = 3f / 5f;

        /* --- Construct scene --- */
        
        progressText.text = "Constructing graveyard...";

        Gravestone[] gravestones = FindObjectsOfType<Gravestone>();

        int numAssignedGravestones;
        for (numAssignedGravestones = 0; numAssignedGravestones < gravestones.Length && numAssignedGravestones < parsedDataArray.Length; numAssignedGravestones++)
        {
            gravestones[numAssignedGravestones].SetupGravestone(parsedDataArray[numAssignedGravestones]);
            progressSlider.value = (3f + numAssignedGravestones / gravestones.Length) / 4f;
        }

        if (numAssignedGravestones < gravestones.Length)
        {
            // get rid of extra gravestones
            for (; numAssignedGravestones < gravestones.Length; numAssignedGravestones++)
            {
                Debug.LogError($"Num gravestones in scene: {gravestones.Length}. Num gravestone data recieved: {parsedDataArray.Length}. Too many gravestones.");
                gravestones[numAssignedGravestones].gameObject.SetActive(false);
            }
        }
        else if (numAssignedGravestones < parsedDataArray.Length)
        {
            Debug.LogError($"Num gravestones in scene: {gravestones.Length}. Num gravestone data recieved: {parsedDataArray.Length}. Too few gravestones.");
        }
        progressSlider.value = 1;

        /* --- Cleanup --- */
        
        canvas.enabled = false;
        overlay.enabled = false;
        Destroy(gameObject);
        InputManager.Instance.inputActive = true;
    }

    private void TEMPSerializeJSON()
    {
        GravestoneData[] dataArray = new GravestoneData[10];

        dataArray[0] = new GravestoneData();
        dataArray[0].id = "00";
        dataArray[0].name = "Test 1";
        dataArray[0].startTime = 1990;
        dataArray[0].endTime = 1991;
        dataArray[0].numFlowers = 0;
        
        dataArray[1] = new GravestoneData();
        dataArray[1].id = "01";
        dataArray[1].name = "Test 2";
        dataArray[1].startTime = 1992;
        dataArray[1].endTime = 1993;
        dataArray[1].numFlowers = 2;
        
        dataArray[2] = new GravestoneData();
        dataArray[2].id = "02";
        dataArray[2].name = "Test 3";
        dataArray[2].startTime = 1994;
        dataArray[2].endTime = 1995;
        dataArray[2].numFlowers = 6;
        
        dataArray[3] = new GravestoneData();
        dataArray[3].id = "03";
        dataArray[3].name = "Test 4";
        dataArray[3].startTime = 1996;
        dataArray[3].endTime = 1997;
        dataArray[3].numFlowers = 10;
        
        dataArray[4] = new GravestoneData();
        dataArray[4].id = "04";
        dataArray[4].name = "Test 5";
        dataArray[4].startTime = 1998;
        dataArray[4].endTime = 1999;
        dataArray[4].numFlowers = 25;
        
        dataArray[5] = new GravestoneData();
        dataArray[5].id = "05";
        dataArray[5].name = "Test 6";
        dataArray[5].startTime = 2000;
        dataArray[5].endTime = 2001;
        dataArray[5].numFlowers = 100;
        
        dataArray[6] = new GravestoneData();
        dataArray[6].id = "06";
        dataArray[6].name = "Test 7";
        dataArray[6].startTime = 2002;
        dataArray[6].endTime = 2003;
        dataArray[6].numFlowers = 1000;
        
        dataArray[7] = new GravestoneData();
        dataArray[7].id = "07";
        dataArray[7].name = "Test 8";
        dataArray[7].startTime = 2004;
        dataArray[7].endTime = 2005;
        dataArray[7].numFlowers = 101;
        
        dataArray[8] = new GravestoneData();
        dataArray[8].id = "08";
        dataArray[8].name = "Test 9";
        dataArray[8].startTime = 2006;
        dataArray[8].endTime = 2007;
        dataArray[8].numFlowers = 1492;
        
        dataArray[9] = new GravestoneData();
        dataArray[9].id = "09";
        dataArray[9].name = "Test 10";
        dataArray[9].startTime = 2008;
        dataArray[9].endTime = 2009;
        dataArray[9].numFlowers = 9001;

        string toJSON = Utilities.JsonHelper.ToJson(dataArray);
        Debug.Log(toJSON);
    }

    private string GetDummyDataFromFile()
    {
        var sr = new StreamReader(Application.dataPath + "/DummyData.json");
        var fileContents = sr.ReadToEnd();
        sr.Close();
        return fileContents;
    }
    
    private GravestoneData[] DeserializeJsonDummy(string jsonData)
    {
        GravestoneData[] dataArray = Utilities.JsonHelper.FromJson<GravestoneData>(jsonData);
        foreach (GravestoneData data in dataArray)
        {
            Debug.Log($"deserialized {data.name}");
        }
        return dataArray;
    }
    
    private GravestoneData[] DeserializeJson(string jsonData)
    {
        JArray jsonDataArray = JArray.Parse(jsonData);
        
        GravestoneData[] dataArray = new GravestoneData[jsonDataArray.Count];

        for (int i = 0; i < jsonDataArray.Count; i++)
        {
            JObject jsonObject = jsonDataArray.Value<JObject>(i);

            if (i == 0)
            {
                Debug.Log(jsonObject.ToString());
            }
            GravestoneData gravestoneData = new GravestoneData();
            gravestoneData.id = jsonObject.GetValue("packageId").ToString();
            gravestoneData.name = jsonObject.GetValue("title").ToString();
            //gravestoneData.startTime = int.Parse(jsonObject.GetValue("DateIncorporated").ToString().Substring(0, 4)); // temp value
            
            gravestoneData.endTime = jsonObject.GetValue("dateIssued").Value<DateTime>().Year;
            
            JToken startDateToken;
            if (jsonObject.TryGetValue("DateIncorporated", out startDateToken) && startDateToken.ToString().Length > 0)
            {
                gravestoneData.startTime = startDateToken.Value<DateTime>().Year;
            }
            else
            {
                gravestoneData.startTime = gravestoneData.endTime;
            }
            
            // try get description
            JToken descriptionToken;
            if (jsonObject.TryGetValue("naics_description", out descriptionToken) && descriptionToken.ToString().Length > 0)
            {
                gravestoneData.description = descriptionToken.ToString();
            }
            else if (jsonObject.TryGetValue("sic_description", out descriptionToken) && descriptionToken.ToString().Length > 0)
            {
                gravestoneData.description = descriptionToken.ToString();
            }
            
            JToken numEmployeesToken;
            if (jsonObject.TryGetValue("employees", out numEmployeesToken) && numEmployeesToken.ToString().Length > 0)
            {
                gravestoneData.numEmployeesString = numEmployeesToken.ToString();
            }
            else if (jsonObject.TryGetValue("employees_range", out numEmployeesToken) && numEmployeesToken.ToString().Length > 0)
            {
                gravestoneData.numEmployeesString = numEmployeesToken.ToString();
            }
            
            dataArray[i] = gravestoneData;
        }

        foreach (GravestoneData data in dataArray)
        {
            Debug.Log($"deserialized {data.name}");
        }
        return dataArray;
    }
}
