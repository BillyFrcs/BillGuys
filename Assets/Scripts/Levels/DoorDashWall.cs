using Helpers.Tags;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Levels
{
    public class DoorDashWall : MonoBehaviour
    {
        [SerializeField] private Rigidbody[] _DoorDashWallRb;

        private PlayerCharacterController _PlayerCharacterController;

        private void Awake()
        {
            _DoorDashWallRb = GetComponentsInChildren<Rigidbody>();

            _PlayerCharacterController = FindObjectOfType<PlayerCharacterController>();
        }

        // Start is called before the first frame update
        private void Start()
        {
            SetUpDoorDashWall();
        }

        // Update is called once per frame
        private void Update()
        {

        }

        /// <summary>
        /// Destroy door dash wall components
        /// </summary>
        private void DestroyDoorDashWall()
        {
            foreach (var doorDashWallRb in _DoorDashWallRb)
            {
                doorDashWallRb.isKinematic = false;
            }
        }

        /// <summary>
        /// SetUp door dash wall components
        /// </summary>
        private void SetUpDoorDashWall()
        {
            foreach (var doorDashWallRb in _DoorDashWallRb)
            {
                doorDashWallRb.isKinematic = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Force only with tags
            if (other.gameObject.CompareTag(TagManager.Player))
            {
                DestroyDoorDashWall();

                Debug.Log($"Force: {gameObject.name}"); // DEBUG
            }
        }
    }
}