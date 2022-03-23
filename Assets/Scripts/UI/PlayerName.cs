using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerName : MonoBehaviour
    {
        [Tooltip("Reference Text")] [SerializeField] private TMP_Text _CharacterName;
        
        [Tooltip("Reference Player Character")] [SerializeField] private GameObject _CharacterObject;

        private Transform _CameraOffset;
        
        // Start is called before the first frame update
        private void Start()
        {
            // Set player character name 
            var replaceCharacterNameObject = _CharacterObject.name.Replace("(Clone)", String.Empty); // Remove (Clone) text when spawn prefab

            _CharacterName.SetText(replaceCharacterNameObject);

            if (Camera.main != null)
            {
                _CameraOffset = Camera.main.gameObject.transform;
            }
        }

        private void LateUpdate()
        {
            Quaternion cameraRotation = _CameraOffset.transform.rotation;
            
            // Character name look at to the character object position
            transform.LookAt(gameObject.transform.position + cameraRotation.normalized * Vector3.forward * Time.deltaTime, cameraRotation.normalized * Vector3.up * Time.deltaTime);
        }
    }
}
