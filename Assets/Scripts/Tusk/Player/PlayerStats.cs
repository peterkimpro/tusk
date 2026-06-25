using System;
using UnityEngine;

namespace Tusk.Player
{
    /// <summary>Player HP + damage events. Stamina lives on PlayerController.</summary>
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private float maxHP = 100f;

        public float HP { get; private set; }
        public float MaxHP => maxHP;
        public bool IsDead => HP <= 0f;

        public event Action<float, float> OnHPChanged; // (current, max)
        public event Action OnDied;

        private void Awake()
        {
            HP = maxHP;
            OnHPChanged?.Invoke(HP, maxHP);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            HP = Mathf.Max(0f, HP - amount);
            OnHPChanged?.Invoke(HP, maxHP);
            if (HP <= 0f) OnDied?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            HP = Mathf.Min(maxHP, HP + amount);
            OnHPChanged?.Invoke(HP, maxHP);
        }
    }
}
