using UnityEngine;
using UnityEngine.Events;
using Core.Interface;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        [field:SerializeField] public EnemyData EnemyDataAseet { get; private set; }
        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;
        private void OnEnable()
        {
            if (EnemyDataAseet == null)
            {
                Debug.LogError("EnemyDataがセットされていません。");
                return;
            }
            CurrentHP = EnemyDataAseet.MaxHP;
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

