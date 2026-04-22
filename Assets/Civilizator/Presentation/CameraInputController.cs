using UnityEngine;
using UnityEngine.InputSystem;

namespace Civilizator.Presentation
{
    /// <summary>
    /// Bridges the Input System camera actions to the isometric camera rig.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(IsometricCameraRig))]
    public sealed class CameraInputController : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset _inputActions;

        [SerializeField]
        private string _cameraMapName = "Camera";

        [SerializeField]
        private string _panActionName = "Pan";

        [SerializeField]
        private string _zoomActionName = "Zoom";

        [SerializeField]
        private float _panSpeed = 20f;

        [SerializeField]
        private float _zoomSpeed = 5f;

        private IsometricCameraRig _cameraRig;
        private InputActionMap _cameraMap;
        private InputAction _panAction;
        private InputAction _zoomAction;

        /// <summary>
        /// Returns true when the configured camera actions were found and cached.
        /// </summary>
        public bool HasBoundActions => _panAction != null && _zoomAction != null;

        /// <summary>
        /// Allows tests or scene bootstrap code to inject an action asset before binding.
        /// </summary>
        public void SetInputActions(InputActionAsset inputActions)
        {
            _inputActions = inputActions;
        }

        private void Awake()
        {
            _cameraRig = GetComponent<IsometricCameraRig>();
        }

        private void OnEnable()
        {
            RefreshBindings();
        }

        private void OnDisable()
        {
            if (_cameraMap != null)
            {
                _cameraMap.Disable();
            }
        }

        private void Update()
        {
            if (!HasBoundActions)
            {
                return;
            }

            ApplyInput(_panAction.ReadValue<Vector2>(), _zoomAction.ReadValue<float>(), Time.unscaledDeltaTime);
        }

        /// <summary>
        /// Resolves the configured action map and enables it for use.
        /// </summary>
        public void RefreshBindings()
        {
            _cameraRig ??= GetComponent<IsometricCameraRig>();

            if (_inputActions == null)
            {
                _cameraMap = null;
                _panAction = null;
                _zoomAction = null;
                return;
            }

            if (_cameraMap != null)
            {
                _cameraMap.Disable();
            }

            _cameraMap = _inputActions.FindActionMap(_cameraMapName, true);
            _panAction = _cameraMap.FindAction(_panActionName, true);
            _zoomAction = _cameraMap.FindAction(_zoomActionName, true);
            _cameraMap.Enable();
        }

        /// <summary>
        /// Applies camera movement from resolved input values.
        /// </summary>
        public void ApplyInput(Vector2 panInput, float zoomInput, float deltaTime)
        {
            _cameraRig ??= GetComponent<IsometricCameraRig>();
            if (_cameraRig == null)
            {
                return;
            }

            if (panInput != Vector2.zero)
            {
                _cameraRig.Pan(panInput * (_panSpeed * deltaTime));
            }

            if (Mathf.Abs(zoomInput) > Mathf.Epsilon)
            {
                _cameraRig.ZoomBy(-zoomInput * _zoomSpeed);
            }
        }
    }
}
