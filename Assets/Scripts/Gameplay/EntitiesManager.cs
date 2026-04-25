using System;
using System.Collections;
using Gameplay.Entities;
using Gameplay.Entities.PlayerControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay
{
    [Serializable]
    public class EntitiesManager
    {
        [SerializeField] private SimpleCharacterController _simpleCharacterController;
        [SerializeField] private Transform _characterContainer;

        [SerializeField] private Star _star;
        [SerializeField] private float _timeToRespawnStar = 5f;

        public Star Star => _star;
        public SimpleCharacterController CharacterInstance { get; private set; }

        internal void Initialize()
        {
            CharacterInstance = Object.Instantiate(_simpleCharacterController, _characterContainer);
        }

        internal IEnumerator RespawnStarCoroutine()
        {
            yield return new WaitForSeconds(_timeToRespawnStar);
            _star.gameObject.SetActive(true);
            _star.Show();
        }
    }
}