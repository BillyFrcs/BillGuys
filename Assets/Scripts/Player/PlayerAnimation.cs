using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        private struct ParameterAnimator
        {
            public static String Movement = "Movement";
            public static String Velocity = "Velocity";
            public static String Run = "Run";
            public static String Jump = "Jump";
            public static String Jumping = "Jumping";
            public static String Slide = "Slide";
            public static String Dance = "Dance";
            public static String Dizzy = "Dizzy";
            public static String Punch = "Punch";
            public static String Kick = "Kick";
            public static String Victory = "Victory";
            public static String Die = "Die";
        }

        // Blend animator controller
        private float _movementVelocity = 0.0F;
        [Tooltip("Acceleration Of Blend Movement Animation")] [SerializeField] private float _acceleration = 1F;
        [Tooltip("Deceleration Of Blend Movement Animation")] [SerializeField] private float _deceleration = 1F;

        private float _jumpVelocity = 0.0F;
        private float _slideVelocity = 0.0F;

        private const float Damping = 0.05f;
        
        public static PlayerAnimation Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        /// <summary>
        /// Playing movement animation
        /// <param name="isPlayerMovement">Boolean (Check if the player is move)</param>
        /// </summary>
        public void MovementAnimation(in Boolean isPlayerMovement)
        {
            if (TryGetComponent(out Animator PlayerAnimator))
            {
                var movementAnimation = Animator.StringToHash(ParameterAnimator.Velocity);

                switch (isPlayerMovement)
                {
                    case true when _movementVelocity < 1.0f:
                        this._movementVelocity += Time.deltaTime * _acceleration;
                    
                        // Debug.Log("Accelerate velocity"); // DEBUG
                        break;
                    
                    case false when _movementVelocity > 0.0f:
                        this._movementVelocity -= Time.deltaTime * _deceleration;
                    
                        // Debug.Log("Decelerate velocity"); // DEBUG
                        break;
                }

                // Reset the movement velocity value
                if (!isPlayerMovement && _movementVelocity < 0.0f)
                {
                    this._movementVelocity = 0.0F;
                    
                    // Debug.Log("Reset velocity"); // DEBUG
                }

                PlayerAnimator.SetFloat(movementAnimation, _movementVelocity, Damping, Time.deltaTime);
            }
        }

        /// <summary>
        /// Playing jump animation
        /// </summary>
        /// <param name="isPlayerJump">Boolean</param>
        public void JumpAnimation(in Boolean isPlayerJump)
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                var jumpAnimation = Animator.StringToHash(ParameterAnimator.Jumping);

                switch (isPlayerJump)
                {
                    case true when _jumpVelocity < 1.0f:
                        _jumpVelocity = 0.1f;
                        break;
                    
                    case false when _jumpVelocity > 0.0f:
                        _jumpVelocity -= 0.1f;
                        break;
                }

                // Reset the jump velocity value
                if (!isPlayerJump && _jumpVelocity < 0.0f)
                {
                    _jumpVelocity = 0.0f;
                }
                
                PlayerAnimator.SetFloat(jumpAnimation, _jumpVelocity);
            }
        }
        
        /// <summary>
        /// Playing slide animation
        /// </summary>
        /// <param name="isPlayerSlide">Boolean</param>
        public void SlideAnimation(Boolean isPlayerSlide)
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                // var slideAnimation = Animator.StringToHash(ParameterAnimator.Slide);
                
                var jumpAnimation = Animator.StringToHash(ParameterAnimator.Jumping);

                switch (isPlayerSlide)
                {
                    case true when _slideVelocity < 1.0f:
                        _slideVelocity = 1.0f;
                        break;
                    
                    case false when _slideVelocity > 0.0f:
                        _slideVelocity -= 1.0f;
                        break;
                }

                // Reset the jump velocity value
                if (!isPlayerSlide && _slideVelocity < 0.0f)
                {
                    _slideVelocity = 0.0f;
                }
                
                PlayerAnimator.SetFloat(jumpAnimation, _slideVelocity);
            }
        }

        /// <summary>
        /// Playing dance animation
        /// </summary>
        /// <param name="isPlayerDance">Boolean</param>
        public void DanceAnimation(in Boolean isPlayerDance)
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                var danceAnimation = Animator.StringToHash(ParameterAnimator.Dance);

                if (isPlayerDance != false)
                {
                    PlayerAnimator.SetBool(danceAnimation, true);
                }
                else
                {
                    PlayerAnimator.SetBool(danceAnimation, false);
                }
            }
        }
        
        /// <summary>
        /// Playing dizzy animation
        /// </summary>
        public void DizzyAnimation()
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                var dizzyAnimation = Animator.StringToHash(ParameterAnimator.Dizzy);

                PlayerAnimator.SetTrigger(dizzyAnimation);
            }
        }

        /// <summary>
        /// Playing punch animation
        /// </summary>
        public void PunchAnimation()
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                var punchAnimation = Animator.StringToHash(ParameterAnimator.Punch);

                PlayerAnimator.SetTrigger(punchAnimation);
            }
        }
        
        /// <summary>
        /// Playing kick animation
        /// </summary>
        public void KickAnimation()
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                var kickAnimation = Animator.StringToHash(ParameterAnimator.Kick);

                PlayerAnimator.SetTrigger(kickAnimation);
            }
        }

        /// <summary>
        /// Playing victory animation
        /// </summary>
        public void VictoryAnimation()
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                var victoryAnimation = Animator.StringToHash(ParameterAnimator.Victory);

                PlayerAnimator.SetTrigger(victoryAnimation);
            }
        }

        /// <summary>
        /// Playing die animation
        /// </summary>
        public void DieAnimation()
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                var dieAnimation = Animator.StringToHash(ParameterAnimator.Die);
                
                PlayerAnimator.SetTrigger(dieAnimation);
                
                // Debug.Log("Die animation"); // DEBUG
            }
        }
        
        /// <summary>
        /// Player character animator component
        /// </summary>
        public Animator PlayerCharacterAnimator => TryGetComponent(out Animator PlayerAnimator) ? PlayerAnimator : null;
    }
}
