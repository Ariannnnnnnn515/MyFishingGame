using Fishing.Core;
using Fishing.Visual;
using System;
using UnityEngine;

namespace Fishing.Systems
{
    /// <summary>
    /// Отвечает за анимацию броска, полёт лески и приводнение.
    /// Использует Curve для траектории.
    /// </summary>
    public class CastingSystem : MonoBehaviour
    {
        [Header("Настройки броска")]
        [SerializeField] private float castDuration = 1.5f;
        [SerializeField] private AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Header("Компоненты")]
        [SerializeField] private LineVisualizer lineVisual;
        [SerializeField] private Transform castOrigin; // Точка старта (рука/удочка)

        private Action onCompleteCallback;
        private Vector3 targetPos;
        private float castTimer;
        private bool isCasting;

        public void Initialize(FishingController controller) { /* Можно подписаться на события */ }

        /// <summary>
        /// Начать процесс заброса.
        /// </summary>
        public void StartCast(Vector3 target, Action callback)
        {
            targetPos = target;
            onCompleteCallback = callback;
            castTimer = 0f;
            isCasting = true;

            lineVisual?.EnableLine(true);
            Debug.Log($"Бросок в {target}");
        }

        private void Update()
        {
            if (!isCasting) return;

            castTimer += Time.deltaTime / castDuration;
            if (castTimer >= 1f)
            {
                isCasting = false;
                onCompleteCallback?.Invoke();
                return;
            }

            // Вычисляем позицию лески по кривой
            Vector3 currentPos = Vector3.Lerp(castOrigin.position, targetPos, castTimer);
            currentPos.y += heightCurve.Evaluate(castTimer) * 2f; // Высота дуги

            // Обновляем визуал лески
            lineVisual?.UpdateLine(castOrigin.position, currentPos);
        }

        public void ResetCast()
        {
            isCasting = false;
            lineVisual?.EnableLine(false);
        }
    }
}