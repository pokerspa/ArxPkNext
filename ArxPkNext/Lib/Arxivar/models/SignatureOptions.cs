using Poker.Lib.Arxivar.Services;

namespace Poker.Lib.Arxivar.Models
{
    public class SignatureOptions
    {
        public bool enabled = true;
        // public decimal[] create = new decimal[] { 0, 126, 265.5, 195, 275.5 };
        public int[] create = new int[] { 0, 126, 266, 196, 276 };
        public string simple = "";
        public bool sms = true;

        public SignatureOptions(int id)
        {
            ProfileService profileService = new ProfileService();
            var profile = profileService.GetProfileById(id);
            this.simple = profile.MITTENTE;
            // this.sms = profile.TEL // Dioporco non c'è :facepalm:
        }
    }
}
