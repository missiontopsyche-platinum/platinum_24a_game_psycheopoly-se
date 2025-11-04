using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
	/// <summary>
	/// Sanity checks to ensure that the Unity Testing Framework is set up correctly.
	/// These tests should always pass if all is correctly configured.
	/// </summary>
	public class _TestFrameworkVerification
	{
		[Test]
		public void TestFramework_IsConfigured()
		{
			Assert.IsTrue(true, "Test framework should be operational.");
		}

		[Test]
		public void TestFramework_CanPerformBasicMath()
		{
			int result = 2 + 2;
			Assert.AreEqual(4, result, "Basic math operations should work in tests.");
		}

		[Test]
		public void TestFramework_CanCreateUnityObjects()
		{
			GameObject testObject = new GameObject("TestObject");

			Assert.IsNotNull(testObject, "Should be able to create GameObjects in tests.");
			Assert.AreEqual("TestObject", testObject.name, "GameObject should have correct name.");

			Object.DestroyImmediate(testObject);
		}
	}
}
