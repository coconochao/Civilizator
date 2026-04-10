using Civilizator.Presentation;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Compile-time anchor: ensures UI asmdef references Presentation, uGUI, and Input System UI resolve.
    /// </summary>
    public sealed class UIAssemblyMarker : MonoBehaviour
    {
        [SerializeField]
        private Button _button;

        [SerializeField]
        private InputSystemUIInputModule _inputModule;

        [SerializeField]
        private PresentationAssemblyMarker _presentation;
    }
}
