using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSRoguelite.InGame.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [Header("ÉJÉÅÉâÇÃäÓñ{ê›íË")]

        [SerializeField] private float lookSensitivity = 0.2f;

        [SerializeField] private float minPitch = -10f;

        [SerializeField] private float maxPitch = 60f;

        [SerializeField] private float zoomSpeed = 5.0f;

        [SerializeField] private float targetDistance = 3.0f;

        [SerializeField] private float targetHeightOffset = 1.2f;

        [SerializeField] private float targetShouldereOffset = 0.8f;

        private PlayerInputActions inputActions;

        private Vector2 lookInput = Vector2.zero;

        private float currentYaw = 0f;

        private float currentPitch = 20f;

        private float currentDistance = 0f;

        private float currentHeightOffset = 0f;

        private float currentShoulderOffset = 0f;
        private void Awake()
        {
            inputActions = new PlayerInputActions();

            Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = false;
        }
        private void OnEnable()
        {
            inputActions.Enable();
        }
        private void OnDisable()
        {
            inputActions.Disable();
        }
        void Update()
        {
            lookInput = inputActions.Player.Look.ReadValue<Vector2>();

            currentYaw += lookInput.x * lookSensitivity;

            currentPitch -= lookInput.y * lookSensitivity;

            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }
        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }
            
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSpeed * Time.deltaTime);

            currentHeightOffset = Mathf.Lerp(currentHeightOffset, targetHeightOffset, zoomSpeed * Time.deltaTime);

            currentShoulderOffset = Mathf.Lerp(currentShoulderOffset, targetShouldereOffset, zoomSpeed * Time.deltaTime);

            Quaternion rotate = Quaternion.Euler(currentPitch, currentYaw, 0f);

            Vector3 basePosition = target.position + Vector3.up * currentHeightOffset;

            Vector3 shoulderPosition = basePosition + (rotate * Vector3.right * currentShoulderOffset);

            Vector3 cameraPosition = shoulderPosition + (rotate * Vector3.forward * currentDistance);

            transform.position = cameraPosition;

            transform.rotation = rotate;
        }
    }
}