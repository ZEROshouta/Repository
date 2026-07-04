using UnityEngine;
using UnityEngine.AI;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent = null;

        [SerializeField] private EnemyState enemyState = null;

        private Transform targetPlayer = null;

        private void Awake()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetPlayer = player.transform;
            }
            else
            {
                Debug.LogError("Playerというタグのついたオブジェクトが見つかりませんでした。");
            }
            if (navMeshAgent != null && enemyState != null && enemyState.EnemyDataAseet != null)
            {

            }
        }
        void Update()
        {
            if (targetPlayer != null && navMeshAgent != null)
            {
                navMeshAgent.SetDestination(targetPlayer.position);
            }
        }
    }
}
