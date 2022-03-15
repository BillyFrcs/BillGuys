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
            public static String Movement = "Movement";
            public static String Run = "Run";
            public static String Jump = "Jump";
            public static String JumpOnTake = "Jump On Take";
            public static String Dance = "Dance";
            public static String Dizzy = "Dizzy";
            public static String Punch = "Punch";
            public static String Kick = "Kick";
            public static String Victory = "Victory";
            public static String Die = "Die";
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
                bool isPlayerRun = PlayerAnimator.GetBool(ParameterAnimator.Movement);
                
                if (isPlayerMove == true && !isPlayerRun)
                {
                    PlayerAnimator.SetBool(ParameterAnimator.Movement, true);
                    
                    // Debug.Log("Move animation"); // DEBUG
                }
                else if (!isPlayerMove && isPlayerRun)
                {
                    PlayerAnimator.SetBool(ParameterAnimator.Movement, false);
                    
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
                bool isPlayerRunning = PlayerAnimator.GetBool(ParameterAnimator.Run);
                
                if (isPlayerMove == true && isPlayerRun && !isPlayerRunning)
                {
                    PlayerAnimator.SetBool(ParameterAnimator.Run, true);
                    
                    // Debug.Log("Run animation"); // DEBUG
                }
                else if (!isPlayerMove || !isPlayerRun && isPlayerRunning)
                {
                    PlayerAnimator.SetBool(ParameterAnimator.Run, false);
                    
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
                var jumpAnimation = Animator.StringToHash(ParameterAnimator.Jump);

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
                var jumpOnTakeAnimation = Animator.StringToHash(ParameterAnimator.JumpOnTake);

                PlayerAnimator.SetInteger(jumpOnTakeAnimation, jumpCounter);
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
