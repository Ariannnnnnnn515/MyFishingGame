using System;
using UnityEngine;
using Fishing.Core;
using Fishing.Visual;

namespace Fishing.Systems
{
    public class CastingSystem : MonoBehaviour
    {
        [Header("Настройки заброса")]
        [SerializeField] private float castDuration = 1.5f;
        [SerializeField]
        private AnimationCurve heightCurve =
            AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Header("Ссылки")]
        [SerializeField] private LineVisualizer lineVisual;
        [SerializeField] private Transform castOrigin;

        private Action onCompleteCallback;
        private Vector3 targetPosition;
        private float castProgress;
        private bool isCasting;

        public void Initialize(FishingController controller)
        {
            // Контроллер пока не нужен: результат возвращаем через callback.
        }

        public void StartCast(Vector3 target, Action callback)
        {
            targetPosition = target;
            onCompleteCallback = callback;
            castProgress = 0f;
            isCasting = true;

            lineVisual?.EnableLine(true);
            Debug.Log($"Заброс в точку {targetPosition}");
        }

        private void Update()
        {
            if (!isCasting)
                return;

            castProgress += Time.deltaTime / castDuration;

            if (castProgress >= 1f)
            {
                lineVisual?.UpdateLine(castOrigin.position, targetPosition);
                isCasting = false;
                onCompleteCallback?.Invoke();
                return;
            }

            Vector3 currentPosition = Vector3.Lerp(
                castOrigin.position,
                targetPosition,
                castProgress
            );

            currentPosition.y += heightCurve.Evaluate(castProgress) * 2f;
            lineVisual?.UpdateLine(castOrigin.position, currentPosition);
        }

        public void ResetCast()
        {
            isCasting = false;
            lineVisual?.EnableLine(false);
        }
    }
}