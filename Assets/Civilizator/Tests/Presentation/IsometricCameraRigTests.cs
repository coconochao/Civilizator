using NUnit.Framework;
using UnityEngine;

namespace Civilizator.Presentation.Tests
{
    [TestFixture]
    public class IsometricCameraRigTests
    {
        private GameObject gameObject;
        private Camera camera;
        private IsometricCameraRig rig;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("IsometricCameraRig");
            camera = gameObject.AddComponent<Camera>();
            rig = gameObject.AddComponent<IsometricCameraRig>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void ReframeConfiguresOrthographicIsometricCamera()
        {
            rig.Reframe();

            Assert.IsTrue(camera.orthographic);
            Assert.AreEqual(75f, camera.orthographicSize, 0.0001f);
            Assert.That(Quaternion.Angle(Quaternion.Euler(30f, 45f, 0f), gameObject.transform.rotation), Is.LessThan(0.0001f));
            Assert.That(rig.FocusPoint, Is.EqualTo(new Vector3(49.5f, 0f, 49.5f)));
        }

        [Test]
        public void ReframePlacesCameraOnTheIsometricOrbit()
        {
            rig.Reframe();

            Quaternion rotation = Quaternion.Euler(30f, 45f, 0f);
            Vector3 expectedPosition = rig.FocusPoint - rotation * Vector3.forward * 100f;

            Assert.That(Vector3.Distance(expectedPosition, gameObject.transform.position), Is.LessThan(0.0001f));
        }

        [Test]
        public void PanMovesTheFocusPointInWorldSpace()
        {
            rig.Reframe();
            rig.Pan(new Vector2(3f, -2f));

            Assert.That(rig.FocusPoint, Is.EqualTo(new Vector3(52.5f, 0f, 47.5f)));
        }

        [Test]
        public void ZoomIsClampedWithinTheConfiguredBounds()
        {
            rig.SetZoom(999f);
            Assert.AreEqual(140f, camera.orthographicSize, 0.0001f);

            rig.SetZoom(1f);
            Assert.AreEqual(20f, camera.orthographicSize, 0.0001f);
        }
    }
}
