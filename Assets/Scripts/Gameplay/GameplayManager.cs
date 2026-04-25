using Gameplay.Entities.PlayerControl;
using UnityEngine;

namespace Gameplay
{
    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private SimpleCharacterCameraFollow _characterCameraFollow;
        [SerializeField] private EntitiesManager _entitiesManager;
        
        private Coroutine _starCoroutine;


        private void Start()
        {
            _entitiesManager.Initialize();
            _characterCameraFollow.Init(_entitiesManager.CharacterInstance.transform);

            Subscribes();
        }

        private void OnDestroy()
        {
            if (_starCoroutine != null)
            {
                StopCoroutine(_starCoroutine);
            }

            Unsubscribes();
        }

        private void Subscribes()
        {
            _entitiesManager.CharacterInstance.OnShakeCamera += _characterCameraFollow.Shake;
            _entitiesManager.Star.OnStarCollected += RespawnStar;
        }

        private void Unsubscribes()
        {
            _entitiesManager.CharacterInstance.OnShakeCamera -= _characterCameraFollow.Shake;
            _entitiesManager.Star.OnStarCollected -= RespawnStar;
        }

        private void RespawnStar()
        {
            _starCoroutine = StartCoroutine(_entitiesManager.RespawnStarCoroutine());
        }
    }
}