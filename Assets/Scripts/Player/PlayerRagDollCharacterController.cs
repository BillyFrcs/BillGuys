using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerRagDollCharacterController : MonoBehaviour
    {
        // RagDoll character physics controller
        [Header("RagDoll Character")]
        [SerializeField] private Rigidbody[] _CharacterBodyPartsRb;
        [SerializeField] private Collider[] _CharacterBodyPartsCollider;

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
            foreach (Rigidbody PlayerRb in _CharacterBodyPartsRb)
            {
                PlayerRb.isKinematic = false;
                PlayerRb.interpolation = RigidbodyInterpolation.Interpolate;
                PlayerRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
               
            foreach (Collider PlayerCollider in _CharacterBodyPartsCollider)
            {
                PlayerCollider.isTrigger = false;
            }

            // Player's character
            _PlayerCharacterController.PlayerCollider.isTrigger = true;
            _PlayerCharacterController.PlayerRigidbody.isKinematic = true;
            _PlayerCharacterController.PlayerRigidbody.constraints = RigidbodyConstraints.None;
            
            _PlayerAnimation.PlayerCharacterAnimator.enabled = false;
            _PlayerAnimation.PlayerCharacterAnimator.avatar = null;
        }
        
        /// <summary>
        /// Deactivate RagDoll physics simulation to the player's character
        /// </summary>
        private void DeactivateRagDollCharacter()
        {
            // Player's body parts
            foreach (Rigidbody PlayerRb in _CharacterBodyPartsRb)
            {
                PlayerRb.isKinematic = true;
                PlayerRb.interpolation = RigidbodyInterpolation.None;
                PlayerRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
               
            foreach (Collider PlayerCollider in _CharacterBodyPartsCollider)
            {
                PlayerCollider.isTrigger = true;
            }
            
            // Player's character
            _PlayerCharacterController.PlayerCollider.isTrigger = false;
            _PlayerCharacterController.PlayerRigidbody.isKinematic = false;
            _PlayerCharacterController.PlayerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            _PlayerAnimation.PlayerCharacterAnimator.enabled = true;
        }
    }
}
