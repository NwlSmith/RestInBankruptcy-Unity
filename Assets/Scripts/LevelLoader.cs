using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
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
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressSlider;

    private void Start()
    {
        canvas.enabled = false;
        progressText.enabled = false;
        progressSlider.enabled = false;
    }

    public void OnPressStart()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        /* --- Startup --- */
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(canvas);

        canvas.enabled = true;
        progressText.enabled = true;
        progressSlider.enabled = true;
        
        /* --- Load scene --- */
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneToLoad);

        progressText.text = "Loading scene...";

        while (!loading.isDone)
        {
            float prog = Mathf.Clamp01(loading.progress / .9f);
            progressSlider.value = prog;
            yield return null;
        }

        yield return null;

        InputManager.instance.inputActive = false;

        yield return new WaitForSeconds(.1f);

        /* --- Get data from server --- */
        progressText.text = "Retrieving data from server...";

        /* --- Get construct scene --- */
        
        progressText.text = "Constructing graveyard...";

        /* --- Cleanup --- */
        
        canvas.enabled = false;
        img.enabled = false;
        Destroy(gameObject);
        InputManager.instance.inputActive = true;
    }
    
    
}
