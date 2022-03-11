using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerCharacterPhysics : MonoBehaviour
    {
        [Tooltip("Force Magnitude To Push Obstacle Direction")] [SerializeField] private float _forceMagnitude;
        
        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody PlayerRb = hit.collider.attachedRigidbody;

            if (PlayerRb != null)
            {
                Vector3 ForceDirection = hit.transform.position - gameObject.transform.position;

                ForceDirection.y = 0.0f;
                
                ForceDirection.Normalize();
                
                PlayerRb.AddForceAtPosition(ForceDirection * _forceMagnitude, transform.position, ForceMode.Impulse);
                
                // Debug.Log($"Push {gameObject.name}"); // DEBUG
            }
        }
    }
}
