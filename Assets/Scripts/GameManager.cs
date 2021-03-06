using UnityEngine;

/*
 * Date created: 10/26/2021
 * Creator: Nate Smith
 * 
 * Description: Game Manager.
 * This a single instance static object - There should only be 1 GameManager.
 * Manages and controls game various functionalities.
 */
public class GameManager : MonoBehaviour
{
    // Static instance of the object.
    public static GameManager Instance = null;

    // Public Variables.
    public bool debug = false;

    private void Awake()
    {
        // Ensure that there is only one instance of the GameManager.
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }
}
