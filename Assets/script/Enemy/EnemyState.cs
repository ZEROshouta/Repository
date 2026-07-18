using UnityEngine;
using UnityEngine.Events;
using Core.Interface;
using Core.MasterData;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        public EnemyDataRecord EnemyDataAseet { get; private set; }
        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;
        public void Initialize(ulong id)
        {
            EnemyDataAseet = MasterDataAccessor.Instance.GetById<EnemyDataRecord>(id);
        }
        public void Setup()
        {
            if (EnemyDataAseet == null)
            {
                Debug.LogError("EnemyDataがセットされていません。");
                return;
            }

            CurrentHP = EnemyDataAseet.MaxHP;

            gameObject.SetActive(true);
        }
        public void TakeDamage(int damageAmount)
        {
            if (damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;

            Debug.Log($"{EnemyDataAseet.EnemyName}に{damageAmount}のダメージ!残りHP:{CurrentHP}");

            if (CurrentHP <= 0)
            {
                Die();
            }
        }
        private void Die()
        {
            Debug.Log($"{EnemyDataAseet.EnemyName}を倒しました");

            gameObject.SetActive(false);

            OnReturnToPoolAction?.Invoke(this);
        }
    }
}

