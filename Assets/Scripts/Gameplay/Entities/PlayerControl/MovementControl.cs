using System;
using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace Gameplay.Entities.PlayerControl
{
    [Serializable]
    public class MovementControl
    {
        private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
        private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
        private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");
        private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
        private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");
        private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
        private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
        private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
        private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");

        [Header("Components")] [SerializeField]
        private Animator _animator;

        [SerializeField] private Transform _modelTransform;

        [Header("Movement Settings")] [SerializeField]
        private float _moveSpeed = 5f;

        [SerializeField] private float _rotationSpeed = 10f;

        [Header("Inertia Settings")] [SerializeField]
        private float _acceleration = 50;

        [SerializeField] private float _deceleration = 20f;
        [SerializeField] private float _animatorDamping = 10f;

        [Header("Jump Settings")] [SerializeField]
        private float _jumpForce = 10f;

        [SerializeField] private float _gravityMultiplier = 2f;

        [Header("Jump Feel")] [SerializeField] private float _coyoteTime = 0.15f;
        [SerializeField] private float _jumpBufferTime = 0.10f;
        [SerializeField] private float _fallGravityMult = 2.5f;
        [SerializeField] private float _earlyReleaseGravityMult = 3f;
        [SerializeField] private float _minJumpVelocity = 3f;
        [SerializeField] private float _apexGravityMult = 0.35f;
        [SerializeField] private float _apexThreshold = 2f;
        [SerializeField] private float _terminalVelocity = -30f;

        [Header("Ground Check")] [SerializeField]
        private LayerMask _groundLayerMask;

        [SerializeField] private float _groundedOffset = -0.14f;

        private InputReader _inputReader;
        private CharacterController _controller;

        private float _verticalVelocity;

        private bool _isGrounded;
        private bool _wasGrounded;

        private bool _isWalking;
        private bool _isStopped;
        private bool _movementInputHeld;

        private int _currentGait;
        private float _currentSpeed;
        private float _animatorSpeed;
        private float _moveDirectionX;

        private enum AirState
        {
            OnGround,
            Coyote,
            InAir
        }

        private AirState _airState = AirState.OnGround;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _jumpConsumed;
        private bool _jumpButtonHeld;
        public bool IsActive { get; private set; } = true;

        public void Init(InputReader inputReader, CharacterController controller)
        {
            _inputReader = inputReader;
            _controller = controller;
        }

        public void UpdateInput()
        {
            if (!IsActive)
            {
                _moveDirectionX = 0f;
                _movementInputHeld = false;
                _jumpButtonHeld = false;
                return;
            }

            _moveDirectionX = _inputReader._moveComposite.x;
            _movementInputHeld = Mathf.Abs(_moveDirectionX) > 0.01f;
            _jumpButtonHeld = _inputReader.JumpHeld;

            if (_inputReader.JumpJustPressed)
                _jumpBufferTimer = _jumpBufferTime;
        }

        public void FixedUpdate()
        {
            GroundedCheck();
            UpdateAirState();
            ProcessJumpBuffer();
            ApplyInertia();
            FaceMoveDirection();
            ApplyGravity();
            Move();
            TrackLanding();
        }

        public void UpdateAnimator()
        {
            float targetAnimSpeed = Mathf.Abs(_currentSpeed);
            _animatorSpeed = Mathf.Lerp(_animatorSpeed, targetAnimSpeed, _animatorDamping * Time.deltaTime);

            _animator.SetFloat(_moveSpeedHash, _animatorSpeed);
            _animator.SetInteger(_currentGaitHash, _currentGait);
            _animator.SetBool(_isGroundedHash, _isGrounded);
            _animator.SetFloat(_strafeDirectionXHash, 0f);
            _animator.SetFloat(_strafeDirectionZHash, 1f);
            _animator.SetFloat(_isStrafingHash, 0f);
            _animator.SetBool(_isWalkingHash, _isWalking);
            _animator.SetBool(_isStoppedHash, _isStopped);
            _animator.SetBool(_movementInputHeldHash, _movementInputHeld);
        }

        public void OnJump()
        {
            if (IsActive)
            {
                _jumpBufferTimer = _jumpBufferTime;
            }
        }

        public void ChangeMovementAbility(bool isActive)
        {
            IsActive = isActive;
        }

        private void UpdateAirState()
        {
            bool justLeftGround = _wasGrounded && !_isGrounded;

            if (_isGrounded)
            {
                _airState = AirState.OnGround;
                _coyoteTimer = _coyoteTime;
                _jumpConsumed = false;
            }
            else if (justLeftGround && !_jumpConsumed)
            {
                _airState = AirState.Coyote;
            }

            if (_airState == AirState.Coyote)
            {
                _coyoteTimer -= Time.fixedDeltaTime;
                if (_coyoteTimer <= 0f)
                    _airState = AirState.InAir;
            }
        }

        private void ProcessJumpBuffer()
        {
            if (!IsActive)
            {
                _jumpBufferTimer = 0f;
                return;
            }

            _jumpBufferTimer -= Time.fixedDeltaTime;

            bool canJump = _airState is AirState.OnGround or AirState.Coyote && !_jumpConsumed;

            if (_jumpBufferTimer > 0f && canJump)
            {
                ExecuteJump();
                _jumpBufferTimer = 0f;
            }
        }

        private void ExecuteJump()
        {
            _verticalVelocity = _jumpForce;
            _jumpConsumed = true;
            _airState = AirState.InAir;
            _coyoteTimer = 0f;
            _animator.SetBool(_isJumpingAnimHash, true);
        }

        private void ApplyGravity()
        {
            if (_isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
                _animator.SetBool(_isJumpingAnimHash, false);
                return;
            }

            float gravMult;

            bool isRising = _verticalVelocity > 0f;
            bool isApex = Mathf.Abs(_verticalVelocity) < _apexThreshold && _jumpButtonHeld;
            bool isFalling = _verticalVelocity < -_apexThreshold;

            if (isFalling)
            {
                gravMult = _fallGravityMult;
            }
            else if (isApex)
            {
                gravMult = _apexGravityMult;
            }
            else if (isRising && !_jumpButtonHeld)
            {
                gravMult = _earlyReleaseGravityMult;
            }
            else
            {
                gravMult = _gravityMultiplier;
            }

            _verticalVelocity += Physics.gravity.y * gravMult * Time.fixedDeltaTime;
            _verticalVelocity = Mathf.Max(_verticalVelocity, _terminalVelocity);

            if (_verticalVelocity <= 0f)
                _animator.SetBool(_isJumpingAnimHash, false);
        }

        private void TrackLanding()
        {
            _wasGrounded = _isGrounded;
        }

        private void ApplyInertia()
        {
            float targetSpeed = _movementInputHeld ? Mathf.Sign(_moveDirectionX) * _moveSpeed : 0f;
            float rate = _movementInputHeld ? _acceleration : _deceleration;

            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);

            float absSpeed = Mathf.Abs(_currentSpeed);

            _currentGait = absSpeed < 0.01f ? 0 : absSpeed < _moveSpeed * 0.5f ? 1 : 2;

            _isStopped = !_movementInputHeld && absSpeed < 0.1f;
            _isWalking = !_isStopped && _isGrounded;
        }

        private void FaceMoveDirection()
        {
            if (_modelTransform == null) return;
            if (!_movementInputHeld || Mathf.Abs(_currentSpeed) <= 0.1f) return;

            Vector3 faceDirection = new Vector3(_currentSpeed, 0f, 0f);
            Quaternion targetRot = Quaternion.LookRotation(faceDirection);
            _modelTransform.rotation = Quaternion.Slerp(
                _modelTransform.rotation,
                targetRot,
                _rotationSpeed * Time.fixedDeltaTime
            );
        }

        private void Move()
        {
            Vector3 motion = new Vector3(_currentSpeed, _verticalVelocity, 0f) * Time.fixedDeltaTime;
            CollisionFlags flags = _controller.Move(motion);

            if ((flags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
                _verticalVelocity = 0f;
        }

        private void GroundedCheck()
        {
            Vector3 spherePos = new Vector3(
                _controller.transform.position.x,
                _controller.transform.position.y - _groundedOffset,
                _controller.transform.position.z
            );

            _isGrounded = Physics.CheckSphere(spherePos, _controller.radius, _groundLayerMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}