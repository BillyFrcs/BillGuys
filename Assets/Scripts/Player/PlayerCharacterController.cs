using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Helpers.Tags;
using Levels;
#if ENABLE_INPUT_SYSTEM
using Player.InputSystem;
#endif
using Sounds.SFX;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Interactions;
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
          private Vector2 _PlayerMovementInput;
          private Vector3 _CurrentMovement;
          private Vector3 _PlayerCharacterMovementVelocity; // This is for applied player movement
          private Rigidbody _PlayerRb;
          private bool _isMove;
          private bool _isMovementPressed;

          [Header("Rotate Object Direction")]
          [Tooltip("Rotation Speed To Rotate Player Direction")] [SerializeField] [Range(0, 20)] private float _rotationSpeed = 15f;

          [Header("Jump And Gravity Object")]
          [Tooltip("Set Max Jump Height With Speed")] [field: SerializeField] private float _maxJumpHeight = 1.0f;
          [Tooltip("Set Max Jump Time")] [field: SerializeField] private float _maxJumpTime = 0.75f;
          private float _jumpVelocity;
          private int _jump;
          private int _jumpCount;
          private bool _isJump;
          private bool _isJumpPressed;
          private bool _isJumping;
          private bool _isJumpAnimating;
          private bool _canPlayJumpSFX;
          private bool _canDoubleJump;

          // Gravity
          [Tooltip("Gravity Falling Multiplier Of An Object")] [SerializeField] [Range(0f, 10f)] private float _gravityMultiplier = 1.0f;
          private float _gravity;
          private float _groundedGravity;
          
          // Control animation state
          private bool _isDance;
          private bool _isKick;
          
          // Attack condition
          private bool _canKick = true;
          private bool _isKickPressed = false;

          // Constants
          private const int Zero = 0;
          private const float FallDizzy = -5F;
          private const float FallDistance = -40F;
          private const int MaxJump = 2;

          // Camera
          private Transform _CameraTransform;
          
          // RagDoll character physics controller
          [Header("RagDoll Character")]
          [Tooltip("Force Magnitude To Push Obstacle")] [field: SerializeField] private float _forceMagnitude = 1f;
          private PlayerRagDollCharacterController _PlayerRagDoll;

          private bool _isGrounded;
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
               _isKick = true;

               _gravity = -9.81F;
               _groundedGravity = -0.05F;
          }

          // Update is called once per frame
          private void Update()
          {
               // Take player's movement input action 
               _PlayerCharacterMovementVelocity = new Vector3(_PlayerMovementInput.x, 0f, _PlayerMovementInput.y).normalized;
               
               PlayerJump();

               PlayerAttack();

               this.RespawnPlayer();
               
#if ENABLE_INPUT_SYSTEM
              if (Input.GetKeyDown(KeyCode.Q))
               {
                    Application.Quit();
                    
                    Debug.Log("Quit Game!"); // DEBUG
               }
#endif
          }

          // Fixed Update is used for physics calculation
          private void FixedUpdate()
          {
               PlayerMovement();
               
               // Calculate fast fall of player gravity
               if (!TryGetComponent(out Rigidbody playerRb)) 
                    return;
               
               if (playerRb.velocity.y < 0f)
               {
                    playerRb.AddForce(Vector3.up * (Physics.gravity.y * _gravityMultiplier * Time.fixedDeltaTime), ForceMode.Impulse);
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

               // Jump input action
               _PlayerInputController.PlayerCharacterController.Jump.started += OnJump;
               _PlayerInputController.PlayerCharacterController.Jump.canceled += OnJump;
               
               InitializeJump();

               // Dance input action
               _PlayerInputController.PlayerCharacterController.Dance.started += OnDance;
               _PlayerInputController.PlayerCharacterController.Dance.canceled += OnDance;

               // Kick input action
               _PlayerInputController.PlayerCharacterController.Kick.started += OnKick;
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

                    // Player movement
                    _CurrentMovement.x = _PlayerMovementInput.x * _movementSpeed;
                    _CurrentMovement.z = _PlayerMovementInput.y * _movementSpeed;

                    _isMovementPressed = _PlayerMovementInput.x != 0F || _PlayerMovementInput.y != 0F;

                    // Debug.Log(movementContext.ReadValue<Vector2>()); // DEBUG
               }
          }

          /// <summary>
          /// Player jump input action callback
          /// </summary>
          /// <param name="jumpContext">InputAction.CallbackContext</param>
          public void OnJump(InputAction.CallbackContext jumpContext)
          {
               if (_isJump)
               {
                    if (jumpContext.interaction is TapInteraction)
                    {
                         _isJumpPressed = jumpContext.ReadValueAsButton();
                    }

                    // Debug.Log($"Jump: {_isJumpPressed}"); // DEBUG
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
          /// Player kicked input action callback
          /// </summary>
          /// <param name="kickContext">InputAction.CallbackContext</param>
          public void OnKick(InputAction.CallbackContext kickContext)
          {
               if (_isKick && _canKick)
               {
                   _isKickPressed = kickContext.ReadValueAsButton();

                   if (kickContext.started)
                   {
                       PlayerAnimation.Instance.KickAnimation();

                       SoundEffectManager.Instance.PlaySoundEffect("Kick", true);

                       // Debug.Log("Kick: " + _isKickPressed); // DEBUG
                   }
                   else if (kickContext.canceled)
                   {
                       SoundEffectManager.Instance.PlaySoundEffect("Kick", false);
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
                    if (_isMovementPressed)
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

                    _isDance = !_isMovementPressed;
               }
          }
          
          /// <summary>
          /// Rotate player direction by movement input action
          /// </summary>
          private void RotatePlayerDirection()
          {
               Vector3 positionToLookAt;
               
               positionToLookAt.x = _PlayerCharacterMovementVelocity.x;
               positionToLookAt.y = (float)Zero;
               positionToLookAt.z = _PlayerCharacterMovementVelocity.z;
               
               /*
               // Default position to look at
               positionToLookAt.x = _CurrentMovement.x;
               positionToLookAt.y = (float)ZERO;
               positionToLookAt.z = _CurrentMovement.z;
               */

               Quaternion currentRotation = gameObject.transform.rotation;
               
               // Rotate the player direction
               if (_isMovementPressed)
               {
                    Quaternion rotatePlayerDirection = Quaternion.LookRotation(Vector3.Normalize(positionToLookAt * Time.fixedDeltaTime));
                    
                    // Rotate with Rigidbody rotation
                    _PlayerRb.MoveRotation(Quaternion.Slerp(currentRotation.normalized, rotatePlayerDirection.normalized, Mathf.Sin(_rotationSpeed * Time.fixedDeltaTime)));
                    
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

               if (_isJump)
               {
                    if (!_isJumping && _isJumpPressed != !true && _jump > 0)
                    {
                         _isJumping = true;
                         _isJumpAnimating = true;
                         
                         _isDance = false;

                         _jump -= 2;

                         PlayerAnimation.Instance.JumpAnimation(_isJumpPressed);

                         _canPlayJumpSFX = true;

                         if (_canPlayJumpSFX)
                         {
                              SoundEffectManager.Instance.PlaySoundEffect("Jump", true);
                              
                              // Debug.Log("Jump SFX"); // DEBUG
                         }

                         _canDoubleJump = true;
                         
                         if (_canDoubleJump && _isGrounded == false)
                         {
                              _jumpCount++;
                         
                              // Debug.Log($"Jump count: {_jumpCount}"); // DEBUG
                              
                              PlayerAnimation.Instance.SlideAnimation(_canDoubleJump);
                              
                              // Debug.Log("Start double jump"); // DEBUG
                              
                              if (_jumpCount >= 1)
                              {
                                   _canPlayJumpSFX = false;
                                   
                                   PlayerAnimation.Instance.SlideAnimation(_canDoubleJump);

                                   SoundEffectManager.Instance.PlaySoundEffect("Jump", false);

                                   SoundEffectManager.Instance.PlaySoundEffect("Slide", true);
                                   
                                   // Debug.Log($"Play jump SFX {_canPlayJumpSFX}"); // DEBUG
                                   
                                   // Debug.Log("Start double jump"); // DEBUG
                              }
                         }

                         // Debug.Log("Player jump"); // DEBUG

                         // Jump movement
                         _CurrentMovement.y = _jumpVelocity;
                         _PlayerCharacterMovementVelocity.y = _jumpVelocity;

                         // Applied the player jump action with Rigidbody Add Force
                         _PlayerRb.AddForce(_PlayerCharacterMovementVelocity * _maxJumpHeight, ForceMode.Impulse);
                         
                         // Applied the player jump action with Rigidbody velocity
                         // _PlayerRb.velocity = _CurrentMovement;
                         // _PlayerRb.velocity = _PlayerCharacterMovementVelocity;
                    }
                    else if (!_isJumpPressed && _isJumping != !true && _jump > 0)
                    {
                         _isJumping = false;
                         
                         // _canKick = true;
                         _isDance = true;

                         // Debug.Log("Stop jumping: " + _isJumping); // DEBUG
                    }
                    
                    if (_jump == 0)
                         return;
               }
          }
          
          /// <summary>
          /// Initialize and calculate the jump variable
          /// </summary>
          private void InitializeJump()
          {
               var timeApex = _maxJumpTime / 2F;

               // Jump velocity and gravity
               _jumpVelocity = Mathf.Max(2f * (_maxJumpHeight + 1f)) / Mathf.Cos(timeApex * 2f); // Calculate the jump velocity
               _gravity = -2f * (_maxJumpHeight + 3f) / Mathf.Pow((timeApex * 2f), 2f); // Calculate the gravity(fall) of an object
               
               /*
                // Concept
               _jumpVelocity = (2f * _maxJumpHeight) / timeApex;
               _gravity = (-2f * _maxJumpHeight) / Mathf.Pow(timeApex, 2f);
               */
          }

          /// <summary>
          /// Gravity of player character
          /// </summary>
          private void Gravity()
          {
               Boolean isFalling = _CurrentMovement.y <= 0.0f || !_isJumpPressed || !_canDoubleJump;
               
               if (_isGrounded)
               {
                    if (_isJumpAnimating)
                    {
                         PlayerAnimation.Instance.JumpAnimation(_isJumpPressed);
                         PlayerAnimation.Instance.SlideAnimation(_canDoubleJump);

                         if (_jumpCount == 2)
                         {
                              _jumpCount = Zero;

                              // Debug.Log($"Reset jump count {_jumpCount}"); // DEBUG
                         }
                    }
                    
                    _CurrentMovement.y = _groundedGravity;
                    
                    _PlayerCharacterMovementVelocity.y = _groundedGravity;
                     
                    // Debug.Log($"{gameObject.name} is on the ground"); // DEBUG
               }
               else if (isFalling)
               {
                    var previousYVelocity = _CurrentMovement.y;
                    
                    _CurrentMovement.y = previousYVelocity + _gravity * _gravityMultiplier * Time.deltaTime;
                    
                    // Default applied movement
                    // _PlayerCharacterMovementVelocity.y = (previousYVelocity + _CurrentMovement.y) * 0.5F;

                    // Optional to applied movement with max value
                    _PlayerCharacterMovementVelocity.y = Mathf.Max((previousYVelocity + _CurrentMovement.y) * 0.5F, -20.0F);
                    
                    // Debug.Log($"{gameObject.name} is falling!"); // DEBUG
               }
               else
               {
                    var previousYVelocity = _CurrentMovement.y;
                    
                    _CurrentMovement.y = previousYVelocity + _gravity * Time.deltaTime;
                    
                    _PlayerCharacterMovementVelocity.y = (previousYVelocity + _CurrentMovement.y) * 0.5f;
               }
          }

          /// <summary>
          /// Player attack controller
          /// </summary>
          private void PlayerAttack()
          {
               // Player kicking
               _isJump = !_isKickPressed;
          }

          private void OnCollisionEnter(Collision collision)
          {
               var playerRb = collision.collider.attachedRigidbody;
               
               PlayerHitObstacles(collision, playerRb);

               if (playerRb == null)
                    return;
               
               if (playerRb.gameObject.CompareTag(TagManager.Rotator))
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
                    _canDoubleJump = false;
                    _canKick = true;
                    
                    // Start kicking if player is move on the ground
                    _canKick = _isMovementPressed;
                    
                    _jump = MaxJump;
                    
                    // Debug.Log($"Is grounded {_isGrounded}"); // DEBUG
               }
          }

          private void OnCollisionExit(Collision other)
          {
               if (other.collider)
               {
                    _isGrounded = false;
                    _canKick = false;

                    // Debug.Log($"Is grounded {_isGrounded}"); // DEBUG
               }
          }

          /// <summary>
          /// Add force while player's character get hit with obstacles
          /// </summary>
          /// <param name="collision">Collision</param>
          /// <param name="playerRb">Rigidbody</param>
          private void PlayerHitObstacles(Collision collision, Rigidbody playerRb)
          {
               // Force with all obstacle
               if (playerRb != null)
               {
                   AppliedForce(collision, playerRb);

                   Debug.Log("Force obstacle"); // DEBUG
               }
          }

          /// <summary>
          /// Forced player with physics when get hit
          /// </summary>
          public void PlayerGetHit()
          {
               throw new NotImplementedException();
          }

          /// <summary>
          /// Applied force affect to obstacles 
          /// </summary>
          /// <param name="collision">Collision info</param>
          /// <param name="playerRb">Player's Rigidbody</param>
          private void AppliedForce(Collision collision, Rigidbody playerRb)
          {
              Vector3 forceDirection = collision.transform.position - gameObject.transform.position;
              
              forceDirection.y = (float)Zero;
              
              forceDirection.Normalize();
              
              playerRb.AddForceAtPosition(forceDirection.normalized * _forceMagnitude, transform.position, ForceMode.Impulse);
          }

          /// <summary>
          /// Check if the player's character is grounded or not
          /// </summary>
          /// <returns>bool (Physics.Raycast)</returns>
          private Boolean IsGrounded()
          { 
               Debug.DrawRay(_PlayerRb.transform.position, Vector3.down, Color.blue); // DEBUG RAY
               
               return Physics.Raycast(_PlayerRb.transform.position, Vector3.down, _distanceToTheGround + 0.1F);
          }

          /// <summary>
          /// Player character's Collider component
          /// </summary>
          public Collider PlayerCollider => TryGetComponent<Collider>(out var playerCollider) ? playerCollider : null;

          /// <summary>
          /// Player character's Rigidbody component
          /// </summary>
          public Rigidbody PlayerRigidbody => TryGetComponent<Rigidbody>(out var playerRb) ? playerRb : null;

          /// <summary>
          /// Respawn player when falling out of the level
          /// </summary>
          /// <exception cref="System.NullReferenceException">throw null reference</exception>
          private void RespawnPlayer()
          {
               if (TryGetComponent<Rigidbody>(out Rigidbody playerRb))
               {
                    // With Rigidbody
                    Vector3 playerPosition = playerRb.transform.position;

                    // Reset position value if the player is grounded
                    if (IsGrounded())
                    {
                         playerPosition.y = (float)Zero;
                         
                         // Debug.LogAssertion(PlayerPosition.y); // DEBUG ASSERTION
                    }

                    if (playerPosition.y < FallDizzy)
                    {
                         _isJump = false;
                         
                         PlayerAnimation.Instance.DizzyAnimation();
                         
                         // Debug.Log("Player dizzy"); // DEBUG
                    }

                    if (playerPosition.y < FallDistance)
                    {
                         _isJump = false;
                         
                         playerRb.isKinematic = true;
                         
                         Invoke(nameof(DieTimer), 1.0f);

                         PlayerAnimation.Instance.DieAnimation();

                         // Debug.Log($"Player {gameObject.transform.name} falling"); // DEBUG
                    }
               }
               else
               {
                    throw new System.NullReferenceException("Component hasn't been attached!");
               }
          }
          
          /// <summary>
          /// Die timer to respawn player
          /// </summary>
          private void DieTimer()
          {
               SceneManager.LoadScene(SceneManager.GetActiveScene().name);
          }
     }
}