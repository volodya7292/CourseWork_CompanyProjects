using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyProjects
{
    class Employee
    {
        public long id;
        public string name;
        public string phone;
        public string email;
        public Position position;

        public override string ToString()
        {
            return name;
        }
    }
}
