using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers.Interfaces
{
    public interface IPlayerRagDollCharacterController
    {
        public void ActivateRagDollCharacter();
        public void DeactivateRagDollCharacter();
    }
    
    public interface ILevelController
    {
        public void LevelObjectAnimation();
    }

    public interface ISoundManager
    {
        public void PlaySoundEffect(String soundName, Boolean isPlay);
    }
}
