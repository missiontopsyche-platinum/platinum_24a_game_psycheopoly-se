using NUnit.Framework;
using Assets.Scripts.Managers.Rent;
using UnityEngine;

namespace Tests.EditMode.RentTests
{
    ///Verifies RentManager actually moves money using the strategy and adapters.
    public class RentManagerIntegrationTests : RentTestBase
    {
        [Test]
        public void Charges_BaseRent_ToOwner()
        {
            //Given a street with base 10 no monopoly and no houses
            var prop = Street("Prop A", ColorGroup.Red, baseRent: 10, houses: 0, mortgaged: false);
            ownership.SetOwner(prop, owner);

            int beforeTenant = tenant.GetMoney();
            int beforeOwner  = owner.GetMoney();

            rentManager.TryChargeRent(tenant, prop, diceTotal: 7); //dice ignored for streets

            Assert.AreEqual(beforeTenant - 10, tenant.GetMoney());
            Assert.AreEqual(beforeOwner  + 10, owner.GetMoney());
        }

        [Test]
        public void Monopoly_Doubles_Base()
        {
            var a = Street("A", ColorGroup.Orange, 10);
            var b = Street("B", ColorGroup.Orange, 10);
            var c = Street("C", ColorGroup.Orange, 10);

            ownership.SetOwner(a, owner);
            ownership.SetOwner(b, owner);
            ownership.SetOwner(c, owner);

            int beforeTenant = tenant.GetMoney();
            int beforeOwner  = owner.GetMoney();

            rentManager.TryChargeRent(tenant, a, diceTotal: 0);

            Assert.AreEqual(beforeTenant - 20, tenant.GetMoney()); //doubled base
            Assert.AreEqual(beforeOwner  + 20, owner.GetMoney());
        }

        [Test]
        public void Houses_Use_Table()
        {
            var table = new[] { 8, 40, 100, 300, 450, 600 };
            var prop  = Street("Housey", ColorGroup.Yellow, baseRent: 8, houses: 3, mortgaged: false, rentTable: table);
            ownership.SetOwner(prop, owner);

            int beforeTenant = tenant.GetMoney();
            int beforeOwner  = owner.GetMoney();

            rentManager.TryChargeRent(tenant, prop, 0);

            Assert.AreEqual(beforeTenant - 300, tenant.GetMoney());
            Assert.AreEqual(beforeOwner  + 300, owner.GetMoney());
        }

        [Test]
        public void Railroad_Scales_Properly()
        {
            var rr1 = Railroad("RR1");
            var rr2 = Railroad("RR2");
            var rr3 = Railroad("RR3");
            var rr4 = Railroad("RR4");

            ownership.SetOwner(rr1, owner);

            int t0 = tenant.GetMoney();
            int o0 = owner.GetMoney();
            rentManager.TryChargeRent(tenant, rr1, 0);
            Assert.AreEqual(t0 - 25, tenant.GetMoney());
            Assert.AreEqual(o0 + 25, owner.GetMoney());

            ownership.SetOwner(rr2, owner);
            int t1 = tenant.GetMoney();
            int o1 = owner.GetMoney();
            rentManager.TryChargeRent(tenant, rr1, 0);
            Assert.AreEqual(t1 - 50, tenant.GetMoney());
            Assert.AreEqual(o1 + 50, owner.GetMoney());

            ownership.SetOwner(rr3, owner);
            int t2 = tenant.GetMoney();
            int o2 = owner.GetMoney();
            rentManager.TryChargeRent(tenant, rr1, 0);
            Assert.AreEqual(t2 - 100, tenant.GetMoney());
            Assert.AreEqual(o2 + 100, owner.GetMoney());

            ownership.SetOwner(rr4, owner);
            int t3 = tenant.GetMoney();
            int o3 = owner.GetMoney();
            rentManager.TryChargeRent(tenant, rr1, 0);
            Assert.AreEqual(t3 - 200, tenant.GetMoney());
            Assert.AreEqual(o3 + 200, owner.GetMoney());
        }

        [Test]
        public void Utilities_Use_Dice_4x_and_10x()
        {
            var u1 = Utility("Electric");
            var u2 = Utility("Water");
            ownership.SetOwner(u1, owner);

            int bt = tenant.GetMoney();
            int bo = owner.GetMoney();
            rentManager.TryChargeRent(tenant, u1, 7); //7*4
            Assert.AreEqual(bt - 28, tenant.GetMoney());
            Assert.AreEqual(bo + 28, owner.GetMoney());

            ownership.SetOwner(u2, owner);
            bt = tenant.GetMoney(); bo = owner.GetMoney();
            rentManager.TryChargeRent(tenant, u1, 7); //owns both
            Assert.AreEqual(bt - 70, tenant.GetMoney());
            Assert.AreEqual(bo + 70, owner.GetMoney());
        }

        [Test]
        public void Mortgaged_Is_Zero()
        {
            var prop = Street("Mort", ColorGroup.Red, baseRent: 20, houses: 0, mortgaged: true);
            ownership.SetOwner(prop, owner);

            int bt = tenant.GetMoney();
            int bo = owner.GetMoney();
            rentManager.TryChargeRent(tenant, prop, 0);

            Assert.AreEqual(bt, tenant.GetMoney());
            Assert.AreEqual(bo, owner.GetMoney());
        }

        [Test]
        public void SelfOwned_NoCharge()
        {
            var prop = Street("Self", ColorGroup.Green, baseRent: 30);
            ownership.SetOwner(prop, tenant); //tenant owns it

            int bt = tenant.GetMoney();
            rentManager.TryChargeRent(tenant, prop, 0);
            Assert.AreEqual(bt, tenant.GetMoney());
        }
    }
}
