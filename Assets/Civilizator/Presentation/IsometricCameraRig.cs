using Civilizator.Simulation;
using UnityEngine;

namespace Civilizator.Presentation
{
    /// <summary>
    /// Configures the main camera as an orthographic isometric rig for the 100x100 map.
    /// The component keeps the camera ready for later pan/zoom input wiring without depending on it.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricCameraRig : MonoBehaviour
    {
        [SerializeField]
        private int _mapWidthTiles = GridPos.MapWidth;

        [SerializeField]
        private int _mapHeightTiles = GridPos.MapHeight;

        [SerializeField]
        private float _tileWorldSize = 1f;

        [SerializeField]
        private float _pitchDegrees = 30f;

        [SerializeField]
        private float _yawDegrees = 45f;

        [SerializeField]
        private float _cameraDistance = 100f;

        [SerializeField]
        private float _orthographicPadding = 1.5f;

        [SerializeField]
        private float _minOrthographicSize = 20f;

        [SerializeField]
        private float _maxOrthographicSize = 140f;

        private Camera _camera;
        private Vector3 _focusPoint;

        /// <summary>
        /// Gets the camera component managed by this rig.
        /// </summary>
        public Camera Camera => _camera ??= GetComponent<Camera>();

        /// <summary>
        /// Current world-space focus point used by the rig.
        /// </summary>
        public Vector3 FocusPoint => _focusPoint;

        private void Reset()
        {
            Reframe();
        }

        private void Awake()
        {
            Reframe();
        }

        private void OnValidate()
        {
            Reframe();
        }

        /// <summary>
        /// Recenter the camera on the map and apply the default isometric framing.
        /// </summary>
        public void Reframe()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 1000f;

            _focusPoint = GetMapCenter();
            ApplyPose();
            _camera.orthographicSize = GetRecommendedOrthographicSize();
        }

        /// <summary>
        /// Move the rig focus point in world-space tile coordinates.
        /// </summary>
        public void Pan(Vector2 worldDelta)
        {
            _focusPoint += new Vector3(worldDelta.x, 0f, worldDelta.y);
            ApplyPose();
        }

        /// <summary>
        /// Directly set the focus point in world space.
        /// </summary>
        public void FocusOn(Vector3 worldPoint)
        {
            _focusPoint = worldPoint;
            ApplyPose();
        }

        /// <summary>
        /// Clamp and set the camera zoom level.
        /// </summary>
        public void SetZoom(float orthographicSize)
        {
            Camera.orthographicSize = Mathf.Clamp(orthographicSize, _minOrthographicSize, _maxOrthographicSize);
        }

        /// <summary>
        /// Adjust the zoom level by an offset.
        /// </summary>
        public void ZoomBy(float delta)
        {
            SetZoom(Camera.orthographicSize + delta);
        }

        /// <summary>
        /// Compute the default orthographic size used to frame the full map.
        /// </summary>
        public float GetRecommendedOrthographicSize()
        {
            float halfWidth = _mapWidthTiles * _tileWorldSize * 0.5f;
            float halfHeight = _mapHeightTiles * _tileWorldSize * 0.5f;
            float paddedSize = Mathf.Max(halfWidth, halfHeight) * _orthographicPadding;
            return Mathf.Clamp(paddedSize, _minOrthographicSize, _maxOrthographicSize);
        }

        private Vector3 GetMapCenter()
        {
            return new Vector3(
                (_mapWidthTiles - 1) * 0.5f * _tileWorldSize,
                0f,
                (_mapHeightTiles - 1) * 0.5f * _tileWorldSize);
        }

        private void ApplyPose()
        {
            Quaternion rotation = Quaternion.Euler(_pitchDegrees, _yawDegrees, 0f);
            transform.rotation = rotation;
            transform.position = _focusPoint - rotation * Vector3.forward * Mathf.Max(_cameraDistance, 0.01f);
        }
    }
}
