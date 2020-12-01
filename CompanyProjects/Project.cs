using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyProjects
{
    class Project
    {
        public long id;
        public string name;
        public decimal price;
        public string term_start;
        public string term_end;
        public string info;
        public Employee manager;
        public string qa_team;
    }
}
