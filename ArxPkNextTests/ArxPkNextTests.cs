using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Poker.Arxivar.ServerPlugins;
using Poker.Lib.Arxivar.Services;
using Poker.Lib.FormData;
using Abletech.Arxivar.Entities;

namespace ArxPkNextTests
{
    [TestClass]
    public class ArxPkNextTests
    {
        [TestMethod]
        public void PluginIsConfigured()
        {
            var plugin = new NextRest();
            plugin.Initialize();
            Assert.AreEqual("ArxPkNext", plugin.Name);
        }

        [TestMethod]
        public void CreateContact()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("id", "PK123456789");
            data.Add("requestType", "issue");
            data.Add("last_name", "TEST");
            data.Add("first_name", "POKER");
            data.Add("birth_place_city", "Torino");
            data.Add("birth_place_province", "TO");
            data.Add("birth_place_country", "Italia");
            data.Add("gender", "M");
            data.Add("birth_day", "01/01/2000");
            data.Add("tax_id", "GBLDMT29T22F637F");
            data.Add("address", "Via, 0");
            data.Add("address_city", "Torino");
            data.Add("address_zip", "10100");
            data.Add("address_province", "TO");
            data.Add("phone", "800 800 8080");
            data.Add("email", "test@example.com");
            data.Add("payer_tax_id", "GBLDMT29T22F637F");
            data.Add("guardian_first_name", "GUARD");
            data.Add("guardian_last_name", "IAN");
            data.Add("guardian_phone", "011 801 1234");
            data.Add("guardian_email", "guardian@exammple.com");

            var cs = new ContactService();
            var c = cs.GetOrCreateEntry(data);

            Assert.IsNotNull(c);
            Assert.AreEqual("GBLDMT29T22F637F", c.CODFIS);
            Assert.AreEqual("800 800 8080", c.TEL);
        }

        [TestMethod]
        public void TestFromData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("id", "PK123456789");
            data.Add("requestType", "issue");
            data.Add("last_name", "TEST");
            data.Add("first_name", "POKER");
            data.Add("birth_place_city", "Torino");
            data.Add("birth_place_province", "TO");
            data.Add("birth_place_country", "Italia");
            data.Add("gender", "M");
            data.Add("birth_day", "01/01/2000");
            data.Add("tax_id", "GBLDMT29T22F637F");
            data.Add("address", "Via, 0");
            data.Add("address_city", "Torino");
            data.Add("address_zip", "10100");
            data.Add("address_province", "TO");
            data.Add("phone", "800 800 8080");
            data.Add("email", "test@example.com");
            data.Add("payer_tax_id", "GBLDMT29T22F637F");
            data.Add("guardian_first_name", "GUARD");
            data.Add("guardian_last_name", "IAN");
            data.Add("guardian_phone", "011 801 1234");
            data.Add("guardian_email", "guardian@exammple.com");

            var owner = BipFormDataContact.FillFromFormData(data, "", "Richiedente");
            Assert.AreEqual("POKER TEST", owner.CONTATTO);
            Assert.AreEqual("test@example.com", owner.EMAIL);
            Assert.AreEqual("800 800 8080", owner.TEL);
            Assert.AreEqual("Richiedente", owner.MANSIONE);

            var guardian = BipFormDataContact.FillFromFormData(data, "guardian_", "Genitore/Tutore");
            Assert.AreEqual("GUARD IAN", guardian.CONTATTO);
            Assert.AreEqual("guardian@exammple.com", guardian.EMAIL);
            Assert.AreEqual("011 801 1234", guardian.TEL);
            Assert.AreEqual("Genitore/Tutore", guardian.MANSIONE);
        }

        [TestMethod]
        public void GetsProperContact()
        {
            ProfileService ps = new ProfileService();
            Dm_Rubrica contact = ps.GetProfileById(4444);
            Assert.AreEqual("POKER TEST", contact.UCONTATTI);
            Assert.AreEqual("800 800 8080", contact.TEL);
        }
    }
}
