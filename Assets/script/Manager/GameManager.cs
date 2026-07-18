using Cysharp.Threading.Tasks;
using TPSRoguelite.InGame.Camera;
using TPSRoguelite.InGame.Spawner;
using UnityEngine;
using Core.MasterData;

namespace TPSRoguelite.InGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player = null;

        [SerializeField] private EnemySpawner enemySpawner = null;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            Setup().Forget();
        }
        private async UniTaskVoid Setup()
        {
            await MasterDataAccessor.Instance.InitializeAsync();

            if (player != null)
            {
                player.Setup();
            }

            if (enemySpawner != null)
            {
                enemySpawner.Setup();
            }
        }
    }
}
