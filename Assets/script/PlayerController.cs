using Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.ComponentModel;
using System;

namespace TPSRoguelite.InGame.Camera
{
    public class PlayerController : MonoBehaviour
    {
        private const float MOVE_SPEED = 5.0f;

        private const float ROTATE_SPEED = 10f;

        private const float LASER_MAX_DISTANCE = 50f;

        private const int ATTACK_DAMAGE = 20;

        private const float ATTACK_RANGE = 50f;

        private const int MAX_AMMO = 30;

        private const float RELOAD_TIME = 1.5f;

        [SerializeField] private Rigidbody rigidbody;

        [SerializeField] private Transform weponOeigin;

        [SerializeField] private LineRenderer laserLineRenderer;

        private PlayerInputActions inputActions;

        private Vector2 moveInput = Vector2.zero;

        private Vector3 moveDirection = Vector3.zero;

        private Transform mainCameraTransform;

        private bool isReloading;

        public Vector3 CurrentVelocity { get; private set; }

        public int CurrentAmmo { get; private set; }
        private void Awake()
        {
            CurrentAmmo = MAX_AMMO;

            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire;
            inputActions.Player.Reload.performed += OnReload;

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("MainCameraが見つかりません。");
            }
        }
        void Update()
        {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            DrawLaserPointer();
        }
        private void FixedUpdate()
        {
            Move();
        }
        private void Move()
        {
            if (rigidbody == null)
            {
                Debug.LogError("Rigidbodyが設定されていません。");
                return;
            }
            if (moveInput == Vector2.zero)
            {
                rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
                return;
            }

            Vector3 cameraForward = mainCameraTransform.forward;
            Vector3 cameraRight = mainCameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            Quaternion targeRotation = Quaternion.LookRotation(moveDirection);
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targeRotation, ROTATE_SPEED * Time.fixedDeltaTime);

            Vector3 targetVelocity = moveDirection * MOVE_SPEED;
            rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

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
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中!");

                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                if (target != null)
                {
                    target.TakeDamage(ATTACK_DAMAGE);
                }
            }
        }
        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == MAX_AMMO)
            {
                return;
            }

            ReloadAsync().Forget();
        }
        private async UniTask ReloadAsync()
        {
            isReloading = true;
            Debug.Log("リロード中");

            await UniTask.Delay(TimeSpan.FromSeconds(RELOAD_TIME), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentAmmo = MAX_AMMO;
            isReloading = false;
            Debug.Log("リロード完了");
        }
        private void DrawLaserPointer()
        {
            if (laserLineRenderer == null || weponOeigin == null || mainCameraTransform == null)
            {
                return;
            }

            laserLineRenderer.SetPosition(0, weponOeigin.position);

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
            {
                laserLineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {
                laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }
        }
    }
}
