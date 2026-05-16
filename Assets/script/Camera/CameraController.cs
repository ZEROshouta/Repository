using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSRoguelite.InGame.Camera
{
    public class CameraController : MonoBehaviour
    {
        private float LOOK_SENSITIVITY = 0.2f;

        private float DISTANCE = 5.0f;

        private float HEIGIT_OFFSET = 1.5f;

        private float MIN_PITCH = -10f;

        private float MAX_PITCH = 60f;

        [SerializeField] private Transform target;

        private PlayerInputActions inputActions;

        private Vector2 lookInput = Vector2.zero;

        private float currentYaw = 0f;

        private float currentPitch = 20f;
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
        // Update is called once per frame
        void Update()
        {
            lookInput = inputActions.Player.Look.ReadValue<Vector2>();

            currentYaw += lookInput.x * LOOK_SENSITIVITY;
            currentPitch -= lookInput.y * LOOK_SENSITIVITY;

            currentPitch = Mathf.Clamp(currentPitch, MIN_PITCH, MAX_PITCH);
        }
        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 targetPosition = target.position + Vector3.up * HEIGIT_OFFSET;

            Quaternion rotate = Quaternion.Euler(currentPitch, currentYaw, 0f);

            Vector3 cameraPosition = targetPosition - (rotate * Vector3.forward * DISTANCE);

            transform.position = cameraPosition;
            transform.rotation = rotate;
        }
    }
}
