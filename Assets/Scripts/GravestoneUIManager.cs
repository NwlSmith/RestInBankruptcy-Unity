using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GravestoneUIManager : MonoBehaviour
{

    [System.Serializable]
    public class GravestoneCommentFlowerData
    {
        public List<string> comments = new List<string>();
        public string packageId;
        public int flowers;
    }
    
    [System.Serializable]
    public class KeyObj
    {
        public string packageId;
    }

    [System.Serializable]
    public class FlowerPutRequest
    {
        public KeyObj keyObj;
    }

    [System.Serializable]
    public class CommentPutRequest
    {
        public KeyObj keyObj;
        public string comment;
    }
    
    /*
     * UI:
     * Add in tagline for business
     * Add in flower screen
     * - to enter, have a floating flower model to the lower left with the number of flowers, cover with a button, brings you to another screen that says how many flowers they have, then asks you if you want to add another, also has return button
     * Add in comment screen
     * - to enter, have a 3d speech bubble with words? on lower right, cover with a button, clicking on it brings up screen-space list of comments, and a text box to enter in a comment of your own, also has return button.
     */

    [SerializeField] private TMP_Text numFlowersToDonateText;
    [SerializeField] private Button flowerMinusButton;
    [SerializeField] private Button flowerPlusButton;
    [SerializeField] private TMP_Text[] flowerNumberTexts;
    [SerializeField] private TMP_Text commentNumberText;
    [SerializeField] private RectTransform[] initialViewUI;
    [SerializeField] private RectTransform[] flowerUI;
    [SerializeField] private RectTransform[] commentUI;
    [SerializeField] private TMP_InputField commentInputField;
    [SerializeField] private RectTransform commentContentPane;
    [SerializeField] private GameObject commentPrefab;
    [SerializeField] private TMP_Text enterCommentMessage;

    private int _numFlowersToAdd = 1;

    public bool readyToShowUI { get; private set; } = true;

    private Gravestone _currentGravestone;
    private PlayerLook _playerLook;
    
    private Canvas _canvas;

    private string databaseServerURL = "http://18.220.179.6:8080/dynamoDB/doc/GravestoneOfferings/packageId/";
    private string databaseServerSendFlowersURL = "http://18.220.179.6:8080/dynamoDB/doc/flowers/";
    private string databaseServerSendCommentsURL = "http://18.220.179.6:8080/dynamoDB/doc/comments";

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;
        _playerLook = FindObjectOfType<PlayerLook>();
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
        enterCommentMessage.enabled = false;
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

        _numFlowersToAdd = 1;
        DisableButton(flowerMinusButton);
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
        enterCommentMessage.enabled = false;
    }

    public void CommentButtonReturn()
    {
        EnableUI(initialViewUI);
        DisableUI(flowerUI);
        DisableUI(commentUI);
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

        string URL = databaseServerURL + id;
        
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

            for (int i = commentContentPane.childCount - 1; i >= 0; i--)
            {
                // Get rid of children!
                Debug.Log($"Destroying child {i}, name: {commentContentPane.GetChild(i)}");
                Destroy(commentContentPane.GetChild(i).gameObject);
            }
            commentContentPane.DetachChildren();

            foreach (var comment in parsedDataArray.comments)
            {
                AddCommentUI(comment);
            }
            
            commentNumberText.text = parsedDataArray.comments.Count.ToString();
        }
        
        readyToShowUI = true;
    }

    private GravestoneCommentFlowerData DeserializeJson(string jsonData)
    {
        JObject jsonDataObject = JObject.Parse(jsonData); // THIS IS CAUSING A LOT OF LAG
        
        GravestoneCommentFlowerData gravestoneCommentFlowerData = new GravestoneCommentFlowerData();
        gravestoneCommentFlowerData.packageId = jsonDataObject.GetValue("packageId").ToString();
        gravestoneCommentFlowerData.flowers = int.Parse(jsonDataObject.GetValue("flowers").ToString());
        JArray jsonComments = (JArray)jsonDataObject.GetValue("comments");

        Debug.Log($"package has {jsonComments.Count} children");

        foreach (var keyValuePair in jsonComments)
        {
            string comment = keyValuePair.ToString();
            Debug.Log($"comment string is: {comment}");
            gravestoneCommentFlowerData.comments.Add(comment);
        }
        
        return gravestoneCommentFlowerData;
    }
    
    // Flower button

    public void IncrementNumFlowerButton()
    {
        _numFlowersToAdd++;
        _numFlowersToAdd = Mathf.Clamp(_numFlowersToAdd, 1, 10);
        numFlowersToDonateText.text = _numFlowersToAdd.ToString();
        
        EnableButton(flowerMinusButton);
        if (_numFlowersToAdd == 10)
        {
            flowerPlusButton.interactable = false;
            DisableButton(flowerPlusButton);
        }
        else
        {
            flowerPlusButton.interactable = true;
            EnableButton(flowerPlusButton);
        }
    }
    
    public void DecrementNumFlowerButton()
    {
        _numFlowersToAdd--;
        _numFlowersToAdd = Mathf.Clamp(_numFlowersToAdd, 1, 10);
        numFlowersToDonateText.text = _numFlowersToAdd.ToString();
        
        EnableButton(flowerPlusButton);
        if (_numFlowersToAdd == 1)
        {
            DisableButton(flowerMinusButton);
        }
        else
        {
            EnableButton(flowerMinusButton);
        }
    }

    private void EnableButton(Button buttonToEnable)
    {
        buttonToEnable.interactable = true;
        if (ColorUtility.TryParseHtmlString("#1B2C4AFF", out Color color))
        {
            buttonToEnable.GetComponentInChildren<TMP_Text>().color = color;
        }
        else
        {
            Debug.LogError("Failed to convert color properly?");
        }
    }

    private void DisableButton(Button buttonToDisable)
    {
        buttonToDisable.interactable = false;
        if (ColorUtility.TryParseHtmlString("#1B2C4A80", out Color color))
        {
            buttonToDisable.GetComponentInChildren<TMP_Text>().color = color;
        }
        else
        {
            Debug.LogError("Failed to convert color properly?");
        }
    }

    public void GiveFlowerButton()
    {
        _currentGravestone.IncrementNumFlowers(_numFlowersToAdd);
        SetNumFlowers(_currentGravestone.GetInfo().NumFlowers);
        StartCoroutine(SendNewFlowerNumber());
        
        FlowerButtonReturn();
    }

    private void SetNumFlowers(int newNumFlowers)
    {
        string flowerNumber = newNumFlowers.ToString();
        foreach (TMP_Text flowerNumberText in flowerNumberTexts)
        {
            flowerNumberText.text = flowerNumber;
        }
    }

    private IEnumerator SendNewFlowerNumber()
    {
        string URL = databaseServerSendFlowersURL + _numFlowersToAdd;

        // Construct JSON object:

        FlowerPutRequest flowerPut = new FlowerPutRequest();
        flowerPut.keyObj = new KeyObj();
        flowerPut.keyObj.packageId = _currentGravestone.GetInfo().ID;

        string jsonObj = JsonUtility.ToJson(flowerPut);

        byte[] myData = System.Text.Encoding.UTF8.GetBytes(jsonObj);
        
        UnityWebRequest webRequest = UnityWebRequest.Put(URL, myData);
        webRequest.SetRequestHeader ("Content-Type", "application/json");
        Debug.Log($"Sending request {jsonObj} to {URL}");
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
    }
    
    // Comment System.
    
    public void OnPostButtonPressed()
    {
        Debug.Log("PostButtonPressed");
        string newComment = commentInputField.text;
        if (newComment != "")
        {
            AddComment(newComment);
            commentInputField.text = "";
            enterCommentMessage.enabled = false;
        }
        else
        {
            enterCommentMessage.enabled = true;
            commentInputField.onValueChanged.AddListener(delegate { HideWarningMessageCallback(); });
        }
    }

    private void HideWarningMessageCallback()
    {
        enterCommentMessage.enabled = false;
        commentInputField.onValueChanged.RemoveAllListeners();
    }

    private void AddCommentUI(string newComment)
    {
        GameObject newCommentGO = Instantiate(commentPrefab, commentContentPane);
        TMP_Text commentText = newCommentGO.GetComponentInChildren<TMP_Text>();
        commentText.text = newComment;

        newCommentGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, commentText.text.Split('\n').Length * 28.5f + 28.5f);
        Debug.Log($"Comment {newComment} has {commentText.text.Split('\n').Length} lines, so it should be {commentText.text.Split('\n').Length * 28.5f + 28.5f} tall");

        float sizeOfPane = 18;
        for (int i = 0; i < commentContentPane.childCount; i++)
        {
            sizeOfPane += commentContentPane.GetChild(i).GetComponent<RectTransform>().sizeDelta.y + 18;//rect.height + 18;
            Debug.Log($"Adding {commentContentPane.GetChild(i).GetComponent<RectTransform>().sizeDelta.y + 18} from {commentContentPane.GetChild(i).name} to size of pane, for total of  {sizeOfPane} ");
        }
        
        commentContentPane.sizeDelta = new Vector2(0, sizeOfPane);

        commentNumberText.text = commentContentPane.childCount.ToString();
    }

    private void AddComment(string newComment)
    {
        AddCommentUI(newComment);
        StartCoroutine(AddCommentCO(newComment));
    }

    private IEnumerator AddCommentCO(string newComment)
    {
        // send newComment

        string URL = databaseServerSendCommentsURL;// + "USCOURTS-deb-1_07-bk-10416";//+ id;
        
        // Construct JSON object:

        CommentPutRequest commentPut = new CommentPutRequest();
        commentPut.keyObj = new KeyObj();
        commentPut.keyObj.packageId = _currentGravestone.GetInfo().ID;//"USCOURTS-nywb-1_00-bk-12654";//"USCOURTS-deb-1_07-bk-10416"; 
        commentPut.comment = newComment;

        string jsonObj = JsonUtility.ToJson(commentPut);

        byte[] jsonObjAsBytes = System.Text.Encoding.UTF8.GetBytes(jsonObj);
        
        UnityWebRequest webRequest = UnityWebRequest.Put(URL, jsonObjAsBytes);
        webRequest.SetRequestHeader ("Content-Type", "application/json");
        Debug.Log($"Sending comment request {jsonObj} to {URL}");
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
    }
}
