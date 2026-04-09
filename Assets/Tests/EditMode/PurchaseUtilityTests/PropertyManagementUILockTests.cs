using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.PropertyManagementTests
{
    public class PropertyManagementUILockTests
    {
        private GameObject go;
        private PropertyManagementUIController controller;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("PropertyManagementUI");
            controller = go.AddComponent<PropertyManagementUIController>();
            go.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void HideWhenNotInDebtMode()
        {
            SetPrivateField(controller, "isDebtResolutionMode", false);
            SetPrivateField(controller, "currentDebtAmount", 0);

            controller.Hide();

            Assert.IsFalse(go.activeSelf);
        }

        [Test]
        public void HideWhenInDebtModeWithDebtRemaining()
        {
            SetPrivateField(controller, "isDebtResolutionMode", true);
            SetPrivateField(controller, "currentDebtAmount", 200);

            controller.Hide();

            Assert.IsTrue(go.activeSelf);
        }

        [Test]
        public void HideWhenInDebtModeButDebtCleared()
        {
            SetPrivateField(controller, "isDebtResolutionMode", true);
            SetPrivateField(controller, "currentDebtAmount", 0);

            controller.Hide();

            Assert.IsFalse(go.activeSelf);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            field.SetValue(target, value);
        }
    }
}