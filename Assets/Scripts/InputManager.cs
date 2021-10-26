using UnityEngine;
/*
 * Date created: 10/26/2021
 * Creator: Nate Smith
 * 
 * Description: Input manager
 * Takes in and stores input.
 */
public class InputManager : MonoBehaviour
{
    // Static instance of the object.
    public static InputManager instance = null;

    public bool inputActive = true;
    
    public float xInput { get; private set; } = 0f;
    public float yInput { get; private set; } = 0f;
    public float zInput { get; private set; } = 0f;
    public bool sprint { get; private set; } = false;
    public float mouseX { get; private set; } = 0f;
    public float mouseY { get; private set; } = 0f;
    public bool mouseClick { get; private set; } = false;
    public bool escape { get; private set; } = false;
    
    private void Awake()
    {
        // Ensure that there is only one instance of the InputManager.
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Update()
    {
        if (inputActive)
        {
            DetectInputs();
        }
        else
        {
            ZeroInputs();
        }

        escape = Input.GetButtonDown("Cancel");
    }

    private void DetectInputs()
    {
        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");
        yInput = Input.GetAxis("Jump");
        sprint = Input.GetButton("Sprint");

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        mouseClick = Input.GetButtonDown("Fire1");
    }

    private void ZeroInputs()
    {
        xInput = 0;
        zInput = 0;
        yInput = 0;
        sprint = false;

        mouseX = 0;
        mouseY = 0;
        mouseClick = false;
    }
}
