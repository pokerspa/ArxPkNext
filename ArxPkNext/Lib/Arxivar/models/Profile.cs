using System;

namespace Poker.Lib.Arxivar.Models
{
    public class Profile
    {
        public int id;
        public string name;
        public string filename;
        public DateTime createdAt;
        public string path;
        public string hash;
        public Contact contact;

        public Profile()
        {

        }

        public Profile(object[] e)
        {
            id = Decimal.ToInt32((decimal)e[0]);
            name = (string)e[1];
            filename = (string)e[2];
            createdAt = (DateTime)e[3];
            path = (string)e[4];
            hash = (string)e[5];
        }
    }
}
