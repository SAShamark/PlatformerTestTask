using DG.Tweening;
using UnityEngine;

namespace Gameplay.Entities.PlayerControl
{
    public class SimpleCharacterCameraFollow : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Vector3 _offset = new(0f, 5f, -10f);
        [SerializeField] private float _smoothSpeed = 5f;

        [Header("Shake Settings")] 
        [SerializeField] private Transform _cameraVisual;
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private float _shakeStrength = 0.5f;
        [SerializeField] private int _shakeVibrato = 10;

        private Transform _target;


        public void Init(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            FollowTarget();
        }

        private void FollowTarget()
        {
            Vector3 targetPosition = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
            transform.LookAt(_target);
        }

        public void Shake()
        {
            transform.DOComplete();
            transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato);
        }
    }
}