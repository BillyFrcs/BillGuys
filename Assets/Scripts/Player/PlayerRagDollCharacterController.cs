using System;
using System.Collections;
using System.Collections.Generic;
using Helpers.Interfaces;
using UnityEngine;

namespace Player
{
    public class PlayerRagDollCharacterController : MonoBehaviour, IPlayerRagDollCharacterController
    {
        // RagDoll character physics controller
        [Header("RagDoll Character's Body Parts")]
        private Rigidbody[] _CharacterBodyPartsRb;
        private Collider[] _CharacterBodyPartsCollider;

        private PlayerCharacterController _PlayerCharacterController;
        private PlayerAnimation _PlayerAnimation;

        private void Awake()
        {
            _CharacterBodyPartsRb = GetComponentsInChildren<Rigidbody>();
            _CharacterBodyPartsCollider = GetComponentsInChildren<Collider>();

            _PlayerCharacterController = FindObjectOfType<PlayerCharacterController>();
            _PlayerAnimation = FindObjectOfType(type: typeof(PlayerAnimation)) as PlayerAnimation;
        }

        // Start is called before the first frame update
        private void Start()
        {
            DeactivateRagDollCharacter();
        }

        // Update is called once per frame
        private void Update()
        {

        }
        
        /// <summary>
        /// Activate RagDoll physics simulation to the player's character
        /// </summary>
        public void ActivateRagDollCharacter()
        {
            // Player's body parts
            foreach (Rigidbody playerRb in _CharacterBodyPartsRb)
            {
                playerRb.isKinematic = false;
                playerRb.interpolation = RigidbodyInterpolation.Interpolate;
                playerRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
               
            foreach (Collider playerCollider in _CharacterBodyPartsCollider)
            {
                playerCollider.isTrigger = false;
            }

            // Player's character
            _PlayerCharacterController.PlayerCollider.isTrigger = true;
            _PlayerCharacterController.PlayerRigidbody.isKinematic = true;
            _PlayerCharacterController.PlayerRigidbody.constraints = RigidbodyConstraints.None;
            
            _PlayerAnimation.PlayerCharacterAnimator.enabled = false;
        }
        
        /// <summary>
        /// Deactivate RagDoll physics simulation to the player's character
        /// </summary>
        public void DeactivateRagDollCharacter()
        {
            // Player's body parts
            foreach (Rigidbody playerRb in _CharacterBodyPartsRb)
            {
                playerRb.isKinematic = true;
                playerRb.interpolation = RigidbodyInterpolation.None;
                playerRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
               
            foreach (Collider playerCollider in _CharacterBodyPartsCollider)
            {
                playerCollider.isTrigger = true;
            }
            
            // Player's character
            _PlayerCharacterController.PlayerCollider.isTrigger = false;
            _PlayerCharacterController.PlayerRigidbody.isKinematic = false;
            _PlayerCharacterController.PlayerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            _PlayerAnimation.PlayerCharacterAnimator.enabled = true;
        }
    }
}