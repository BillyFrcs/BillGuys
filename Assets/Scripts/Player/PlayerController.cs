using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Player.InputSystem;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

namespace Player
{
     [RequireComponent(typeof(CharacterController))]
     [RequireComponent(typeof(Rigidbody))]
     [RequireComponent(typeof(PlayerAnimation))]
     public class PlayerController : MonoBehaviour, PlayerInputSystemController.IPlayerCharacterControllerActions
     {
          // Player controller component
          private CharacterController _PlayerCharacterController;
          private PlayerInputSystemController _PlayerInputController;
          
          // Velocity of player character
          private Vector3 _PlayerVelocity;

          [Header("Player Movement Controller")]
          [Tooltip("Default Movement Speed Of Player Character")] [SerializeField] private float _movementSpeed;
          [Tooltip("Run Movement Speed Of Player Character")] [SerializeField] private float _runMovementSpeed;
          private Vector2 _CurrentMovementInput;
          private Vector3 _CurrentMovement;
          private Vector3 _CurrentRunMovement;
          private Vector3 _AppliedPlayerMovement;
          private bool _isMove;
          private bool _isMovementPressed;
          private bool _isRunPressed;
          
          [Header("Rotate Object Direction")]
          [Tooltip("Rotation Speed To Rotate Player Direction")] [SerializeField] [Range(0, 20)] private float _rotationSpeed = 15f;

          [Header("Jump And Gravity Object")] 
          [Tooltip("Set Max Jump Height")] [SerializeField] private float _maxJumpHeight = 4.0f;
          [Tooltip("Set Max Jump Time")] [SerializeField] private float _maxJumpTime = 0.75f;
          private float _jumpVelocity;
          private bool _isJump;
          private bool _isJumpPressed;
          private bool _isJumping;
          private bool _isJumpAnimating;

          // Trajectory jump velocity
          private Dictionary<int, float> _InitialJumpVelocity = new Dictionary<int, float>();
          private Dictionary<int, float> _InitialJumpGravity = new Dictionary<int, float>();
          private Coroutine _CurrentJumpResetRoutine = null;
          private int _jumpCounter;

          // Gravity
          private float _gravity;
          private float _groundedGravity;
          [Tooltip("Gravity Falling Multiplier Of An Object")] [SerializeField] [Range(0f, 5f)] private float _gravityMultiplier = 1.0f;
          
          // Control animation state
          private bool _isDance;
          private bool _isPunch;
          private bool _isKick;
          
          // Timer
          private float _dieTimer;
          
          // Constants ----------------------------------------------------------------
          private const int ZERO = 0;
          private const float FALL_DIZZY = -10F;
          private const float FALL_DISTANCE = -40F;

          private void Awake()
          {
               _PlayerCharacterController = gameObject.GetComponent(typeof(CharacterController)) as CharacterController;

               _PlayerInputController = new PlayerInputSystemController();
               
               PlayerInputSystemActionCallback();
          }

          // Start is called before the first frame update
          private void Start()
          {
               _isMove = true;
               _isJump = true;
               _isJumpPressed = false;
               _isJumping = false;
               _isJumpAnimating = false;
               _isDance = true;
               _isPunch = true;
               _isKick = false;

               _gravity = -9.8F;
               _groundedGravity = -0.05F;

               _dieTimer = 2.0f;
          }

          // Update is called once per frame
          private void Update()
          {
               this.PlayerMovement();
               
               this.PlayerJump();
               
               RespawnPlayer();
          }

          // Fixed Update is used for physics calculation
          private void FixedUpdate()
          {
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
               if (hasFocus)
               {
                    Cursor.lockState = CursorLockMode.Locked;
               }
               else
               {
                    Cursor.lockState = CursorLockMode.None;
               }
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
               _PlayerInputController.PlayerCharacterController.Punch.performed+= OnPunch;
               _PlayerInputController.PlayerCharacterController.Punch.canceled += OnPunch;
               
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
                    _CurrentMovementInput = movementContext.ReadValue<Vector2>();

                    // Player move
                    _CurrentMovement.x = _CurrentMovementInput.x * _movementSpeed;
                    _CurrentMovement.z = _CurrentMovementInput.y * _movementSpeed;

                    // Player run
                    _CurrentRunMovement.x = _CurrentMovementInput.x * _runMovementSpeed;
                    _CurrentRunMovement.z = _CurrentMovementInput.y * _runMovementSpeed;

                    _isMovementPressed = _CurrentMovementInput.x != 0F || _CurrentMovementInput.y != 0F;

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
          /// Player punch input action callback 
          /// </summary>
          /// <param name="punchContext">InputAction.CallbackContext</param>
          public void OnPunch(InputAction.CallbackContext punchContext)
          {
               if (_isPunch)
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
               if (kickContext.performed)
               {
                    if (_isKick)
                    {
                         PlayerAnimation.Instance.KickAnimation();

                         // Debug.Log(kickContext.performed); // DEBUG
                    }
               }
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
                         // Run with high movement speed
                         _AppliedPlayerMovement.x = _CurrentRunMovement.x;
                         _AppliedPlayerMovement.z = _CurrentRunMovement.z;
                         
                         // _PlayerCharacterController.Move(_CurrentRunMovement * Time.deltaTime);
                    }
                    else
                    {
                         // Run with default movement speed
                         _AppliedPlayerMovement.x = _CurrentMovement.x;
                         _AppliedPlayerMovement.z = _CurrentMovement.z;
                         
                         // _PlayerCharacterController.Move(_CurrentMovement * Time.deltaTime);

                         _isKick = true;
                    }
                    
                    // Move player character
                    _PlayerCharacterController.Move(_AppliedPlayerMovement * Time.deltaTime);

                    RotatePlayerDirection();

                    PlayerAnimation.Instance.MovementAnimation(_isMovementPressed);

                    PlayerAnimation.Instance.RunAnimation(_isMovementPressed, _isRunPressed);
               }
          }

          /// <summary>
          /// Rotate player direction by movement input action
          /// </summary>
          private void RotatePlayerDirection()
          {
               Vector3 PositionToLookAt;

               PositionToLookAt.x = _CurrentMovement.x;
               PositionToLookAt.y = (float)ZERO;
               PositionToLookAt.z = _CurrentMovement.z;

               Quaternion CurrentRotation = gameObject.transform.rotation;

               if (_isMovementPressed)
               {
                    Quaternion PlayerRotation = Quaternion.LookRotation(Vector3.Normalize(PositionToLookAt * Time.deltaTime));
                    
                    gameObject.transform.rotation = Quaternion.Slerp(CurrentRotation, PlayerRotation, Mathf.Sin(_rotationSpeed * Time.deltaTime));
               }
          }

          /// <summary>
          /// Player jump controller
          /// </summary>
          private void PlayerJump()
          {
               this.Gravity();
               
               var isPlayerGrounded = _PlayerCharacterController.isGrounded;

               if (!_isJumping && _isJumpPressed && isPlayerGrounded)
               {
                    if (_jumpCounter < 2 && _CurrentJumpResetRoutine != null)
                    {
                         StopCoroutine(_CurrentJumpResetRoutine);
                    }
                    
                    /*
                    // Trajectory jump
                    // --------------------------------
                    // Use this if we use 3 type of jump animation
                    if (_jumpCounter < 3 && _CurrentJumpResetRoutine != null)
                    {
                         StopCoroutine(_CurrentJumpResetRoutine);
                    }
                    */
                    
                    _isJumping = true;

                    PlayerAnimation.Instance.JumpAnimation(true);
                    
                    _isJumpAnimating = true;

                    _jumpCounter++;

                    PlayerAnimation.Instance.JumpOnTakeAnimation(_jumpCounter);
                    
                    Debug.Log($"{gameObject.name} is jump {_jumpCounter}"); // DEBUG
                    
                    // Jump movement
                    _CurrentMovement.y = _InitialJumpVelocity[_jumpCounter];
                    _AppliedPlayerMovement.y = _InitialJumpVelocity[_jumpCounter];
               }
               else if (!_isJumpPressed && _isJumping && isPlayerGrounded)
               {
                    _isJumping = false;

                    // print("Stop jumping: " + _isJumping); // PRINT
               }
          }

          /// <summary>
          /// Initialize and calculate the jump trajectory with physics
          /// </summary>
          private void InitializeJump()
          {
               var timeApex = _maxJumpTime / 2F;
               
               /*
               // Default concept of first jump velocity & gravity
               _jumpVelocity = (2f * _maxJumpHeight) / timeApex * 1.25f;
               _gravity = (-2f * _maxJumpHeight) / Mathf.Pow(timeApex, 2f);
               */
               
               // First jump velocity & gravity
               _jumpVelocity = Mathf.Max(2f * (_maxJumpHeight + 3f)) / Mathf.Cos(timeApex * 2f);
               _gravity = (-2f * (_maxJumpHeight + 3f)) / Mathf.Pow((timeApex * 1.5f), 2f);

               // Second jump velocity & gravity
               float secondJumpVelocity = Mathf.Max(2f * (_maxJumpHeight + 2f)) / Mathf.Cos(timeApex * 2f);
               float secondJumpGravity = (-2f * (_maxJumpHeight + 2f)) / Mathf.Pow((timeApex * 1.5f), 2f);
               
               // Third jump velocity & gravity
               // Use this if we use 3 type of jump animation
               // float thirdJumpVelocity = Mathf.Max(2f * (_maxJumpHeight + 3f)) / Mathf.Cos(timeApex * 2f);
               // float thirdJumpGravity = (-2f * (_maxJumpHeight + 3f)) / Mathf.Pow((timeApex * 1.5f), 2f);
               
               // Initial jump velocity
               _InitialJumpVelocity.Add(1, _jumpVelocity);
               _InitialJumpVelocity.Add(2, secondJumpVelocity);
               
               // Use this if we use 3 type of jump animation
               // _InitialJumpVelocity.Add(3, thirdJumpVelocity);
               
               // Initial jump gravity
               _InitialJumpGravity.Add(ZERO, _gravity);
               _InitialJumpGravity.Add(1, _gravity);
               _InitialJumpGravity.Add(2, secondJumpGravity);
               
               // Use this if we use 3 type of jump animation
               // _InitialJumpGravity.Add(3, thirdJumpGravity);
          }

          /// <summary>
          /// Gravity of player character
          /// </summary>
          private void Gravity()
          {
               Boolean isFalling = _CurrentMovement.y <= 0.0f || !_isJumpPressed;
               
               if (_PlayerCharacterController.isGrounded)
               {
                    if (_isJumpAnimating == true)
                    {
                         PlayerAnimation.Instance.JumpAnimation(false);

                         _isJumpAnimating = false;

                         _CurrentJumpResetRoutine = StartCoroutine(ResetJumpRoutine(0.5f));

                         if (_jumpCounter == 2)
                         {
                              _jumpCounter = ZERO;
                              
                              PlayerAnimation.Instance.JumpOnTakeAnimation(_jumpCounter);
                         }
                         
                         /*
                         // Trajectory jump
                         // --------------------------------
                         // Use this if we use 3 type of jump animation
                         if (_jumpCounter == 3)
                         {
                              _jumpCounter = ZERO;
                              
                              PlayerAnimation.Instance.JumpOnTakeAnimation(_jumpCounter);
                         }
                         */
                    }
                    
                    _CurrentMovement.y = _groundedGravity;
                    _AppliedPlayerMovement.y = _groundedGravity;
                     
                    // Debug.Log($"{gameObject.name} is on the ground"); // DEBUG
               }
               else if (isFalling)
               {
                    var previousYVelocity = _CurrentMovement.y;
                    
                    _CurrentMovement.y = previousYVelocity + (_InitialJumpGravity[_jumpCounter] * _gravityMultiplier * Time.deltaTime);
                         
                    // Default applied movement
                    // _AppliedMovement.y = (previousYVelocity + _CurrentMovement.y) * 0.5F;

                    // Optional next velocity of y with max value
                    _AppliedPlayerMovement.y = Mathf.Max((previousYVelocity + _CurrentMovement.y) * 0.5F, -20.0F);
               }
               else
               {
                    var previousYVelocity = _CurrentMovement.y;

                    _CurrentMovement.y = previousYVelocity + (_InitialJumpGravity[_jumpCounter] * Time.deltaTime);

                    _AppliedPlayerMovement.y = (previousYVelocity + _CurrentMovement.y) * 0.5F;
               }
          }

          /// <summary>
          /// Reset jump counter 
          /// </summary>
          /// <param name="timer"></param>
          /// <returns>WaitForSeconds</returns>
          private IEnumerator ResetJumpRoutine(float timer)
          {
               yield return new WaitForSeconds(timer);

               _jumpCounter = ZERO;
          }

          /// <summary>
          /// Respawn player when falling out of the level
          /// </summary>
          /// <exception cref="System.Exception">throw</exception>
          private void RespawnPlayer()
          {
               if (TryGetComponent<Rigidbody>(out Rigidbody PlayerRb))
               {
                    // With Rigidbody
                    Vector3 PlayerPosition = PlayerRb.transform.position;

                    // With Character Controller
                    // Vector3 PlayerPosition = _PlayerCharacterController.transform.position;

                    // Reset velocity value
                    if (_PlayerCharacterController.isGrounded)
                    {
                         PlayerPosition.y = ZERO;
                         
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