using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GravestoneUIManager : MonoBehaviour
{
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

    private Gravestone _currentGravestone;
    private PlayerLook _playerLook;
    
    private Canvas _canvas;

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
        
        string flowerNumber = _currentGravestone.GetInfo().NumFlowers.ToString();
        foreach (TMP_Text flowerNumberText in flowerNumberTexts)
        {
            flowerNumberText.text = flowerNumber;
        }
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
        string newFlowerNumber = _currentGravestone.GetInfo().NumFlowers.ToString();
        foreach (TMP_Text flowerNumberText in flowerNumberTexts)
        {
            flowerNumberText.text = newFlowerNumber;
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
}
