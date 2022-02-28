using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        private struct ParameterAnimator
        {
            public static String movement = "Movement";
            public static String run = "Run";
            public static String jump = "Jump";
            public static String jumpOnTake = "Jump On Take";
            public static String doubleJump = "Double Jump";
            public static String dance = "Dance";
            public static String dizzy = "Dizzy";
            public static String punch = "Punch";
            public static String kick = "Kick";
            public static String victory = "Victory";
            public static String die = "Die";
        }
        
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
        /// <param name="isPlayerMove">Boolean</param>
        /// </summary>
        public void MovementAnimation(in Boolean isPlayerMove)
        {
            if (TryGetComponent(out Animator PlayerAnimator))
            {
                bool isPlayerRun = PlayerAnimator.GetBool(ParameterAnimator.movement);
                
                if (isPlayerMove == true && !isPlayerRun)
                {
                    PlayerAnimator.SetBool(ParameterAnimator.movement, true);
                    
                    // Debug.Log("Move animation"); // DEBUG
                }
                else if (!isPlayerMove && isPlayerRun)
                {
                    PlayerAnimator.SetBool(ParameterAnimator.movement, false);
                    
                    // Debug.Log("Stop move animation"); // DEBUG
                }
            }
        }

        /// <summary>
        /// Playing run animation
        /// <param name="isPlayerMove">Boolean</param>
        /// <param name="isPlayerRun">Boolean</param>
        /// </summary>
        public void RunAnimation(in Boolean isPlayerMove, in Boolean isPlayerRun)
        {
            if (TryGetComponent(out Animator PlayerAnimator))
            {
                bool isPlayerRunning = PlayerAnimator.GetBool(ParameterAnimator.run);
                
                if (isPlayerMove == true && isPlayerRun && !isPlayerRunning)
                {
                    PlayerAnimator.SetBool(ParameterAnimator.run, true);
                    
                    // Debug.Log("Run animation"); // DEBUG
                }
                else if (!isPlayerMove || !isPlayerRun && isPlayerRunning)
                {
                    PlayerAnimator.SetBool(ParameterAnimator.run, false);
                    
                    // Debug.Log("Stop run animation"); // DEBUG
                }
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
                var jumpAnimation = Animator.StringToHash(ParameterAnimator.jump);

                if (isPlayerJump != false)
                {
                    PlayerAnimator.SetBool(jumpAnimation, true);
                    
                    // Debug.Log($"Player {gameObject.name} is jump"); // DEBUG
                }
                else
                {
                    PlayerAnimator.SetBool(jumpAnimation, false);

                    // Debug.Log($"Player {gameObject.name} is not jump"); // DEBUG
                }
            }
        }
        
        /// <summary>
        /// Playing slide animation
        /// </summary>
        /// <param name="jumpCounter">Int32</param>
        public void JumpOnTakeAnimation(Int32 jumpCounter)
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                var jumpOnTakeAnimation = Animator.StringToHash(ParameterAnimator.jumpOnTake);

                PlayerAnimator.SetInteger(jumpOnTakeAnimation, jumpCounter);
            }
        }

        /// <summary>
        /// Playing double jump animation
        /// </summary>
        /// <param name="isPlayerDoubleJump">Boolean</param>
        public void DoubleJumpAnimation(Boolean isPlayerDoubleJump)
        {
            if (TryGetComponent<Animator>(out var PlayerAnimator))
            {
                var doubleJumpAnimation = Animator.StringToHash(ParameterAnimator.doubleJump);

                if (isPlayerDoubleJump == !false)
                {
                    PlayerAnimator.SetBool(doubleJumpAnimation, true);
                }
                else
                {
                    PlayerAnimator.SetBool(doubleJumpAnimation, false);
                }
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
                var danceAnimation = Animator.StringToHash(ParameterAnimator.dance);

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
                var dizzyAnimation = Animator.StringToHash(ParameterAnimator.dizzy);

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
                var punchAnimation = Animator.StringToHash(ParameterAnimator.punch);

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
                var kickAnimation = Animator.StringToHash(ParameterAnimator.kick);

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
                var victoryAnimation = Animator.StringToHash(ParameterAnimator.victory);

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
                var dieAnimation = Animator.StringToHash(ParameterAnimator.die);
                
                PlayerAnimator.SetTrigger(dieAnimation);
                
                // Debug.Log("Die animation"); // DEBUG
            }
        }
    }
}
