using Newtonsoft.Json;
using System.Collections.Generic;

namespace Poker.Lib.Arxivar.Models
{
    public class SignatureList
    {
        /*
        public Dictionary<string, SignatureOptions> empty = new Dictionary<string, SignatureOptions>()
        {
            { "Signature1", new SignatureOptions {  } },
        };
        */

        [JsonProperty(PropertyName = "Firma")]
        public SignatureOptions Firma;

        [JsonProperty(PropertyName = "*def")]
        public Dictionary<string, object> def = new Dictionary<string, object>() { { "enabled", false } };

        [JsonProperty(PropertyName = "*add")]
        public Dictionary<string, object> add = new Dictionary<string, object>() { { "enabled", false } };

        [JsonProperty(PropertyName = "*inv")]
        public Dictionary<string, object> inv = new Dictionary<string, object>() { { "enabled", false } };

        public SignatureList(int id)
        {
            Firma = new SignatureOptions(id);
        }
    }
}
