using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Helpers.Tags;
using Player.InputSystem;
using Sounds.SFX;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

namespace Player
{
     [RequireComponent(typeof(Rigidbody))]
     [RequireComponent(typeof(PlayerAnimation))]
     public class PlayerCharacterController : MonoBehaviour, PlayerInputSystemController.IPlayerCharacterControllerActions
     {
          // Player controller component
          private PlayerInputSystemController _PlayerInputController;
          
          [Header("Player Movement Controller")]
          [Tooltip("Default Movement Speed Of Player Character")] [SerializeField] private float _movementSpeed = 250f;
          [Tooltip("Run Movement Speed Of Player Character")] [SerializeField] private float _runMovementSpeed = 260;
          private Vector2 _PlayerMovementInput;
          private Vector3 _CurrentMovement;
          private Vector3 _CurrentRunMovement;
          private Vector3 _PlayerCharacterMovementVelocity; // This is for applied player movement
          private Rigidbody _PlayerRb;
          private bool _isMove;
          private bool _isMovementPressed;
          private bool _isRunPressed;
          
          [Header("Rotate Object Direction")]
          [Tooltip("Rotation Speed To Rotate Player Direction")] [SerializeField] [Range(0, 20)] private float _rotationSpeed = 15f;

          [Header("Jump And Gravity Object")]
          [Tooltip("Set Max Jump Height With Speed")] [field: SerializeField] private float _maxJumpHeight = 1.0f;
          [Tooltip("Set Max Jump Time")] [field: SerializeField] private float _maxJumpTime = 0.75f;
          private float _jumpVelocity;
          private bool _isJump;
          private bool _isJumpPressed;
          private bool _isJumping;
          private bool _isJumpAnimating;
          private bool _canPlayJumpSFX;

          // Gravity
          [Tooltip("Gravity Falling Multiplier Of An Object")] [SerializeField] [Range(0f, 10f)] private float _gravityMultiplier = 1.0f;
          private float _gravity;
          private float _groundedGravity;
          private bool _isGrounded;
          
          // Control animation state
          private bool _isDance;
          private bool _isPunch;
          private bool _isKick;
          
          // Attack condition
          private bool _canPunch = true;
          private bool _canKick = false;
          
          // Timer
          private float _dieTimer;
          
          // Constants
          private const int ZERO = 0;
          private const float FALL_DIZZY = -10F;
          private const float FALL_DISTANCE = -40F;

          // Camera
          private Transform _CameraTransform;
          
          // RagDoll character physics controller
          [Header("RagDoll Character")]
          [Tooltip("Force Magnitude To Push Obstacle")] [field: SerializeField] private float _forceMagnitude = 1f;
          private PlayerRagDollCharacterController _PlayerRagDoll;

          private float _distanceToTheGround;

          private void Awake()
          {
               _PlayerRb = GetComponent(typeof(Rigidbody)) as Rigidbody;

               _PlayerRagDoll = FindObjectOfType<PlayerRagDollCharacterController>();

               _PlayerInputController = new PlayerInputSystemController();
               
               PlayerInputSystemActionCallback();
          }

          // Start is called before the first frame update
          private void Start()
          {
               if (Camera.main != null)
               {
                    _CameraTransform = Camera.main.transform;
               }

               // Get character distance to the ground
               _distanceToTheGround = GetComponent<Collider>().bounds.extents.y;

               _isMove = true;
               _isJump = true;
               _isJumpPressed = false;
               _isJumpAnimating = false;
               _isDance = true;
               _isPunch = true;
               _isKick = true;

               _gravity = -9.81F;
               _groundedGravity = -0.05F;

               _dieTimer = 2.0f;
          }

          // Update is called once per frame
          private void Update()
          {
               // Take player's movement input action 
               _PlayerCharacterMovementVelocity = new Vector3(_PlayerMovementInput.x, 0f, _PlayerMovementInput.y).normalized;
               
               PlayerJump();
               
               this.RespawnPlayer();
          }

          // Fixed Update is used for physics calculation
          private void FixedUpdate()
          {
               PlayerMovement();
               
               // Calculate fast fall of player gravity
               if (TryGetComponent(out Rigidbody PlayerRb))
               {
                    if (PlayerRb.velocity.y < 0f)
                    {
                         PlayerRb.velocity += Vector3.up * Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
                    }
               }
          }

          private void OnEnable()
          {
               _PlayerInputController.PlayerCharacterController.Enable();
          }

          private void OnDisable()
          {
               _PlayerInputController.PlayerCharacterController.Disable();
          }
          
          private void OnApplicationFocus(bool hasFocus)
          {
               // Lock cursor during play mode
               Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
               
               /*
               if (hasFocus)
               {
                    Cursor.lockState = CursorLockMode.Locked;
               }
               else
               {
                    Cursor.lockState = CursorLockMode.None;
               }
               */
          }

          /// <summary>
          /// Initialize the new player input system action callback
          /// </summary>
          private void PlayerInputSystemActionCallback()
          {
               // Movement input action
               _PlayerInputController.PlayerCharacterController.Movement.started += OnMovement;
               _PlayerInputController.PlayerCharacterController.Movement.canceled += OnMovement;
               _PlayerInputController.PlayerCharacterController.Movement.performed += OnMovement;
               
               // Run movement input action
               _PlayerInputController.PlayerCharacterController.Run.started += OnRun;
               _PlayerInputController.PlayerCharacterController.Run.canceled += OnRun;
               
               // Jump input action
               _PlayerInputController.PlayerCharacterController.Jump.started += OnJump;
               _PlayerInputController.PlayerCharacterController.Jump.canceled += OnJump;
               
               InitializeJump();

               // Dance input action
               _PlayerInputController.PlayerCharacterController.Dance.started += OnDance;
               _PlayerInputController.PlayerCharacterController.Dance.canceled += OnDance;
               
               // Punch input action
               if (_canPunch)
               {
                    _PlayerInputController.PlayerCharacterController.Punch.performed += OnPunch;
                    _PlayerInputController.PlayerCharacterController.Punch.canceled += OnPunch;
               }

               // Kick input action
               _PlayerInputController.PlayerCharacterController.Kick.performed += OnKick;
               _PlayerInputController.PlayerCharacterController.Kick.canceled += OnKick;
          }

          /// <summary>
          /// Player movement input action callback
          /// </summary>
          /// <param name="movementContext">InputAction.CallbackContext</param>
          public void OnMovement(InputAction.CallbackContext movementContext)
          {
               if (_isMove)
               {
                    _PlayerMovementInput = movementContext.ReadValue<Vector2>();

                    // Player move
                    _CurrentMovement.x = _PlayerMovementInput.x * _movementSpeed;
                    _CurrentMovement.z = _PlayerMovementInput.y * _movementSpeed;

                    // Player run
                    _CurrentRunMovement.x = _PlayerMovementInput.x * _runMovementSpeed;
                    _CurrentRunMovement.z = _PlayerMovementInput.y * _runMovementSpeed;
                    
                    _isMovementPressed = _PlayerMovementInput.x != 0F || _PlayerMovementInput.y != 0F;

                    // Debug.Log(movementContext.ReadValue<Vector2>()); // DEBUG
               }
          }

          /// <summary>
          /// Player run input action callback
          /// </summary>
          /// <param name="runMovementContext">InputAction.CallbackContext</param>
          public void OnRun(InputAction.CallbackContext runMovementContext)
          {
               _isRunPressed = runMovementContext.ReadValueAsButton();
               
               // Debug.Log($"Player run: {_isRunPressed}");
          }

          /// <summary>
          /// Player jump input action callback
          /// </summary>
          /// <param name="jumpContext">InputAction.CallbackContext</param>
          public void OnJump(InputAction.CallbackContext jumpContext)
          {
               if (_isJump)
               {
                    _isJumpPressed = jumpContext.ReadValueAsButton();

                    Debug.Log($"Jump: {_isJumpPressed}"); // DEBUG
               }
          }
          
          /// <summary>
          /// Player dance input action callback
          /// </summary>
          /// <param name="danceContext">InputAction.CallbackContext</param>
          public void OnDance(InputAction.CallbackContext danceContext)
          {
               if (_isDance)
               {
                    if (danceContext.started)
                    {
                         PlayerAnimation.Instance.DanceAnimation(true);
                    }
                    else if (danceContext.canceled)
                    {
                         PlayerAnimation.Instance.DanceAnimation(false);
                    }
               }
          }

          /// <summary>
          /// Player punch input action callback 
          /// </summary>
          /// <param name="punchContext">InputAction.CallbackContext</param>
          public void OnPunch(InputAction.CallbackContext punchContext)
          {
               if (_isPunch && _canPunch)
               {
                    if (punchContext.performed)
                    {
                         PlayerAnimation.Instance.PunchAnimation();
                    }
               }
          }

          /// <summary>
          /// Player kicked input action callback
          /// </summary>
          /// <param name="kickContext">InputAction.CallbackContext</param>
          public void OnKick(InputAction.CallbackContext kickContext)
          {
               if (_isKick && _canKick)
               {
                    if (kickContext.performed)
                    {
                         PlayerAnimation.Instance.KickAnimation();

                         // Debug.Log(kickContext.performed); // DEBUG
                    }
               }
          }

          /// <summary>
          /// Player mouse look input action callback
          /// </summary>
          /// <param name="onLookContext">InputAction.CallbackContext</param>
          public void OnLook(InputAction.CallbackContext onLookContext)
          {
               onLookContext.ReadValueAsButton();
          }

          /// <summary>
          /// Player movement controller
          /// </summary>
          private void PlayerMovement()
          {
               if (_isMove)
               {
                    if (_isRunPressed)
                    {
                         // Set player run velocity
                         _PlayerCharacterMovementVelocity.x = _CurrentRunMovement.x;
                         _PlayerCharacterMovementVelocity.z = _CurrentRunMovement.z;
                         
                         // _PlayerRb.velocity = _CurrentRunMovement * Time.fixedDeltaTime + new Vector3(0f, _PlayerRb.velocity.y, 0f);
                    }
                    else
                    {
                         // Set player movement velocity
                         _PlayerCharacterMovementVelocity.x = _CurrentMovement.x;
                         _PlayerCharacterMovementVelocity.z = _CurrentMovement.z;
                         
                         // _PlayerRb.velocity = _CurrentMovement * Time.fixedDeltaTime + new Vector3(0f, _PlayerRb.velocity.y, 0f);
                    }
                    
                    // Follow the player direction with the angle of main camera
                    _PlayerCharacterMovementVelocity = Quaternion.AngleAxis(_CameraTransform.rotation.eulerAngles.y, Vector3.Normalize(Vector3.up)) * _PlayerCharacterMovementVelocity;

                    // Move the player's character with Rigidbody (physics) 
                    _PlayerRb.velocity = _PlayerCharacterMovementVelocity * Time.fixedDeltaTime + new Vector3(0f, _PlayerRb.velocity.y, 0f);

                    RotatePlayerDirection();
                    
                    PlayerAnimation.Instance.MovementAnimation(_isMovementPressed);
                    
                    PlayerAnimation.Instance.RunAnimation(_isMovementPressed, _isRunPressed);
                    
                    // Checking for kick action when player is move or running
                    if (_isMovementPressed || _isRunPressed)
                    { 
                         _canKick = true;
                    }
                    else
                    {
                         _canKick = false;
                    }
               }
          }

          /// <summary>
          /// Rotate player direction by movement input action
          /// </summary>
          private void RotatePlayerDirection()
          {
               Vector3 PositionToLookAt;
               
               PositionToLookAt.x = _PlayerCharacterMovementVelocity.x;
               PositionToLookAt.y = (float)ZERO;
               PositionToLookAt.z = _PlayerCharacterMovementVelocity.z;
               
               /*
                // Default position to look at
               PositionToLookAt.x = _CurrentMovement.x;
               PositionToLookAt.y = (float)ZERO;
               PositionToLookAt.z = _CurrentMovement.z;
               */

               Quaternion CurrentRotation = gameObject.transform.rotation;
               
               // Rotate the player direction
               if (_isMovementPressed)
               {
                    Quaternion RotatePlayerDirection = Quaternion.LookRotation(Vector3.Normalize(PositionToLookAt * Time.fixedDeltaTime));
                    
                    // Rotate with Rigidbody physics
                    _PlayerRb.MoveRotation(Quaternion.Slerp(CurrentRotation.normalized, RotatePlayerDirection.normalized, Mathf.Sin(_rotationSpeed * Time.fixedDeltaTime)));
                    
                    // Rotate with transform
                    // gameObject.transform.rotation = Quaternion.Slerp(CurrentRotation.normalized, RotatePlayerDirection.normalized, Mathf.Sin(_rotationSpeed * Time.fixedDeltaTime));
               }
          }

          /// <summary>
          /// Player jump controller
          /// </summary>
          private void PlayerJump()
          {
               this.Gravity();
               
               if (!_isJumping && _isJumpPressed == !false && _isGrounded)
               {
                    _isJumping = true;
                    _isJumpAnimating = true;

                    _canPunch = false;

                    PlayerAnimation.Instance.JumpAnimation(_isJumpPressed);
                    
                    _canPlayJumpSFX = true;
                    
                    if (_canPlayJumpSFX)
                    {
                         SoundEffectManager.Instance.PlaySoundEffect("Jump", true);
                    }
                    
                    // Debug.Log("Player jump"); // DEBUG

                    // Jump movement
                    _CurrentMovement.y = _jumpVelocity;
                    _PlayerCharacterMovementVelocity.y = _jumpVelocity;
                    
                    // Applied the player jump action with Rigidbody
                    _PlayerRb.AddForce(_PlayerCharacterMovementVelocity * _maxJumpHeight, ForceMode.Impulse);
                    
                    // _PlayerRb.velocity = _CurrentMovement;
                    // _PlayerRb.velocity = _PlayerCharacterMovementVelocity;
               }
               else if (!_isJumpPressed && _isJumping == true && _isGrounded)
               {
                    _isJumping = false;
                    _canPunch = true;

                    // print("Stop jumping: " + _isJumping); // PRINT
               }
          }
          
          /// <summary>
          /// Initialize and calculate the jump variable
          /// </summary>
          private void InitializeJump()
          {
               var timeApex = _maxJumpTime / 2F;

               // Jump velocity and gravity
               _jumpVelocity = Mathf.Max(2f * (_maxJumpHeight + 2f)) / Mathf.Cos(timeApex * 2f); // Calculate the jump velocity
               _gravity = (-2f * (_maxJumpHeight + 3f)) / Mathf.Pow((timeApex * 2f), 2f); // Calculate the gravity(fall) of an object
               
               /*
                // Concept
               _gravity = (-2f * _maxJumpHeight) / Mathf.Pow(timeApex, 2f);
               _jumpVelocity = (2f * _maxJumpHeight) / timeApex;
               */
          }

          /// <summary>
          /// Gravity of player character
          /// </summary>
          private void Gravity()
          {
               Boolean isFalling = _CurrentMovement.y <= 0.0f || !_isJumpPressed;
               
               if (_isGrounded)
               {
                    if (_isJumpAnimating)
                    {
                         PlayerAnimation.Instance.JumpAnimation(_isJumpPressed);
                    }
                    
                    _CurrentMovement.y = _groundedGravity;
                    
                    _PlayerCharacterMovementVelocity.y = _groundedGravity;
                     
                    // Debug.Log($"{gameObject.name} is on the ground"); // DEBUG
               }
               else if (isFalling)
               {
                    var previousYVelocity = _CurrentMovement.y;
                    
                    _CurrentMovement.y = previousYVelocity + (_gravity * _gravityMultiplier * Time.deltaTime);
                    
                    // Default applied movement
                    // _PlayerMovement.y = (previousYVelocity + _CurrentMovement.y) * 0.5F;

                    // Optional next velocity of y with max value
                    _PlayerCharacterMovementVelocity.y = Mathf.Max((previousYVelocity + _CurrentMovement.y) * 0.5f, -20.0f);
               }
               else
               {
                    var previousYVelocity = _CurrentMovement.y;
                    
                    _CurrentMovement.y = previousYVelocity + (_gravity * Time.deltaTime);
                    
                    _PlayerCharacterMovementVelocity.y = (previousYVelocity + _CurrentMovement.y) * 0.5f;
               }
          }

          private void OnCollisionEnter(Collision collision)
          {
               PlayerGetHit(collision);

               var PlayerRb = collision.collider.attachedRigidbody;

               if (PlayerRb == null)
                    return;
               
               if (PlayerRb.gameObject.CompareTag(TagsManager.Rotator))
               {
                    _PlayerRagDoll.ActivateRagDollCharacter();
                    
                    Debug.Log($"Collision with {collision.gameObject.name}");
               }
          }

          private void OnCollisionStay(Collision collisionInfo)
          {
               if (collisionInfo.collider)
               {
                    _isGrounded = true;
               }
          }

          private void OnCollisionExit(Collision other)
          {
               if (other.collider)
               {
                    _isGrounded = false;
               }
          }

          /// <summary>
          /// Add force while player's character get hit with obstacles
          /// </summary>
          /// <param name="collision">Collision</param>
          private void PlayerGetHit(Collision collision)
          {
               Rigidbody PlayerRb = collision.collider.attachedRigidbody;

               if (PlayerRb != null)
               {
                    Vector3 ForceDirection = collision.transform.position - gameObject.transform.position;

                    ForceDirection.y = (float)ZERO;

                    ForceDirection.Normalize();

                    PlayerRb.AddForceAtPosition(ForceDirection.normalized * _forceMagnitude, transform.position, ForceMode.Impulse);

                    // Debug.Log($"Force {gameObject.name}"); // DEBUG
               }
          }

          /// <summary>
          /// Check if the player's character is grounded or not
          /// </summary>
          /// <returns>bool (Physics.Raycast)</returns>
          private Boolean IsGrounded()
          {
               Debug.DrawRay(_PlayerRb.transform.position, -Vector3.up, Color.blue); // DEBUG RAY
               
               return Physics.Raycast(_PlayerRb.position, -Vector3.up, _distanceToTheGround + 0.1F);
          }

          /// <summary>
          /// Player character's Collider component
          /// </summary>
          public Collider PlayerCollider => TryGetComponent<Collider>(out var PlayerCollider) ? PlayerCollider : null;

          /// <summary>
          /// Player character's Rigidbody component
          /// </summary>
          public Rigidbody PlayerRigidbody => TryGetComponent<Rigidbody>(out var PlayerRb) ? PlayerRb : null;

          /// <summary>
          /// Respawn player when falling out of the level
          /// </summary>
          /// <exception cref="System.NullReferenceException">throw null reference</exception>
          private void RespawnPlayer()
          {
               if (TryGetComponent<Rigidbody>(out Rigidbody PlayerRb))
               {
                    // With Rigidbody
                    Vector3 PlayerPosition = PlayerRb.transform.position;

                    // Reset position value if the player is grounded
                    if (IsGrounded())
                    {
                         PlayerPosition.y = (float)ZERO;
                         
                         // Debug.LogAssertion(PlayerPosition.y); // DEBUG ASSERTION
                    }

                    if (PlayerPosition.y < FALL_DIZZY)
                    {
                         _isJump = false;
                         
                         PlayerAnimation.Instance.DizzyAnimation();
                         
                         // Debug.Log("Player dizzy"); // DEBUG
                    }

                    if (PlayerPosition.y < FALL_DISTANCE)
                    {
                         _isJump = false;
                         
                         PlayerRb.isKinematic = true;

                         _dieTimer -= Time.deltaTime;

                         if (_dieTimer <= 0F)
                         {
                              SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
                         }

                         PlayerAnimation.Instance.DieAnimation();

                         // Debug.Log($"Player {gameObject.transform.name} falling"); // DEBUG
                    }
               }
               else
               {
                    throw new System.NullReferenceException("Component hasn't been attached!");
               }
          }
     }
}