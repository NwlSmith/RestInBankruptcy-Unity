using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class GravestoneUIManager : MonoBehaviour
{

    [System.Serializable]
    public class UserCommentPair
    {
        public string user = "";
        public string comment = "";
    }
    
    [System.Serializable]
    public class GravestoneCommentFlowerData
    {
        public List<UserCommentPair> userCommentPairs = new List<UserCommentPair>();
        public string packageId;
        public int flowers;
    }
    
    /*
     * UI:
     * Add in tagline for business
     * Add in flower screen
     * - to enter, have a floating flower model to the lower left with the number of flowers, cover with a button, brings you to another screen that says how many flowers they have, then asks you if you want to add another, also has return button
     * Add in comment screen
     * - to enter, have a 3d speech bubble with words? on lower right, cover with a button, clicking on it brings up screen-space list of comments, and a text box to enter in a comment of your own, also has return button.
     */

    [SerializeField] private TMP_Text[] flowerNumberTexts;
    [SerializeField] private RectTransform[] initialViewUI;
    [SerializeField] private RectTransform[] flowerUI;
    [SerializeField] private RectTransform[] commentUI;
    [SerializeField] private RectTransform commentContentPane;
    [SerializeField] private TMP_InputField commentInputField;
    [SerializeField] private GameObject commentPrefab;

    public bool readyToShowUI { get; private set; } = true;

    private Gravestone _currentGravestone;
    private PlayerLook _playerLook;
    
    private Canvas _canvas;

    private string databaseServerURL = "http://18.220.179.6:8080/dynamoDB/doc/GravestoneOfferings/packageId/";

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;
        _playerLook = FindObjectOfType<PlayerLook>();
        
        AddComment("Lorem Ipsem dolor.");
    }

    public void Activate(Gravestone gravestone)
    {
        _currentGravestone = gravestone;
        _canvas.transform.position = gravestone.transform.position;
        
        _canvas.enabled = true;
        EnableUI(initialViewUI);
        DisableUI(flowerUI);
        DisableUI(commentUI);
        
        SetNumFlowers(_currentGravestone.GetInfo().NumFlowers);
    }

    public void Deactivate()
    {
        _canvas.enabled = false;
        DisableUI(initialViewUI);
        DisableUI(flowerUI);
        DisableUI(commentUI);
    }

    public void ExitButton()
    {
        _playerLook.ExitGravestoneUI();
    }

    public void FlowerUIButton()
    {
        DisableUI(initialViewUI);
        EnableUI(flowerUI);
        DisableUI(commentUI);
    }

    public void FlowerButtonReturn()
    {
        EnableUI(initialViewUI);
        DisableUI(flowerUI);
        DisableUI(commentUI);
    }
    
    public void CommentUIButton()
    {
        DisableUI(initialViewUI);
        DisableUI(flowerUI);
        EnableUI(commentUI);
        
        
        // Retrieve all comments?
        // Set content pane height to the height of a comment * number of comments
    }

    public void CommentButtonReturn()
    {
        EnableUI(initialViewUI);
        DisableUI(flowerUI);
        DisableUI(commentUI);
    }
    
    public void GiveFlowerButton()
    {
        _currentGravestone.IncrementNumFlowers();
        SetNumFlowers(_currentGravestone.GetInfo().NumFlowers);
    }

    private void SetNumFlowers(int numFlowers)
    {
        string flowerNumber = numFlowers.ToString();
        foreach (TMP_Text flowerNumberText in flowerNumberTexts)
        {
            flowerNumberText.text = flowerNumber;
        }
    }

    private void EnableUI(RectTransform[] uiElements)
    {
        foreach (RectTransform rectTransform in uiElements)
        {
            rectTransform.gameObject.SetActive(true);
        }
    }

    private void DisableUI(RectTransform[] uiElements)
    {
        foreach (RectTransform rectTransform in uiElements)
        {
            rectTransform.gameObject.SetActive(false);
        }
    }

    public void RequestGravestoneData(string id, Gravestone newlyHighlightedGravestone)
    {
        _currentGravestone = newlyHighlightedGravestone;
        StartCoroutine(RequestGravestoneDataCO(id));
    }

    private IEnumerator RequestGravestoneDataCO(string id)
    {
        readyToShowUI = false;

        string URL = databaseServerURL + "USCOURTS-deb-1_07-bk-10416";//+ id;
        
        UnityWebRequest webRequest = UnityWebRequest.Get(URL);
        Debug.Log($"Sending request for {URL}");
        // Request and wait for the desired page.
        yield return webRequest.SendWebRequest();

        string webRequestResponse = "";

        if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
            webRequest.result == UnityWebRequest.Result.ProtocolError ||
            webRequest.result == UnityWebRequest.Result.DataProcessingError) {
            Debug.LogError("Error: " + webRequest.error);
            yield return new WaitForSeconds(200f);
        }
        else {
            webRequestResponse = webRequest.downloadHandler.text;
            Debug.Log(webRequestResponse);
        }

        if (webRequestResponse == "")
        {
            Debug.Log("Web request failed successfully! :)");
        }
        else
        {
            GravestoneCommentFlowerData parsedDataArray = DeserializeJson(webRequestResponse);

            _currentGravestone.SetNumFlowers(parsedDataArray.flowers);
            SetNumFlowers(parsedDataArray.flowers);

            for (int i = commentContentPane.childCount - 1; i >= 0; i++)
            {
                // Get rid of children!
            }
        }
        
        readyToShowUI = true;
    }

    private GravestoneCommentFlowerData DeserializeJson(string jsonData)
    {
        JObject jsonDataObject = JObject.Parse(jsonData);
        
        GravestoneCommentFlowerData gravestoneCommentFlowerData = new GravestoneCommentFlowerData();
        gravestoneCommentFlowerData.packageId = jsonDataObject.GetValue("packageId").ToString();
        gravestoneCommentFlowerData.flowers = int.Parse(jsonDataObject.GetValue("flowers").ToString());
        JObject jsonComments = (JObject)jsonDataObject.GetValue("comments");

        Debug.Log($"package has {jsonComments.Count} children");

        foreach (var keyValuePair in jsonComments)
        {
            UserCommentPair userCommentPair = new UserCommentPair();
            userCommentPair.user = keyValuePair.Key;
            userCommentPair.comment = keyValuePair.Value.ToString();
            gravestoneCommentFlowerData.userCommentPairs.Add(userCommentPair);
        }
        
        return gravestoneCommentFlowerData;
    }
    
    // Comment System.

    private void AddComment(string newComment)
    {
        GameObject newCommentGO = Instantiate(commentPrefab, commentContentPane);
        TMP_Text commentText = newCommentGO.GetComponent<TMP_Text>();
        commentText.text = newComment;
        commentContentPane.sizeDelta = new Vector2(0, commentContentPane.childCount * newCommentGO.GetComponent<RectTransform>().rect.height);
    }

    public void OnPostButtonPressed()
    {
        Debug.Log("PostButtonPressed");
        string newComment = commentInputField.text;
        AddComment(newComment);
        commentInputField.text = "";
        
        // Send to server
    }
}
