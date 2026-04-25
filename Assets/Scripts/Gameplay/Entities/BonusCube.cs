using DamageNumbersPro;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.Entities
{
    public class BonusCube : MonoBehaviour
    {
        [Header("Damage Number")]
        [SerializeField] private DamageNumber _numberPrefab;
        [SerializeField] private float _spawnOffset = 1.1f;
        [SerializeField] private int _rewardValue = 1;

        [Header("Visual")]
        [SerializeField] private Transform _visual;

        [Header("Bounce Settings")]
        [SerializeField] private float _downOffset = -0.075f;
        [SerializeField] private float _upOffset = 0.33f;

        [SerializeField] private float _downDuration = 0.04f;
        [SerializeField] private float _upDuration = 0.08f;
        [SerializeField] private float _returnDuration = 0.12f;

        [Header("Scale Settings")]
        [SerializeField] private Vector3 _squashScale = new(1.2f, 0.85f, 1.2f);
        [SerializeField] private Vector3 _stretchScale = new(0.85f, 1.2f, 0.85f);
        [SerializeField] private float _scaleReturnDuration = 0.1f;

        public void Hit()
        {
            if (_numberPrefab != null)
            {
                _numberPrefab.Spawn(
                    transform.position + Vector3.up * _spawnOffset,
                    $"+{_rewardValue}"
                );
            }

            BounceEffect();
        }

        private void BounceEffect()
        {
            _visual.DOKill(true);

            _visual.localPosition = Vector3.zero;
            _visual.localScale = Vector3.one;

            Sequence seq = DOTween.Sequence();

            seq.Append(_visual.DOLocalMoveY(_downOffset, _downDuration).SetEase(Ease.OutQuad));
            seq.Join(_visual.DOScale(_squashScale, _downDuration).SetEase(Ease.OutQuad));

            seq.Append(_visual.DOLocalMoveY(_upOffset, _upDuration).SetEase(Ease.OutCubic));
            seq.Join(_visual.DOScale(_stretchScale, _upDuration).SetEase(Ease.OutCubic));

            seq.Append(_visual.DOLocalMoveY(0f, _returnDuration).SetEase(Ease.OutBounce));
            seq.Join(_visual.DOScale(Vector3.one, _scaleReturnDuration).SetEase(Ease.OutBack));
        }
    }
}