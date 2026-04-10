using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Test script to verify Input System UI module works: click a button and log success.
    /// Used for play mode verification of Input System UI integration.
    /// </summary>
    public sealed class TestInputSystemButton : MonoBehaviour
    {
        [SerializeField]
        private Button _testButton;

        private void OnEnable()
        {
            if (_testButton != null)
            {
                _testButton.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (_testButton != null)
            {
                _testButton.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            Debug.Log("[TestInputSystemButton] Button clicked successfully via Input System UI module!");
        }
    }
}
