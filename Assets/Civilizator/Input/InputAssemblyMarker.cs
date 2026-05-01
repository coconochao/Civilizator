using UnityEngine;
using UnityEngine.InputSystem;

namespace Civilizator.Input
{
    /// <summary>
    /// Compile-time anchor: ensures Input asmdef resolves Unity Input System. Assign an input actions asset in the inspector when wiring camera/UI input.
    /// </summary>
    public sealed class InputAssemblyMarker : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset _inputActions;
    }
}
