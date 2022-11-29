using Abletech.Arxivar.Entities;
using Poker.Lib.Arxivar.Services;
using System.Text.RegularExpressions;

namespace Poker.Lib.Arxivar.Models
{
    public class SignatureOptions
    {
        public bool enabled = true;
        // public decimal[] create = new decimal[] { 0, 126, 265.5, 195, 275.5 };
        public int[] create = new int[] { 0, 126, 266, 196, 276 };
        public string simple = "";
        public string sms = "";

        public SignatureOptions(int id)
        {
            ProfileService profileService = new ProfileService();
            Dm_Rubrica contact = profileService.GetProfileById(id);
            this.simple = contact.UCONTATTI;
            var phone = Regex.Replace(contact.TEL, @"[^\d]", "");
            this.sms = contact.TEL.StartsWith("+39") ? phone : string.Format("+39{0}", phone);
        }
    }
}
