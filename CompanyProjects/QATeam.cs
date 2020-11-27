using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyProjects
{
    class QATeam
    {
        public long id;
        public Employee team_lead;
        public Employee[] engineers;

        public override string ToString()
        {
            return id.ToString();
        }
    }
}
