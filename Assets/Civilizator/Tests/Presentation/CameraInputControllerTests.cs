using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Civilizator.Presentation.Tests
{
    [TestFixture]
    public class CameraInputControllerTests
    {
        private GameObject gameObject;
        private Camera camera;
        private IsometricCameraRig rig;
        private CameraInputController controller;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("CameraInputController");
            camera = gameObject.AddComponent<Camera>();
            rig = gameObject.AddComponent<IsometricCameraRig>();
            controller = gameObject.AddComponent<CameraInputController>();
            rig.Reframe();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void RefreshBindingsBuildsFallbackCameraActionsWhenNoAssetIsAssigned()
        {
            controller.RefreshBindings();

            Assert.IsTrue(controller.HasBoundActions);
            Assert.IsTrue(controller.gameObject.GetComponent<IsometricCameraRig>() != null);
        }

        [Test]
        public void RefreshBindingsFindsCameraActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Camera");
            map.AddAction("Pan", InputActionType.Value, expectedControlLayout: "Vector2");
            map.AddAction("Zoom", InputActionType.Value, expectedControlLayout: "Axis");
            asset.AddActionMap(map);

            controller.SetInputActions(asset);
            controller.RefreshBindings();

            Assert.IsTrue(controller.HasBoundActions);
            Assert.IsTrue(map.enabled);
        }

        [Test]
        public void ApplyInputMovesAndZoomsTheRig()
        {
            controller.ApplyInput(new Vector2(1f, 0.5f), 1f, 1f);

            Quaternion rotation = Quaternion.Euler(30f, 45f, 0f);
            Vector3 expectedFocus = new Vector3(69.5f, 0f, 59.5f);
            Vector3 expectedPosition = expectedFocus - rotation * Vector3.forward * 100f;

            Assert.That(Vector3.Distance(expectedPosition, gameObject.transform.position), Is.LessThan(0.0001f));
            Assert.AreEqual(70f, camera.orthographicSize, 0.0001f);
        }
    }
}
