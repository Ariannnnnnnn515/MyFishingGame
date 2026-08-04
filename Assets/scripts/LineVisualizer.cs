using UnityEngine;

namespace Fishing.Visual
{
    /// <summary>
    /// Отрисовывает леску как кривую Безье между точками.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LineVisualizer : MonoBehaviour
    {
        [SerializeField] private int segments = 20;
        [SerializeField] private float sagAmount = 0.5f; // Провисание

        private LineRenderer lineRenderer;
        private Vector3 startPoint;
        private Vector3 endPoint;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = segments;
            lineRenderer.enabled = false; // До заброса леску не показываем.
        }

        /// <summary>
        /// Обновить линию между двумя точками.
        /// </summary>
        public void UpdateLine(Vector3 start, Vector3 end)
        {
            startPoint = start;
            endPoint = end;

            Vector3 mid = (start + end) / 2;
            mid.y -= sagAmount; // Провисание вниз

            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)(segments - 1);
                Vector3 point = CalculateQuadraticBezier(start, mid, end, t);
                lineRenderer.SetPosition(i, point);
            }
        }

        private Vector3 CalculateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1 - t;
            return u * u * p0 + 2 * u * t * p1 + t * t * p2;
        }

        public void EnableLine(bool enable) => lineRenderer.enabled = enable;
    }
}