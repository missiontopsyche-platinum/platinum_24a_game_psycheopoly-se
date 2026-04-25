using NUnit.Framework;
using Assets.Scripts.Managers.Rent;
using Events.EventDataStructures;

namespace Tests.EditMode.RentTests
{
    /// <summary>
    /// Verifies that RentManager computes rent and transfers money the right way
    /// </summary>
    public class RentManagerIntegrationTests : RentTestBase
    {
        [Test]
        public void Charges_BaseRent_ToOwner()
        {
            var property = Street("Prop A", ColorGroup.Red, baseRent: 10);
            ownership.SetOwner(property, owner);

            AssertRentTransfer(property, diceTotal: 7, expectedRent: 10);
        }

        [Test]
        public void Monopoly_Doubles_BaseRent()
        {
            var a = Street("A", ColorGroup.Orange, baseRent: 10);
            var b = Street("B", ColorGroup.Orange, baseRent: 10);
            var c = Street("C", ColorGroup.Orange, baseRent: 10);

            ownership.SetOwner(a, owner);
            ownership.SetOwner(b, owner);
            ownership.SetOwner(c, owner);

            AssertRentTransfer(a, diceTotal: 0, expectedRent: 20);
        }

        [Test]
        public void Houses_Use_RentTable()
        {
            var rentTable = new[] { 8, 40, 100, 300, 450, 600 };
            var property = Street(
                "Housey",
                ColorGroup.Yellow,
                baseRent: 8,
                houses: 3,
                mortgaged: false,
                rentTable: rentTable);

            ownership.SetOwner(property, owner);

            AssertRentTransfer(property, diceTotal: 0, expectedRent: 300);
        }

        [Test]
        public void Railroad_Rent_Scales_With_Owned_Count()
        {
            var rr1 = Railroad("RR1");
            var rr2 = Railroad("RR2");
            var rr3 = Railroad("RR3");
            var rr4 = Railroad("RR4");

            ownership.SetOwner(rr1, owner);
            AssertRentTransfer(rr1, diceTotal: 0, expectedRent: 25);

            ownership.SetOwner(rr2, owner);
            AssertRentTransfer(rr1, diceTotal: 0, expectedRent: 50);

            ownership.SetOwner(rr3, owner);
            AssertRentTransfer(rr1, diceTotal: 0, expectedRent: 100);

            ownership.SetOwner(rr4, owner);
            AssertRentTransfer(rr1, diceTotal: 0, expectedRent: 200);
        }

        [Test]
        public void Utility_Rent_Uses_4x_When_One_Utility_Is_Owned()
        {
            var electric = Utility("Electric");
            ownership.SetOwner(electric, owner);

            AssertRentTransfer(electric, diceTotal: 7, expectedRent: 28);
        }

        [Test]
        public void Utility_Rent_Uses_10x_When_Both_Utilities_Are_Owned()
        {
            var electric = Utility("Electric");
            var water = Utility("Water");

            ownership.SetOwner(electric, owner);
            ownership.SetOwner(water, owner);

            AssertRentTransfer(electric, diceTotal: 7, expectedRent: 70);
        }

        [Test]
        public void Mortgaged_Property_Does_Not_Charge_Rent()
        {
            var property = Street("Mort", ColorGroup.Red, baseRent: 20, mortgaged: true);
            ownership.SetOwner(property, owner);

            AssertNoMoneyTransfer(property, diceTotal: 0);
        }

        [Test]
        public void Landing_On_Your_Own_Property_Does_Not_Charge_Rent()
        {
            var property = Street("Self", ColorGroup.Green, baseRent: 30);
            ownership.SetOwner(property, tenant);

            AssertNoMoneyTransfer(property, diceTotal: 0);
        }

        private void AssertRentTransfer(ITileRentInfo tile, int diceTotal, int expectedRent)
        {
            int tenantBefore = tenant.GetMoney();
            int ownerBefore = owner.GetMoney();

            rentManager.TryChargeRent(tenant, tile, diceTotal);

            Assert.AreEqual(
                tenantBefore - expectedRent,
                tenant.GetMoney(),
                "Tenant money did not decrease by expected amount.");

            Assert.AreEqual(
                ownerBefore + expectedRent,
                owner.GetMoney(),
                "Owner money did not increase by expected amount.");
        }

        private void AssertNoMoneyTransfer(ITileRentInfo tile, int diceTotal)
        {
            int tenantBefore = tenant.GetMoney();
            int ownerBefore = owner.GetMoney();

            rentManager.TryChargeRent(tenant, tile, diceTotal);

            Assert.AreEqual(
                tenantBefore,
                tenant.GetMoney(),
                "Tenant money should not have changed.");

            Assert.AreEqual(
                ownerBefore,
                owner.GetMoney(),
                "Owner money should not have changed.");
        }
    }
}