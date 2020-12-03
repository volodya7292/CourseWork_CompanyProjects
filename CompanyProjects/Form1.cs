using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CompanyProjects
{
    public partial class Form1 : Form
    {
        MySqlConnection conn0, conn1, conn2;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string connStr = "Server=localhost; Database=company_projects; port=3306; User Id=root; password=admin;";
            conn0 = new MySqlConnection(connStr);
            conn1 = new MySqlConnection(connStr);
            conn2 = new MySqlConnection(connStr);
            try
            {
                conn0.Open();
                conn1.Open();
                conn2.Open();
            }
            catch (Exception)
            {
                MessageBox.Show(e.ToString(), "Cannot connect to database.");
            }

            Position[] positions = QueryPositions();
            positionCB1.Items.AddRange(positions);
            positionCB1.SelectedIndex = 0;

            updateTeams();
            updateTeamLeads();
            updateProjectManagers();
            updateCustomers();
            updateEngineers();
        }

        private MySqlCommand CreateCommand(MySqlConnection conn, string sql)
        {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = sql;
            return command;
        }

        private void updateTeamUnassignedEngineers()
        {
            if (teamCB1.SelectedIndex != -1)
            {
                Employee[] engineers = QueryUnassignedEngineers(((QATeam)teamCB1.SelectedItem).name);

                testerCB1.Items.Clear();
                testerCB1.Items.AddRange(engineers);
                if (engineers.Length > 0)
                    testerCB1.SelectedIndex = 0;
            }
        }

        private void updateEngineers()
        {
            Employee[] engineers = QueryEngineers();

            testersLV5.Items.Clear();
            foreach (Employee employee in engineers)
            {
                ListViewItem item = new ListViewItem(employee.name);
                item.Tag = employee;
                item.SubItems.Add(employee.position.name);
                testersLV5.Items.Add(item);
            }
            if (engineers.Length > 0)
                testerCB1.SelectedIndex = 0;

        }

        private void updateTeamLeads()
        {
            Employee[] teamLeads = QueryTeamLeads();

            teamLeadCB1.Items.Clear();
            teamLeadCB1.Items.AddRange(teamLeads);
            if (teamLeads.Length > 0)
                teamLeadCB1.SelectedIndex = 0;
        }

        private void updateProjectManagers()
        {
            Employee[] managers = QueryProjectManagers();

            managerCB3.Items.Clear();
            managerCB3.Items.AddRange(managers);
            if (managers.Length > 0)
                managerCB3.SelectedIndex = 0;
        }

        private void updateTeams()
        {
            QATeam[] teams = QueryQATeams();

            teamCB1.Items.Clear();
            teamCB1.Items.AddRange(teams);
            if (teams.Length > 0)
                teamCB1.SelectedIndex = 0;

            teamCB3.Items.Clear();
            teamCB3.Items.AddRange(teams);
            if (teams.Length > 0)
                teamCB3.SelectedIndex = 0;
        }

        private void updateCustomers()
        {
            Customer[] customers = QueryCustomers();

            customerCB3.Items.Clear();
            customerCB3.Items.AddRange(customers);
            if (customers.Length > 0)
                customerCB3.SelectedIndex = 0;

            customerCB4.Items.Clear();
            customerCB4.Items.AddRange(customers);
            if (customers.Length > 0)
                customerCB4.SelectedIndex = 0;
        }

        private void updateProjects()
        {
            Project[] projects = QueryCustomerProjects(((Customer)customerCB4.SelectedItem).id);

            projectsLV4.Items.Clear();

            foreach (Project project in projects)
            {
                ListViewItem item = new ListViewItem(project.name);
                item.SubItems.Add(project.price.ToString());
                item.SubItems.Add(project.term_start.ToString());
                item.SubItems.Add(project.term_end.ToString());
                item.SubItems.Add(project.manager.name);
                item.SubItems.Add(project.qa_team);
                item.Tag = project;
                projectsLV4.Items.Add(item);
            }
        }


        private Position[] QueryPositions()
        {
            List<Position> positions = new List<Position>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT * FROM position").ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    Position position = new Position
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name"))
                    };
                    positions.Add(position);
                }
            }

            return positions.ToArray();
        }

        private Position QueryPosition(long id)
        {
            Position position = null;

            using (DbDataReader reader = CreateCommand(conn2, "SELECT * FROM position WHERE position.id = " + id).ExecuteReader())
            {
                if (reader.HasRows && reader.Read())
                {
                    position = new Position
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name"))
                    };
                }
            }

            return position;
        }

        private QATeam[] QueryQATeams()
        {
            List<QATeam> teams = new List<QATeam>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT * FROM qa_team").ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    QATeam team = new QATeam
                    {
                        name = reader.GetString(reader.GetOrdinal("name")),
                        team_lead = QueryEmployee(reader.GetInt64(reader.GetOrdinal("team_lead")))
                    };

                    List<Employee> engineers = new List<Employee>();

                    using (DbDataReader reader2 = CreateCommand(conn1, "SELECT employee.* FROM qa_team_engineer, employee " +
                        "WHERE employee.id = qa_team_engineer.qa_engineer AND qa_team_engineer.qa_team = '" + team.name + "'").ExecuteReader())
                    {
                        while (reader2.HasRows && reader2.Read())
                        {
                            Employee engineer = new Employee
                            {
                                id = reader2.GetInt64(reader2.GetOrdinal("id")),
                                name = reader2.GetString(reader2.GetOrdinal("name")),
                                phone = reader2.GetString(reader2.GetOrdinal("phone")),
                                email = reader2.GetString(reader2.GetOrdinal("email")),
                                position = QueryPosition(reader2.GetInt64(reader2.GetOrdinal("position")))
                            };
                            engineers.Add(engineer);
                        }
                    }

                    team.engineers = engineers.ToArray();
                    teams.Add(team);
                }
            }

            return teams.ToArray();
        }

        private QATeam QueryQATeam(string name)
        {
            QATeam team = null;

            using (DbDataReader reader = CreateCommand(conn0, "SELECT * FROM qa_team WHERE qa_team.name = '" + name + "'").ExecuteReader())
            {
                if (reader.HasRows && reader.Read())
                {
                    team = new QATeam
                    {
                        name = reader.GetString(reader.GetOrdinal("name")),
                        team_lead = QueryEmployee(reader.GetInt64(reader.GetOrdinal("team_lead")))
                    };

                    List<Employee> engineers = new List<Employee>();

                    using (DbDataReader reader2 = CreateCommand(conn1, "SELECT employee.* FROM qa_team_engineer, employee " +
                        "WHERE employee.id = qa_team_engineer.qa_engineer AND qa_team_engineer.qa_team = '" + team.name + "'").ExecuteReader())
                    {
                        while (reader2.HasRows && reader2.Read())
                        {
                            Employee engineer = new Employee
                            {
                                id = reader2.GetInt64(reader2.GetOrdinal("id")),
                                name = reader2.GetString(reader2.GetOrdinal("name")),
                                phone = reader2.GetString(reader2.GetOrdinal("phone")),
                                email = reader2.GetString(reader2.GetOrdinal("email")),
                                position = QueryPosition(reader2.GetInt64(reader2.GetOrdinal("position")))
                            };
                            engineers.Add(engineer);
                        }
                    }

                    team.engineers = engineers.ToArray();
                }
            }

            return team;
        }

        private Employee QueryEmployee(long id)
        {
            Employee employee = null;

            using (DbDataReader reader = CreateCommand(conn1, "SELECT * FROM employee WHERE employee.id = " + id).ExecuteReader())
            {
                if (reader.HasRows && reader.Read())
                {
                    employee = new Employee
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        phone = reader.GetString(reader.GetOrdinal("phone")),
                        email = reader.GetString(reader.GetOrdinal("email")),
                        position = QueryPosition(reader.GetInt64(reader.GetOrdinal("position")))
                    };
                }
            }

            return employee;
        }

        private Employee[] QueryEmployees(long position_id)
        {
            List<Employee> employees = new List<Employee>();

            using (DbDataReader reader = CreateCommand(conn1, "SELECT * FROM employee WHERE employee.position = " + position_id).ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    Employee employee = new Employee
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        phone = reader.GetString(reader.GetOrdinal("phone")),
                        email = reader.GetString(reader.GetOrdinal("email")),
                        position = QueryPosition(reader.GetInt64(reader.GetOrdinal("position")))
                    };
                    employees.Add(employee);
                }
            }

            return employees.ToArray();
        }

        private Employee[] QueryTeamLeads()
        {
            Employee[] team_leads = QueryEmployees(2);
            Employee[] project_managers = QueryProjectManagers();
            Employee[] combined = new Employee[team_leads.Length + project_managers.Length];

            Array.Copy(team_leads, 0, combined, 0, team_leads.Length);
            Array.Copy(project_managers, 0, combined, team_leads.Length, project_managers.Length);

            return combined;
        }

        private Employee[] QueryEngineers()
        {
            Employee[] team_leads = QueryTeamLeads();
            Employee[] engineers = QueryEmployees(1);
            Employee[] combined = new Employee[team_leads.Length + engineers.Length];

            Array.Copy(engineers, 0, combined, 0, engineers.Length);
            Array.Copy(team_leads, 0, combined, engineers.Length, team_leads.Length);

            return combined;
        }

        private Employee[] QueryUnassignedEngineers(string qa_team)
        {
            List<Employee> employees = new List<Employee>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT employee.* FROM employee WHERE " +
                "(SELECT COUNT(1) FROM qa_team_engineer WHERE qa_team_engineer.qa_engineer = employee.id " +
                "AND qa_team_engineer.qa_team = '" + qa_team + "') = 0").ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    Employee employee = new Employee
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        phone = reader.GetString(reader.GetOrdinal("phone")),
                        email = reader.GetString(reader.GetOrdinal("email")),
                        position = QueryPosition(reader.GetInt64(reader.GetOrdinal("position")))
                    };
                    employees.Add(employee);
                }
            }

            return employees.ToArray();
        }

        private Customer[] QueryCustomers()
        {
            List<Customer> customers = new List<Customer>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT * FROM customer").ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    Customer customer = new Customer
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        address = reader.GetString(reader.GetOrdinal("address")),
                        phone = reader.GetString(reader.GetOrdinal("phone")),
                        email = reader.GetString(reader.GetOrdinal("email")),
                    };
                    customers.Add(customer);
                }
            }

            return customers.ToArray();
        }

        private Project[] QueryCustomerProjects(long customer_id)
        {
            List<Project> projects = new List<Project>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT * FROM project WHERE project.customer = " + customer_id).ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    Project project = new Project
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        price = reader.GetDecimal(reader.GetOrdinal("price")),
                        term_start = reader.GetString(reader.GetOrdinal("term_start")),
                        term_end = reader.GetString(reader.GetOrdinal("term_end")),
                        info = reader.GetString(reader.GetOrdinal("info")),
                        manager = QueryEmployee(reader.GetInt64(reader.GetOrdinal("manager"))),
                        qa_team = reader.GetString(reader.GetOrdinal("qa_team"))
                    };
                    projects.Add(project);
                }
            }

            return projects.ToArray();
        }

        private Employee[] QueryProjectEngineers(long project_id)
        {
            List<Employee> engineers = new List<Employee>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT employee.* FROM employee, project, qa_team, qa_team_engineer " +
                "WHERE project.qa_team = qa_team.name AND qa_team_engineer.qa_team = qa_team.name AND employee.id = qa_team_engineer.qa_engineer " +
                "AND qa_team.name = project.qa_team AND project.id = " + project_id).ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    Employee employee = new Employee
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        phone = reader.GetString(reader.GetOrdinal("phone")),
                        email = reader.GetString(reader.GetOrdinal("email")),
                        position = QueryPosition(reader.GetInt64(reader.GetOrdinal("position")))
                    };
                    engineers.Add(employee);
                }
            }

            return engineers.ToArray();
        }

        private Project[] QueryEngineerProjects(long employee_id)
        {
            List<Project> projects = new List<Project>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT project.* FROM project, qa_team, qa_team_engineer WHERE " +
                "project.qa_team = qa_team.name AND qa_team_engineer.qa_team = qa_team.name AND qa_team_engineer.qa_engineer = " + employee_id).ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    Project project = new Project
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        price = reader.GetDecimal(reader.GetOrdinal("price")),
                        term_start = reader.GetString(reader.GetOrdinal("term_start")),
                        term_end = reader.GetString(reader.GetOrdinal("term_end")),
                        info = reader.GetString(reader.GetOrdinal("info")),
                        manager = QueryEmployee(reader.GetInt64(reader.GetOrdinal("manager"))),
                        qa_team = reader.GetString(reader.GetOrdinal("qa_team"))
                    };
                    projects.Add(project);
                }
            }

            return projects.ToArray();
        }

        private Project[] QueryTeamLeadProjects(long employee_id)
        {
            List<Project> projects = new List<Project>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT project.* FROM employee, project, qa_team WHERE " +
                "qa_team.team_lead = employee.id AND project.qa_team = qa_team.name AND employee.id = " + employee_id).ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    Project project = new Project
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        price = reader.GetDecimal(reader.GetOrdinal("price")),
                        term_start = reader.GetString(reader.GetOrdinal("term_start")),
                        term_end = reader.GetString(reader.GetOrdinal("term_end")),
                        info = reader.GetString(reader.GetOrdinal("info")),
                        manager = QueryEmployee(reader.GetInt64(reader.GetOrdinal("manager"))),
                        qa_team = reader.GetString(reader.GetOrdinal("qa_team"))
                    };
                    projects.Add(project);
                }
            }

            return projects.ToArray();
        }

        private Project[] QueryManagerProjects(long employee_id)
        {
            List<Project> projects = new List<Project>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT project.* FROM employee, project WHERE " +
                "employee.id = project.manager AND project.manager = " + employee_id).ExecuteReader())
            {
                while (reader.HasRows && reader.Read())
                {
                    Project project = new Project
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        price = reader.GetDecimal(reader.GetOrdinal("price")),
                        term_start = reader.GetString(reader.GetOrdinal("term_start")),
                        term_end = reader.GetString(reader.GetOrdinal("term_end")),
                        info = reader.GetString(reader.GetOrdinal("info")),
                        manager = QueryEmployee(reader.GetInt64(reader.GetOrdinal("manager"))),
                        qa_team = reader.GetString(reader.GetOrdinal("qa_team"))
                    };
                    projects.Add(project);
                }
            }

            return projects.ToArray();
        }

        private Employee[] QueryProjectManagers()
        {
            return QueryEmployees(3);
        }

        private void teamCB1_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateTeamUnassignedEngineers();
        }

        private void customerCB4_SelectedIndexChanged(object sender, EventArgs e)
        {
            testersLV4.Items.Clear();
            updateProjects();
        }

        private void updateWorkersLV4()
        {
            testersLV4.Items.Clear();

            if (projectsLV4.SelectedItems.Count > 0)
            {
                Project project = (Project)projectsLV4.SelectedItems[0].Tag;
                Employee[] employees = QueryProjectEngineers(project.id);

                testersLV4.Items.Clear();

                foreach (Employee employee in employees)
                {
                    ListViewItem item = new ListViewItem(employee.name);
                    item.SubItems.Add(employee.position.name);
                    testersLV4.Items.Add(item);
                }

                Employee team_lead = QueryQATeam(project.qa_team).team_lead;
                ListViewItem item1 = new ListViewItem(team_lead.name);
                item1.SubItems.Add("Керiвник команди");
                testersLV4.Items.Add(item1);

                Employee manager = project.manager;
                ListViewItem item2 = new ListViewItem(manager.name);
                item2.SubItems.Add("Менеджер проекту");
                testersLV4.Items.Add(item2);
            }
        }

        private void projectsLV4_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateWorkersLV4();
        }

        private void updateWorkersLV5()
        {
            projectsLV5.Items.Clear();

            if (testersLV5.SelectedItems.Count > 0)
            {
                long employee_id = ((Employee)testersLV5.SelectedItems[0].Tag).id;

                foreach (Project project in QueryEngineerProjects(employee_id))
                {
                    ListViewItem item = new ListViewItem(project.name);
                    item.SubItems.Add(project.price.ToString());
                    item.SubItems.Add(project.term_start.ToString());
                    item.SubItems.Add(project.term_end.ToString());
                    item.SubItems.Add("Тестувальник");
                    item.SubItems.Add(project.qa_team);
                    projectsLV5.Items.Add(item);
                }
                foreach (Project project in QueryTeamLeadProjects(employee_id))
                {
                    ListViewItem item = new ListViewItem(project.name);
                    item.SubItems.Add(project.price.ToString());
                    item.SubItems.Add(project.term_start.ToString());
                    item.SubItems.Add(project.term_end.ToString());
                    item.SubItems.Add("Керiвник команди");
                    item.SubItems.Add(project.qa_team);
                    projectsLV5.Items.Add(item);
                }
                foreach (Project project in QueryManagerProjects(employee_id))
                {
                    ListViewItem item = new ListViewItem(project.name);
                    item.SubItems.Add(project.price.ToString());
                    item.SubItems.Add(project.term_start.ToString());
                    item.SubItems.Add(project.term_end.ToString());
                    item.SubItems.Add("Менеджер проекту");
                    item.SubItems.Add(project.qa_team);
                    projectsLV5.Items.Add(item);
                }
            }
        }

        private void testersLV5_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateWorkersLV5();
        }

        private void createEmployeeB_Click(object sender, EventArgs e)
        {
            string name = nameTB1.Text;
            string phone = phoneTB1.Text;
            string email = emailTB1.Text;
            long position_id = ((Position)positionCB1.SelectedItem).id;

            CreateCommand(conn0, "INSERT INTO employee (name, phone, email, position) VALUES(" +
                "'" + name + "','" + phone + "','" + email + "'," + position_id + ")").ExecuteNonQuery();

            nameTB1.Clear();
            phoneTB1.Clear();
            emailTB1.Clear();

            updateEngineers();
            updateTeamUnassignedEngineers();
            updateTeamLeads();
            updateProjectManagers();
        }

        private void createCustomerB_Click(object sender, EventArgs e)
        {
            string name = nameTB2.Text;
            string address = addressTB2.Text;
            string phone = phoneTB2.Text;
            string email = emailTB2.Text;

            CreateCommand(conn0, "INSERT INTO customer (name, address, phone, email) VALUES(" +
                "'" + name + "','" + address + "','" + phone + "','" + email + "')").ExecuteNonQuery();

            nameTB2.Clear();
            addressTB2.Clear();
            phoneTB2.Clear();
            emailTB2.Clear();

            updateCustomers();
        }

        private void createTeamB_Click(object sender, EventArgs e)
        {
            string name = teamNameTB1.Text;
            CreateCommand(conn0, "INSERT INTO qa_team (name, team_lead) VALUES('" + name + "'," + ((Employee)teamLeadCB1.SelectedItem).id + ")").ExecuteNonQuery();
            updateTeams();
        }

        private void createProjectB_Click(object sender, EventArgs e)
        {
            string name = nameTB3.Text;
            long customer_id = ((Customer)customerCB3.SelectedItem).id;
            decimal price = priceN3.Value;
            string term_start = termStartD3.Value.ToString("yyyy-MM-dd");
            string term_end = termEndD3.Value.ToString("yyyy-MM-dd");
            string add_info = addInfoTB3.Text;
            long manager_id = ((Employee)managerCB3.SelectedItem).id;
            string qa_team = ((QATeam)teamCB3.SelectedItem).name;

            CreateCommand(conn0, "INSERT INTO project (name, customer, price, term_start, term_end, info, manager, qa_team) VALUES(" +
                "'" + name + "'," + customer_id + "," + price + ",'" + term_start + "','" + term_end + "','" + add_info + "'," + manager_id + ",'" + qa_team + "')").ExecuteNonQuery();

            nameTB3.Clear();
            priceN3.Value = 1.0M;
            addInfoTB3.Clear();

            updateProjects();
            updateWorkersLV5();
        }

        private void addTesterToTeamB_Click(object sender, EventArgs e)
        {
            string team = ((QATeam)teamCB1.SelectedItem).name;
            long engineer_id = ((Employee)testerCB1.SelectedItem).id;

            CreateCommand(conn0, "INSERT INTO qa_team_engineer (qa_team, qa_engineer) VALUES('" + team + "'," + engineer_id + ")").ExecuteNonQuery();

            updateTeamUnassignedEngineers();
            updateWorkersLV4();
        }
    }
}
