using Tusk.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Tusk.UI
{
    /// <summary>HP + stamina bars, plus dev hint text overlay.</summary>
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private PlayerController controller;
        [SerializeField] private Image hpFill;
        [SerializeField] private Image staminaFill;
        [SerializeField] private Text hintText;

        private void Awake()
        {
            if (stats != null) stats.OnHPChanged += HandleHP;
            if (hintText != null)
                hintText.text = "WASD move · Shift sprint · Space jump · Mouse look";
        }

        private void OnDestroy()
        {
            if (stats != null) stats.OnHPChanged -= HandleHP;
        }

        private void HandleHP(float current, float max)
        {
            if (hpFill != null) hpFill.fillAmount = max > 0f ? current / max : 0f;
        }

        private void Update()
        {
            if (controller != null && staminaFill != null)
            {
                staminaFill.fillAmount = controller.MaxStamina > 0f
                    ? controller.Stamina / controller.MaxStamina
                    : 0f;
            }
        }
    }
}
