using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poker.Lib.Arxivar.Models
{
    public class Contact
    {
        public int id;
        public string firstName;
        public string lastName;
        public string taxId;
        public string birthDay;
        public string phone;
        public string email;

        public string fullName
        {
            get
            {
                return string.Format("{0} {1}", firstName, lastName);
            }
        }

        public string nameId
        {
            get
            {
                return string.Format("{0} - {1}", fullName, taxId);
            }
        }
    }
}
