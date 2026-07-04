using Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.ComponentModel;
using System;
using System.Threading;

namespace TPSRoguelite.InGame.Camera
{
    public class PlayerController : MonoBehaviour
    {
        private const float MOVE_SPEED = 5.0f;

        private const float ROTATE_SPEED = 10f;

        private const float LASER_MAX_DISTANCE = 50f;

        private const float ATTACK_RANGE = 50f;

        [SerializeField] private Rigidbody rigidbody;

        [SerializeField] private Transform weponOeigin;

        [SerializeField] private LineRenderer laserLineRenderer;

        [SerializeField] private WeaponData currentWeapon;

        private PlayerInputActions inputActions;

        private Vector2 moveInput = Vector2.zero;

        private Vector3 moveDirection = Vector3.zero;

        private Transform mainCameraTransform;

        private bool isReloading;

        private bool canShoot;

        private CancellationTokenSource fireCts;

        public Vector3 CurrentVelocity { get; private set; }

        public int CurrentAmmo { get; private set; }
        private void Awake()
        {
            if (currentWeapon != null)
            {
                CurrentAmmo = currentWeapon.MaxAmmo;
            }
            else
            {
                Debug.LogError("WeaponDataがありません。");
            }

            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire;
            inputActions.Player.Fire.canceled += OnFire;
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
            if (context.performed)
            {
                if (canShoot || isReloading || currentWeapon == null)
                {
                    return;
                }

                fireCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(fireCts.Token, this.GetCancellationTokenOnDestroy());

                switch (currentWeapon.WeaponFireType)
                {
                    case Enum.FireType.SemiAuto:
                        ShootBurstAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.Bust:
                        ShootSemiAutoAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.FullAuto:
                        ShootFullAutoAsync(linkedCts.Token).Forget();
                        break;

                    default:
                        Debug.LogWarning($"割り当てていない射撃タイプがあります。{currentWeapon.WeaponFireType}");
                        break;
                }
            }

            if (context.canceled)
            {
                fireCts?.Cancel();
                fireCts?.Dispose();
                fireCts = null;
            }
        }
        private async UniTaskVoid ShootSemiAutoAsync(CancellationToken token)
        {
            canShoot = true;

            if (CurrentAmmo == 0)
            {
                ReloadAsync().Forget();
                return;
            }
            canShoot = false;

            CurrentAmmo--;
            Debug.Log($"セミオートで撃った ! 弾数: {CurrentAmmo}");
            Shoot();

            await (UniTask.Delay(System.TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token));

            canShoot = true;
        }

        private async UniTaskVoid ShootBurstAsync(CancellationToken token)
        {
            canShoot = false;

            for (int i = 0; i < 3; i++)
            {
                if (CurrentAmmo <= 0)
                {
                    ReloadAsync().Forget();
                    break;
                }

                CurrentAmmo--;
                Shoot();
                Debug.Log($"バースト ! 残弾数: {CurrentAmmo}");

                await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInteval), cancellationToken: token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token);
            canShoot = true;
        }
        private async UniTaskVoid ShootFullAutoAsync(CancellationToken token)
        {
            canShoot = false;

            while (!token.IsCancellationRequested)
            {
                if (CurrentAmmo <= 0)
                {
                    ReloadAsync().Forget();
                    break;
                }

                CurrentAmmo--;
                Debug.Log($"フルオート ! 残弾数 : {CurrentAmmo}");
                Shoot();

                bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInteval), cancellationToken: token).SuppressCancellationThrow();
                if (isCanceled)
                {
                    break;
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token);
            canShoot = true;
        }
        private void Shoot()
        {
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中!");

                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                if (target != null)
                {
                    target.TakeDamage(currentWeapon.AttackPower);
                }
            }
        }
        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == currentWeapon.MaxAmmo)
            {
                return;
            }

            ReloadAsync().Forget();
        }
        private async UniTask ReloadAsync()
        {
            isReloading = true;
            Debug.Log("リロード中");

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.ReloadTime), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentAmmo = currentWeapon.MaxAmmo;
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
