using UnityEngine;

namespace CastleAssault.Environment
{
    /// <summary>
    /// Controla la cámara 2D vertical scroller: sigue al jugador hacia arriba
    /// con un offset visual hacia adelante y gestiona sacudidas de cámara (Screen Shake).
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [SerializeField] private Transform _target;
        [SerializeField] private float _verticalOffset = 3.5f;
        [SerializeField] private float _smoothSpeed = 10f;

        private float _shakeDuration;
        private float _shakeIntensity;
        private Vector3 _shakeOffset;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Seguimiento suave del eje Y
            float targetY = _target.position.y + _verticalOffset;
            float currentY = transform.position.y;
            float newY = Mathf.Lerp(currentY, targetY, _smoothSpeed * Time.deltaTime);

            // Manejo de Screen Shake
            if (_shakeDuration > 0f)
            {
                _shakeOffset = (Vector3)(Random.insideUnitCircle * _shakeIntensity);
                _shakeDuration -= Time.deltaTime;
                _shakeIntensity = Mathf.Lerp(_shakeIntensity, 0f, Time.deltaTime * 6f);
            }
            else
            {
                _shakeOffset = Vector3.zero;
            }

            transform.position = new Vector3(_shakeOffset.x, newY + _shakeOffset.y, -10f);
        }

        public void Shake(float duration, float intensity)
        {
            _shakeDuration = duration;
            _shakeIntensity = intensity;
        }
    }
}
