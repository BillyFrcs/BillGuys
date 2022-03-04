using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Levels
{
    public class LevelController : MonoBehaviour
    {
        [Tooltip("Set Types Of Level Element Animation Object To Play")] [SerializeField] private bool[] _animation;
        
        [Header("Animation Speed")]
        [Tooltip("Speed Of Hammer Animation")] [SerializeField] private float _hammer = 2f;
        
        // Start is called before the first frame update
        private void Start()
        {
            for (uint i = 0; i < _animation.Length; i++)
            {
                switch (_animation[i])
                {
                    case true:
                        if (_animation[0])
                        {
                            transform.DORotate(new Vector3(0f, 360f, 0f), _hammer * 0.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
                        } 
                        break;
                    
                    default:
                        throw new ArgumentException("Element Not Found!");
                }
            }
        }

        // Update is called once per frame
        private void Update()
        {
            
        }
    }
}
