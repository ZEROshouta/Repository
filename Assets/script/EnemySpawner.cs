using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TPSRoguelite.InGame.Enemy;
using Unity.VisualScripting;
using Core.MasterData;

namespace TPSRoguelite.InGame.Spawner
{
    public class EnemySpawner : MonoBehaviour
    {
        private const float SPAWN_INTERVAL = 3.0f;

        private const float MAX_SPAWN_DISTANCE = 2.0f;

        private const int POOL_SIZE = 20;

        [SerializeField] GameObject enemyPrefab = null;

        [SerializeField] private Transform[] spawnPoints;

        private Queue<EnemyState> enemyPool = new Queue<EnemyState>();
        public void Setup()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            for (int i = 0; i < POOL_SIZE; i++)
            {
                GameObject enemyObj = Instantiate(enemyPrefab);

                EnemyState enemy = enemyObj.GetComponent<EnemyState>();

                if (enemy != null)
                {
                    ulong randomId = (ulong)UnityEngine.Random.Range(1, MasterDataAccessor.Instance.Count<EnemyDataRecord>());

                    enemy.Initialize(randomId);

                    enemy.gameObject.SetActive(false);

                    enemyPool.Enqueue(enemy);
                }
            }

            SpawnLoopAsync().Forget();
        }
        private async UniTaskVoid SpawnLoopAsync()
        {
            var token = this.GetCancellationTokenOnDestroy();

            while (true)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(SPAWN_INTERVAL));

                SpawnEnemyFromPool();
            }
        }
        private void SpawnEnemyFromPool()
        {
            if (enemyPrefab == null || spawnPoints.Length == 0)
            {
                return;
            }

            int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);

            Transform spawnPoint = spawnPoints[randomIndex];

            Vector3 safePosition = spawnPoint.position;

            if (NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, MAX_SPAWN_DISTANCE, NavMesh.AllAreas))
            {
                safePosition = hit.position;
            }
            else
            {
                Debug.LogWarning("近くに安全なスポーン位置が見つかりませんでした。");
                return;
            }

            EnemyState enemy = null;

            if (enemyPool.Count > 0)
            {
                enemy = enemyPool.Dequeue();
            }
            else
            {
                Debug.LogWarning("プールに空きがなかったため、Instantiateで生成します。プールサイズを増やすか、生成に制限をかけてください");

                GameObject enemyObj = Instantiate(enemyPrefab);

                enemy = enemyObj.GetComponent<EnemyState>();

                if (enemy == null)
                {
                    Debug.LogError("EnemyStateの取得に失敗しました。");
                    return;
                }
            }
            enemy.OnReturnToPoolAction -= ReturnToPool;

            enemy.OnReturnToPoolAction += ReturnToPool;

            enemy.transform.position = safePosition;

            enemy.transform.rotation = spawnPoint.rotation;

            enemy.Setup();
        }
        private void ReturnToPool(EnemyState enemy)
        {
            enemyPool.Enqueue(enemy);

            enemy.OnReturnToPoolAction -= ReturnToPool;
        }
    }
}
