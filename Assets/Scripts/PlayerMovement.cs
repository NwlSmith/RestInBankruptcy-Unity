using System.Collections;
using UnityEngine;
/*
 * Date created: 10/26/2021
 * Creator: Nate Smith
 * 
 * Description: Moves the player in the horizontal plane and handles gravity and jumping.
 *
 * Based off code from Portal1.5
 */
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float maxVelocity = 100f;  
    
    
    private Vector3 _physicsVector;
    private bool _onGround = false;
    private CharacterController _charController;
    private PlayerLook _playerLook;
    private AudioSource _audioSource;
    private bool _audioSourceIsPlaying = false;

    private Coroutine _volumeUpEnum;
    private bool _volumeUpPlaying = false;
    private Coroutine _volumeDownEnum;
    private bool _volumeDownPlaying = false;

    void Start()
    {
        if (!TryGetComponent(out _charController))
        {
            Debug.Log($"{name} does not contain a Character Controller.");
        }
        
        if (!this.TryGetComponentInChildren(out _playerLook))
        {
            Debug.Log($"{name} does not contain a PlayerLook.");
        }

        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = 0f;
    }

    private void FixedUpdate()
    {
        // Calculate input movement vector.
        Vector3 moveVector = transform.right * InputManager.Instance.XInput + transform.forward * InputManager.Instance.ZInput;

        // Calculate physics movement.
        if (_onGround && !_charController.isGrounded)
        {
            _physicsVector += moveVector * moveSpeed * .2f;
        }
        
        _onGround = _charController.isGrounded;
        if (_onGround)
        {
            //walkCycle.SetTrigger("ifNotMoving");
            // When on the ground, the player shouldn't have any horizontal velocity other than input movement.
            _physicsVector.x = 0f;
            _physicsVector.z = 0f;
            
            // When on the ground, the player's vertical velocity doesn't need to increase with gravity.
            if (_physicsVector.y < 0f)
            {
                _physicsVector.y = -2f;
            }
            if (InputManager.Instance.YInput >= 0.1f)
            {
                // Save the player's input movement so it will continue with same velocity while in air.
                _physicsVector += moveVector * moveSpeed * .2f;
                // Jump.
                _physicsVector.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _onGround = false;
            }

            if (moveVector.magnitude > 0 && !_audioSourceIsPlaying)
            {
                TurnOnWalkSound();
            }
            else if (moveVector.magnitude == 0)
            {
                TurnOffWalkSound();
            }
        }
        else
        {
            TurnOffWalkSound();
        }
        // Increment physics gravity.
        _physicsVector.y += gravity * Time.fixedDeltaTime;

        // Clamp velocity.
        _physicsVector.y = Mathf.Clamp(_physicsVector.y, -maxVelocity, maxVelocity);

        // Move player according to its input and physics
        _charController.Move((moveVector * moveSpeed + _physicsVector) * Time.fixedDeltaTime);
    }

    /* Called when the charController collides with an object. */
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 collisionDirection = hit.normal;
        if (collisionDirection == Vector3.down)
        {
            _physicsVector.y = 0f;
        }
        
        if (!_charController.isGrounded)
        {
            if (collisionDirection == Vector3.right || collisionDirection == Vector3.left)
            {
                _physicsVector.x = 0f;
            }
            else if (collisionDirection == Vector3.forward || collisionDirection == Vector3.back)
            {
                _physicsVector.z = 0f;
            }
        }
    }

    private void TurnOnWalkSound()
    {

        if (_volumeUpPlaying)
            return;
        
        if (_volumeDownPlaying)
        {
            _volumeDownPlaying = false;
            StopCoroutine(_volumeDownEnum);
        }
        _volumeUpEnum = StartCoroutine(VolumeUpCO());
    }

    private void TurnOffWalkSound()
    {
        if (_volumeDownPlaying || !_audioSourceIsPlaying)
            return;
        
        if (_volumeUpPlaying)
        {
            _volumeUpPlaying = false;
            StopCoroutine(_volumeUpEnum);
        }
        _volumeDownEnum = StartCoroutine(VolumeDownCO());
        
        _audioSourceIsPlaying = false;
    }

    private IEnumerator VolumeUpCO()
    {
        Debug.Log("VolumeUpCO Started");
        _audioSourceIsPlaying = true;
        _volumeUpPlaying = true;

        float elapsedTime = 0f;
        float duration = .5f;
        float initVol = _audioSource.volume;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(initVol, 1f, elapsedTime / duration);
            yield return null;
        }
        _audioSource.volume = 1f;
        
        _volumeUpPlaying = false;
    }

    private IEnumerator VolumeDownCO()
    {
        Debug.Log("VolumeDownCO Started");
        _volumeDownPlaying = true;

        float elapsedTime = 0f;
        float duration = .5f;
        float initVol = _audioSource.volume;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(initVol, 0f, elapsedTime / duration);
            yield return null;
        }
        _audioSource.volume = 0f;
        
        _volumeDownPlaying = false;
        _audioSourceIsPlaying = false;
    }
}
