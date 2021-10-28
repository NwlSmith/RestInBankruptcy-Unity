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
    public static InputManager Instance = null;

    public bool inputActive = true;
    
    public float XInput { get; private set; } = 0f;
    public float YInput { get; private set; } = 0f;
    public float ZInput { get; private set; } = 0f;
    public bool Sprint { get; private set; } = false;
    public float MouseX { get; private set; } = 0f;
    public float MouseY { get; private set; } = 0f;
    public bool MouseClick { get; private set; } = false;
    public bool Escape { get; private set; } = false;
    
    private void Awake()
    {
        // Ensure that there is only one instance of the InputManager.
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
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

        Escape = Input.GetButtonDown("Cancel");
    }

    private void DetectInputs()
    {
        XInput = Input.GetAxis("Horizontal");
        ZInput = Input.GetAxis("Vertical");
        YInput = Input.GetAxis("Jump");
        Sprint = Input.GetButton("Sprint");

        MouseX = Input.GetAxis("Mouse X");
        MouseY = Input.GetAxis("Mouse Y");
        MouseClick = Input.GetButtonDown("Fire1");
    }

    private void ZeroInputs()
    {
        XInput = 0;
        ZInput = 0;
        YInput = 0;
        Sprint = false;

        MouseX = 0;
        MouseY = 0;
        MouseClick = false;
    }
}
