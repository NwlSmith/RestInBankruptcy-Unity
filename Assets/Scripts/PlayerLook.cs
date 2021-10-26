using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private Transform camera;
    [SerializeField] private GravestoneUIManager gravestoneUI;
    
    [SerializeField] private float lerpDuration = 1f;

    // Private Variables.
    private Transform _playerBody;
    private float _xRot = 0f;
    private bool _viewportCaptured = false;

    private Gravestone _currentlyHighlightedGravestone = null;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _playerBody = GetComponentInParent<CharacterController>().transform;
        if (_playerBody == null)
        {
            Debug.Log("Parent of " + name + " does not contain a Character Controller.");
        }
    }

    void Update()
    {
        ResolveMouseMovement();
        ResolveMousePointing();

        if (_viewportCaptured && InputManager.instance.escape)
        {
            ExitGravestoneUI();
        }
    }

    private void ResolveMouseMovement()
    {
        // Retrieve mouse input.
        float mouseX = InputManager.instance.mouseX * mouseSensitivity * Time.deltaTime;
        float mouseY = InputManager.instance.mouseY * mouseSensitivity * Time.deltaTime;

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
            if (InputManager.instance.mouseClick)
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
    }

    private IEnumerator LerpToGravestonePosEnum()
    {
        InputManager.instance.inputActive = false;
        _viewportCaptured = true;
        
        float elapsedTime = 0f;
        float smoothTime = 0.3F;
        Vector3 velocity = Vector3.zero;
        GetComponentInParent<MeshRenderer>().enabled = false;
        
        Quaternion initRot = camera.rotation;
        
        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;

            camera.position = Vector3.SmoothDamp(camera.position, _currentlyHighlightedGravestone.observationCameraPos.position, ref velocity, smoothTime);
            camera.rotation = Quaternion.Slerp(initRot, _currentlyHighlightedGravestone.observationCameraPos.rotation,
                    elapsedTime / lerpDuration);
            yield return null;
        }

        camera.position = _currentlyHighlightedGravestone.observationCameraPos.position;
        camera.rotation = _currentlyHighlightedGravestone.observationCameraPos.rotation;

        gravestoneUI.Activate(_currentlyHighlightedGravestone);

        Cursor.lockState = CursorLockMode.None;
    }

    public void ExitGravestoneUI()
    {
        StartCoroutine(LerpToPlayerCameraPosEnum());
    }
    
    private IEnumerator LerpToPlayerCameraPosEnum()
    {
        gravestoneUI.Deactivate();
        
        float elapsedTime = 0f;
        float smoothTime = 0.3F;
        Vector3 velocity = Vector3.zero;
        
        Cursor.lockState = CursorLockMode.Locked;
        
        Quaternion initRot = camera.rotation;
        
        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;

            camera.position = Vector3.SmoothDamp(camera.position, transform.position, ref velocity, smoothTime);
            camera.rotation = Quaternion.Slerp(initRot, transform.rotation,
                elapsedTime / lerpDuration);
            yield return null;
        }

        camera.position = transform.position;
        camera.rotation = transform.rotation;
        
        
        InputManager.instance.inputActive = true;
        _viewportCaptured = false;
        GetComponentInParent<MeshRenderer>().enabled = true;
    }
}
