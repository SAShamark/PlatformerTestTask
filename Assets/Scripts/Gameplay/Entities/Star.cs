using UnityEngine;
using DG.Tweening;
using System;

namespace Gameplay.Entities
{
    public class Star : MonoBehaviour, ICollectable
    {
        [Header("Idle Settings")]
        [SerializeField] private float _bobDistance = 0.3f;
        [SerializeField] private float _bobDuration = 1.2f;
        [SerializeField] private float _rotateSpeed = 45f;

        [Header("Appearance")]
        [SerializeField] private float _showDuration = 0.6f;
        [SerializeField] private Ease _showEase = Ease.OutElastic;

        [Header("Collection")]
        [SerializeField] private float _pickupJumpPower = 1.5f;
        [SerializeField] private float _pickupDuration = 0.5f;

        public event Action OnStarCollected;

        private bool _isCollected;
        private Vector3 _baseScale;
        private Vector3 _initialLocalPos;
        private Tween _rotateTween;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _initialLocalPos = transform.localPosition;
        }

        private void Start()
        {
            if (gameObject.activeSelf && !_isCollected)
                StartIdleAnimation();
        }

        public void Show()
        {
            _isCollected = false;
            gameObject.SetActive(true);
            
            transform.DOKill();
            transform.localScale = Vector3.zero;
            
            transform.DOScale(_baseScale, _showDuration).SetEase(_showEase);
            
            _rotateTween = transform.DOLocalRotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);

            StartIdleAnimation();
        }

        private void StartIdleAnimation()
        {
            transform.DOLocalMoveY(_initialLocalPos.y + _bobDistance, _bobDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void Collect()
        {
            if (_isCollected) return; 
            _isCollected = true;

            transform.DOKill();
            _rotateTween?.Kill();

            Sequence pickupSeq = DOTween.Sequence();
            pickupSeq.Append(transform.DOLocalJump(transform.localPosition, _pickupJumpPower, 1, _pickupDuration));
            pickupSeq.Join(transform.DOLocalRotate(new Vector3(0, 720, 0), _pickupDuration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
            pickupSeq.Join(transform.DOScale(0f, _pickupDuration).SetEase(Ease.InBack));
            pickupSeq.OnComplete(() =>
            {
                OnStarCollected?.Invoke();
                gameObject.SetActive(false);
            });
        }
    }
}