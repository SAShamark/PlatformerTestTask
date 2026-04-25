using System;
using System.Collections;
using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace Gameplay.Entities.PlayerControl
{
    public class SimpleCharacterController : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private MovementControl _movementControl;

        [SerializeField] private StarsControl _headStars;
        [SerializeField] private float _dizzinessTime = 1.5f;
        [SerializeField] private int _countForDizziness = 3;

        private bool _hasHitBlockThisJump;
        private int _luckyBoxHitCount;

        public event Action OnShakeCamera;

        private void Start()
        {
            _movementControl.Init(_inputReader, _controller);
            _inputReader.onJumpPerformed += _movementControl.OnJump;
        }

        private void Update()
        {
            _movementControl.UpdateInput();
            _movementControl.UpdateAnimator();
        }

        private void FixedUpdate()
        {
            _movementControl.FixedUpdate();

            if (_controller.isGrounded)
            {
                _hasHitBlockThisJump = false;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.CompareTag("BonusBlock"))
            {
                if (hit.normal.y < -0.7f && !_hasHitBlockThisJump)
                {
                    BonusCube block = hit.gameObject.GetComponent<BonusCube>();
                    if (block != null)
                    {
                        HandleBlockHit(block);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Item"))
            {
                if (other.gameObject.TryGetComponent(out ICollectable collectable))
                {
                    collectable.Collect();
                }
            }
        }

        private void OnDestroy()
        {
            _inputReader.onJumpPerformed -= _movementControl.OnJump;
        }

        private void HandleBlockHit(BonusCube block)
        {
            _hasHitBlockThisJump = true;
            block.Hit();
            OnShakeCamera?.Invoke();
            _luckyBoxHitCount++;
            if (_luckyBoxHitCount == _countForDizziness)
            {
                StartCoroutine(DizzinessCoroutine());
            }
        }

        private IEnumerator DizzinessCoroutine()
        {
            _headStars.gameObject.SetActive(true);
            _movementControl.ChangeMovementAbility(false);
            yield return new WaitForSeconds(_dizzinessTime);

            _luckyBoxHitCount = 0;
            _movementControl.ChangeMovementAbility(true);
            _headStars.FadeOutAndDisable();
        }
    }
}