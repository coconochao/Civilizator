using Civilizator.Simulation;
using UnityEngine;

namespace Civilizator.Presentation
{
    public sealed class PresentationAssemblyMarker : MonoBehaviour
    {
        [SerializeField]
        private int _linkedSimulationAssemblyVersion = SimulationAssemblyMarker.Version;
    }
}
