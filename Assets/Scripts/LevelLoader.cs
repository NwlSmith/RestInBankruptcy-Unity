using System;
using System.Collections;
using Newtonsoft.Json.Linq;
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

    private readonly string databaseServerURL = "http://3.144.16.89:8080/dynamoDB/docs/RestInDatabase/courtState/";
    private string nyString = "New%20York";
    private string njString = "New%20Jersey";
    private string caString = "California";
    
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
    }

    private void Start()
    {
        //canvas.enabled = false;
        progressText.enabled = false;
        progressSlider.gameObject.SetActive(false);
        StartCoroutine(FadeInCO());
    }

    private IEnumerator FadeInCO()
    {
        img.color = new Color (1,1,1,0);
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            overlay.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, elapsedTime / duration));
            yield return null;
        }
        overlay.color = new Color(0, 0, 0, 0);
    }

    public void OnPressStart()
    {
        StartCoroutine(LoadSceneData(nyString));
    }

    public void OnPressStartNY()
    {
        StartCoroutine(LoadSceneData(nyString));
    }

    public void OnPressStartNJ()
    {
        StartCoroutine(LoadSceneData(njString));
    }

    public void OnPressStartCA()
    {
        StartCoroutine(LoadSceneData(caString));
    }

    private IEnumerator LoadSceneData(string stateString)
    {
        /* --- Fadeout --- */
        overlay.enabled = true;

        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newVal = Mathf.Lerp(0, 1, elapsedTime / duration);
            overlay.color = new Color(0, 0, 0, newVal);
            img.color = new Color (1, 1, 1, newVal);
            yield return null;
        }
        overlay.color = new Color(0, 0, 0, 1);
        img.color = new Color (1, 1, 1, 1);

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
        
        UnityWebRequest webRequest = UnityWebRequest.Get(databaseServerURL + stateString);
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
            Debug.LogError($"Num gravestones in scene: {gravestones.Length}. Num gravestone data recieved: {parsedDataArray.Length}. Too many gravestones.");
            for (; numAssignedGravestones < gravestones.Length; numAssignedGravestones++)
            {
                gravestones[numAssignedGravestones].gameObject.SetActive(false);
            }
        }
        else if (numAssignedGravestones < parsedDataArray.Length)
        {
            Debug.LogError($"Num gravestones in scene: {gravestones.Length}. Num gravestone data recieved: {parsedDataArray.Length}. Too few gravestones.");
        }
        progressSlider.value = 1;

        /* --- Cleanup --- */

        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            GameObject uiElement = canvas.transform.GetChild(i).gameObject;
            if (uiElement != overlay.gameObject)
            {
                uiElement.SetActive(false);
            }
        }
        
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            overlay.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, elapsedTime / duration));
            yield return null;
        }
        overlay.color = new Color(0, 0, 0, 0);
        
        canvas.enabled = false;
        overlay.enabled = false;
        Destroy(gameObject);
        InputManager.Instance.inputActive = true;
    }
    
    private GravestoneData[] DeserializeJson(string jsonData)
    {
        JArray jsonDataArray = JArray.Parse(jsonData);
        
        GravestoneData[] dataArray = new GravestoneData[jsonDataArray.Count];

        for (int i = 0; i < jsonDataArray.Count; i++)
        {
            JObject jsonObject = jsonDataArray.Value<JObject>(i);

            GravestoneData gravestoneData = new GravestoneData();
            gravestoneData.id = jsonObject.GetValue("packageId").ToString();
            gravestoneData.name = jsonObject.GetValue("title").ToString();
            
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
            if (jsonObject.TryGetValue("description", out descriptionToken) && descriptionToken.ToString().Length > 0)
            {
                gravestoneData.description = descriptionToken.ToString();
            }
            else if (jsonObject.TryGetValue("naics_description", out descriptionToken) && descriptionToken.ToString().Length > 0)
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
        
        return dataArray;
    }
}
