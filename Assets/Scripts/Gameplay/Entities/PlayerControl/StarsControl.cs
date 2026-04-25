using DG.Tweening;
using UnityEngine;

namespace Gameplay.Entities.PlayerControl
{
    public class StarsControl : MonoBehaviour
    {
        [SerializeField] private Transform[] _stars;
        [SerializeField] private float _moveDuration = 2.0f;
        [SerializeField] private float _endScale = 1.0f;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Ease _moveEase = Ease.Linear;
        [SerializeField] private float _appearanceDelay = 0.15f;

        private Vector3[] _initialPositions;

        private void Awake()
        {
            _initialPositions = new Vector3[_stars.Length];
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    _initialPositions[i] = _stars[i].localPosition;
                }
            }
        }

        private void OnEnable()
        {
            StartOrbit();
        }

        private void OnDisable()
        {
            KillAnimation();
        }

        private void StartOrbit()
        {
            KillAnimation();

            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] == null) continue;

                Transform star = _stars[i];
            
                star.localPosition = _initialPositions[i];
                star.localScale = Vector3.zero;

                star.DOScale(_endScale, _fadeDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * _appearanceDelay);

                Vector3[] myPath = new Vector3[_stars.Length];
                for (int k = 0; k < _stars.Length; k++)
                {
                    myPath[k] = _initialPositions[(i + k + 1) % _stars.Length];
                }

                star.DOLocalPath(myPath, _moveDuration, PathType.CatmullRom)
                    .SetEase(_moveEase)
                    .SetOptions(true)
                    .SetLoops(-1);
            }
        }

        public void FadeOutAndDisable()
        {
            Sequence fadeOutSequence = DOTween.Sequence();

            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] == null) continue;
                fadeOutSequence.Join(_stars[i].DOScale(0f, _fadeDuration).SetEase(Ease.InBack));
            }

            fadeOutSequence.OnComplete(() => {
                if (this != null) gameObject.SetActive(false);
            });
        }

        private void KillAnimation()
        {
            foreach (var star in _stars)
            {
                if (star != null) star.DOKill();
            }
        }
    }
}