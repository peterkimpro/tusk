using UnityEngine;
using UnityEngine.InputSystem;

namespace Tusk.Player
{
    /// <summary>
    /// Third-person CharacterController-based movement. WASD movement,
    /// mouse-driven look, sprint (Shift), jump (Space). Stamina drains
    /// on sprint, regenerates when not sprinting.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 9f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -19.6f;
        [SerializeField] private float turnSpeed = 12f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float sprintDrainPerSec = 18f;
        [SerializeField] private float staminaRegenPerSec = 14f;
        [SerializeField] private float minStaminaToSprint = 10f;

        [Header("Refs")]
        [SerializeField] private Transform cameraRig;

        public float Stamina { get; private set; }
        public float MaxStamina => maxStamina;
        public bool IsSprinting { get; private set; }
        public bool IsGrounded => _controller.isGrounded;
        public Vector3 Velocity => _velocity;

        private CharacterController _controller;
        private InputAction _moveAction;
        private InputAction _sprintAction;
        private InputAction _jumpAction;
        private Vector3 _velocity;
        private Vector2 _moveInput;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            Stamina = maxStamina;
            BindInput();
        }

        private void BindInput()
        {
            _moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.Enable();

            _sprintAction = new InputAction("Sprint", binding: "<Keyboard>/leftShift");
            _sprintAction.AddBinding("<Gamepad>/leftStickPress");
            _sprintAction.Enable();

            _jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
            _jumpAction.AddBinding("<Gamepad>/buttonSouth");
            _jumpAction.performed += OnJump;
            _jumpAction.Enable();
        }

        private void OnDestroy()
        {
            _moveAction?.Disable();
            _sprintAction?.Disable();
            _jumpAction?.Disable();
            if (_jumpAction != null) _jumpAction.performed -= OnJump;
        }

        private void Update()
        {
            _moveInput = _moveAction.ReadValue<Vector2>();
            UpdateSprintState();
            UpdateMovement();
            UpdateGravity();
            UpdateStaminaRegen();
        }

        private void UpdateSprintState()
        {
            bool wantsSprint = _sprintAction.IsPressed() && _moveInput.sqrMagnitude > 0.1f;
            IsSprinting = wantsSprint && Stamina > 0f;
            if (IsSprinting)
            {
                Stamina = Mathf.Max(0f, Stamina - sprintDrainPerSec * Time.deltaTime);
                if (Stamina <= 0f) IsSprinting = false;
            }
        }

        private void UpdateStaminaRegen()
        {
            if (!IsSprinting && Stamina < maxStamina)
                Stamina = Mathf.Min(maxStamina, Stamina + staminaRegenPerSec * Time.deltaTime);
        }

        private void UpdateMovement()
        {
            if (_moveInput.sqrMagnitude < 0.01f) return;

            // Movement is relative to camera-rig facing (Y-only)
            Vector3 camForward = cameraRig != null ? cameraRig.forward : transform.forward;
            Vector3 camRight   = cameraRig != null ? cameraRig.right   : transform.right;
            camForward.y = 0f; camRight.y = 0f;
            camForward.Normalize(); camRight.Normalize();

            Vector3 dir = camForward * _moveInput.y + camRight * _moveInput.x;
            float speed = IsSprinting ? sprintSpeed : walkSpeed;
            _controller.Move(dir * speed * Time.deltaTime);

            // Rotate character to face movement direction
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
            }
        }

        private void OnJump(InputAction.CallbackContext _)
        {
            if (!IsGrounded) return;
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        private void UpdateGravity()
        {
            if (IsGrounded && _velocity.y < 0f) _velocity.y = -2f;
            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}
