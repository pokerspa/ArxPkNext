using Abletech.Arxivar.Entities.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poker.Lib.FormData
{
    public class BipFormDataContact
    {
        public static string ComposeValue(Dictionary<string, string> data, string prefix, string format)
        {
            List<string> result = new List<string> { };
            List<string> selectors = format.Split(',').ToList();

            foreach (string selector in selectors)
            {
                try
                {
                    string val = data[prefix + selector];
                    result.Add(val);
                }
                catch (Exception)
                {

                }
            }

            if (result.Count == 0) return null;

            return String.Join(" ", result.ToArray());
        }

        public static bool CheckFormDataValidity(Dictionary<string, string> data, string prefix)
        {
            List<string> fields = new List<string> {
                "first_name",
                "last_name",
                "gender",
                "birth_day",
            };

            foreach (string field in fields)
            {
                string val = data[prefix + field];
                if (val == null || val.Trim() == string.Empty)
                {
                    return false;
                }
            }

            return true;
        }

        public static Dm_Contatti_ForInsert FillFromFormData(Dictionary<string, string> data, string prefix, string role)
        {
            Dictionary<string, string> mapping = new Dictionary<string, string>
            {
                { "CONTATTO", "first_name,last_name" },
                { "EMAIL", "email" },
                { "TEL", "phone" },
            };

            Dm_Contatti_ForInsert contact = new Dm_Contatti_ForInsert
            {
                MANSIONE = role,
            };

            foreach (KeyValuePair<string, string> entry in mapping)
            {
                string val = ComposeValue(data, prefix, entry.Value);

                if (!String.IsNullOrEmpty(val))
                {
                    contact.GetType().GetProperty(entry.Key).SetValue(contact, val, null);
                }
            }

            return contact;
        }
    }
}
