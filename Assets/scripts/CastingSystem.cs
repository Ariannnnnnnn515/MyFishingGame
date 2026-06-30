using Fishing.Core;
using Fishing.Visual;
using System;
using UnityEngine;

namespace Fishing.Systems
{
    /// <summary>
    /// �������� �� �������� ������, ���� ����� � �����������.
    /// ���������� Curve ��� ����������.
    /// </summary>
    public class CastingSystem : MonoBehaviour
    {
        [Header("��������� ������")]
        [SerializeField] private float castDuration = 1.5f;
        [SerializeField] private AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Header("����������")]
        [SerializeField] private LineVisualizer lineVisual;
        [SerializeField] private Transform castOrigin; // ����� ������ (����/������)

        private Action onCompleteCallback;
        private Vector3 targetPos;
        private float castTimer;
        private bool isCasting;

        public void Initialize(FishingController controller) { /* ����� ����������� �� ������� */ }

        /// <summary>
        /// ������ ������� �������.
        /// </summary>
        public void StartCast(Vector3 target, Action callback)
        {
            targetPos = target;
            onCompleteCallback = callback;
            castTimer = 0f;
            isCasting = true;

            lineVisual?.EnableLine(true);
            Debug.Log($"������ � {target}");
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

            // ��������� ������� ����� �� ������
            Vector3 currentPos = Vector3.Lerp(castOrigin.position, targetPos, castTimer);
            currentPos.y += heightCurve.Evaluate(castTimer) * 2f; // ������ ����

            // ��������� ������ �����
            lineVisual?.UpdateLine(castOrigin.position, currentPos);
        }

        public void ResetCast()
        {
            isCasting = false;
            lineVisual?.EnableLine(false);
        }
    }
}