using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSRoguelite.InGame.Camera
{
    public class PlayerController : MonoBehaviour
    {
        private const float MOVE_SPEED = 5.0f;

        [SerializeField] private Rigidbody rigidbody;

        private PlayerInputActions inputActions;

        private Vector2 moveInput = Vector2.zero;

        private Vector3 moveDirection = Vector3.zero;

        public Vector3 CurrentVelocity { get; private set; }
        private void Awake()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire;
        }
        void Update()
        {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        }
        private void FixedUpdate()
        {
            Move();
        }
        private void Move()
        {
            if (rigidbody == null)
            {
                Debug.LogError("RigidbodyÇ™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
                return;
            }
            if (moveInput == Vector2.zero)
            {
                rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
                return;
            }
            Vector3 targetVelocity = new Vector3(moveInput.x, rigidbody.linearVelocity.y, moveInput.y);
            targetVelocity.Normalize();

            rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

            rigidbody.linearVelocity = targetVelocity * MOVE_SPEED;

            CurrentVelocity = rigidbody.linearVelocity;
        }
        private void OnEnable()
        {
            inputActions.Enable();
        }
        private void OnDisable()
        {
            inputActions.Disable();
        }
        private void OnFire(InputAction.CallbackContext context)
        {
            Debug.Log("Fire");
        }
    }
}
