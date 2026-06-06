using UnityEngine;
using Core.Interface;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        private const int MAX_HP = 100;

        public int CurrentHP { get; private set; }
        private void Awake()
        {
            CurrentHP = MAX_HP;
        }
        public void TakeDamage(int damageAmount)
        {
            if (damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"“G‚É{damageAmount}‚Ìƒ_ƒ[ƒW!Žc‚èHP:{CurrentHP}");

            if (CurrentHP <= 0)
            {
                Die();
            }
        }
        private void Die()
        {
            Debug.Log("“G‚ð“|‚µ‚Ü‚µ‚½");
            Destroy(gameObject);
        }
    }
}

