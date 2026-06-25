using UnityEngine;
using UnityEngine.InputSystem;

namespace Tusk.CameraRig
{
    /// <summary>
    /// Third-person follow camera. Orbits around the player based on mouse delta.
    /// Smoothly trails position. No Cinemachine dependency at runtime so this
    /// works even if the Cinemachine package import fails or version mismatches.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

        [Header("Orbit")]
        [SerializeField] private float distance = 9f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float yawSensitivity = 0.18f;
        [SerializeField] private float pitchSensitivity = 0.14f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.06f;

        private float _yaw = 0f;
        private float _pitch = 18f;
        private Vector3 _velocity;
        private InputAction _lookAction;

        private void Awake()
        {
            _lookAction = new InputAction("Look", binding: "<Mouse>/delta");
            _lookAction.AddBinding("<Gamepad>/rightStick").WithProcessor("scale(factor=10)");
            _lookAction.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDestroy() => _lookAction?.Disable();

        public Transform Target { set => target = value; }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector2 look = _lookAction.ReadValue<Vector2>();
            _yaw   += look.x * yawSensitivity;
            _pitch -= look.y * pitchSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 anchor = target.position + targetOffset;
            Vector3 desired = anchor - rot * Vector3.forward * distance;

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, positionSmoothTime);
            transform.rotation = rot;
        }
    }
}
