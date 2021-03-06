using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/*
 * Date created: 10/26/2021
 * Creator: Nate Smith
 * 
 * Description: Moves the camera on the horizontal and vertical directions based on mouse movement.
 *
 * Based off code from Portal1.5
 */
public class PlayerLook : MonoBehaviour
{
    // Public Variables.
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private LayerMask graveStoneLayermask;
    [SerializeField] private float raycastRange = 2f;
    [FormerlySerializedAs("camera")] [SerializeField] private Transform playerCamera;
    [SerializeField] private GravestoneUIManager gravestoneUI;
    [SerializeField] private Image crosshairs;
    
    [SerializeField] private float lerpDuration = 1f;

    // Private Variables.
    private Transform _playerBody;
    private float _xRot = 0f;
    private bool _viewportCaptured = false;

    private Gravestone _currentlyHighlightedGravestone = null;

    void Start()
    {
        CaptureMouseInput();

        _playerBody = GetComponentInParent<CharacterController>().transform;
        if (_playerBody == null)
        {
            Debug.Log("Parent of " + name + " does not contain a Character Controller.");
        }

        crosshairs.enabled = true;
    }

    void Update()
    {
        if (!_viewportCaptured)
        {
            ResolveMouseMovement();
            ResolveMousePointing();
        }

        if (InputManager.Instance.Escape)
        {
            if (_viewportCaptured)
            {
                ExitGravestoneUI();
            }
            else
            {
                ReleaseMouseInput();
            }
        }
        
    }

    private void ResolveMouseMovement()
    {
        // Retrieve mouse input.
        float mouseX = InputManager.Instance.MouseX * mouseSensitivity * Time.deltaTime;
        float mouseY = InputManager.Instance.MouseY * mouseSensitivity * Time.deltaTime;

        // Move camera vertically.
        _xRot -= mouseY;
        _xRot = Mathf.Clamp(_xRot, -90, 90);
        transform.localRotation = Quaternion.Euler(_xRot, 0, 0);

        // Rotate player horizontally.
        _playerBody.Rotate(Vector3.up * mouseX);
    }

    private void ResolveMousePointing()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, raycastRange, 
            graveStoneLayermask, QueryTriggerInteraction.Ignore))
        {
            Gravestone newCollidedGravestone = hitInfo.collider.gameObject.GetComponent<Gravestone>();
            
            if (newCollidedGravestone != _currentlyHighlightedGravestone)
            {
                // moved from 1 GS to another
                if (_currentlyHighlightedGravestone != null)
                {
                    _currentlyHighlightedGravestone.StopHighlighting();
                }

                // started highlighting new GS
                _currentlyHighlightedGravestone = newCollidedGravestone;
                _currentlyHighlightedGravestone.StartHighlighting();
            }

            // clicked
            if (InputManager.Instance.MouseClick)
            {
                ZoomIntoGravestoneUI();
            }
        }
        // moved from 1 GS to nothing
        else if (_currentlyHighlightedGravestone != null)
        {
            _currentlyHighlightedGravestone.StopHighlighting();
            _currentlyHighlightedGravestone = null;
        }
    }

    private void ZoomIntoGravestoneUI()
    {
        StartCoroutine(LerpToGravestonePosEnum());
//        LerpToGravestoneCameraPos();
        Debug.Log($"Gravestone = {_currentlyHighlightedGravestone.name}");
        
        _currentlyHighlightedGravestone.StopHighlighting();
    }

    private IEnumerator LerpToGravestonePosEnum()
    {
        gravestoneUI.RequestGravestoneData(_currentlyHighlightedGravestone.GetInfo().ID, _currentlyHighlightedGravestone);
        
        crosshairs.enabled = false;
        
        InputManager.Instance.inputActive = false;
        _viewportCaptured = true;
        
        float elapsedTime = 0f;
        float smoothTime = 0.3F;
        Vector3 velocity = Vector3.zero;
        GetComponentInParent<MeshRenderer>().enabled = false;
        
        Quaternion initRot = playerCamera.rotation;
        
        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;

            playerCamera.position = Vector3.SmoothDamp(playerCamera.position, _currentlyHighlightedGravestone.observationCameraPos.position, ref velocity, smoothTime);
            playerCamera.rotation = Quaternion.Slerp(initRot, _currentlyHighlightedGravestone.observationCameraPos.rotation,
                    elapsedTime / lerpDuration);
            yield return null;
        }

        playerCamera.position = _currentlyHighlightedGravestone.observationCameraPos.position;
        playerCamera.rotation = _currentlyHighlightedGravestone.observationCameraPos.rotation;

        while (!gravestoneUI.readyToShowUI)
        {
            yield return null;
        }

        gravestoneUI.Activate(_currentlyHighlightedGravestone);

        ReleaseMouseInput();
        
        _currentlyHighlightedGravestone = null;
    }

    public void ExitGravestoneUI()
    {
        StartCoroutine(LerpToPlayerCameraPosEnum());
    }
    
    private IEnumerator LerpToPlayerCameraPosEnum()
    {
        gravestoneUI.Deactivate();
        
        crosshairs.enabled = true;
        
        float elapsedTime = 0f;
        float smoothTime = 0.3F;
        Vector3 velocity = Vector3.zero;
        
        CaptureMouseInput();
        
        Quaternion initRot = playerCamera.rotation;
        
        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;

            playerCamera.position = Vector3.SmoothDamp(playerCamera.position, transform.position, ref velocity, smoothTime);
            playerCamera.rotation = Quaternion.Slerp(initRot, transform.rotation,
                elapsedTime / lerpDuration);
            yield return null;
        }

        playerCamera.position = transform.position;
        playerCamera.rotation = transform.rotation;
        
        
        InputManager.Instance.inputActive = true;
        _viewportCaptured = false;
        GetComponentInParent<MeshRenderer>().enabled = true;
    }

    private void CaptureMouseInput()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ReleaseMouseInput()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
