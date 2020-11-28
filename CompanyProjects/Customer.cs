using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyProjects
{
    class Customer
    {
        public long id;
        public string name;
        public string address;
        public string phone;
        public string email;

        public override string ToString()
        {
            return name;
        }
    }
}
