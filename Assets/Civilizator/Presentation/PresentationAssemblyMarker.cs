using Civilizator.Simulation;
using UnityEngine;

namespace Civilizator.Presentation
{
    public sealed class PresentationAssemblyMarker : MonoBehaviour
    {
        [SerializeField]
#pragma warning disable CS0414
        private int _linkedSimulationAssemblyVersion = SimulationAssemblyMarker.Version;
#pragma warning restore CS0414
    }
}
