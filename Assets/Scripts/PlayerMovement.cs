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
    }

    private void FixedUpdate()
    {
        // Calculate input movement vector.
        Vector3 moveVector = transform.right * InputManager.Instance.XInput + transform.forward * InputManager.Instance.ZInput;

        // Calculate physics movement.
        if (_onGround && !_charController.isGrounded)
        {
            _physicsVector += moveVector * moveSpeed;
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
                _physicsVector.y = -2f;
            if (InputManager.Instance.YInput >= 0.1f)
            {
                // Save the player's input movement so it will continue with same velocity while in air.
                _physicsVector += moveVector * moveSpeed;
                // Jump.
                _physicsVector.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _onGround = false;
            }

            //if the velocity is more than 0.01f the animation for walking will play
            if (_charController.velocity.magnitude <= 0.01f)
            {
                //walkCycle.SetBool("ifMovingBool", false);
            }

            //if the player isn't moving then the idle animation will start
            if (_charController.velocity.magnitude > 0.01f)
            {
                //walkCycle.SetBool("ifMovingBool", true);
            }
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
}
