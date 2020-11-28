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

            using (DbDataReader reader = CreateCommand(conn0, "SELECT id, name FROM position").ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(reader.GetOrdinal("id"));
                        string name = reader.GetString(reader.GetOrdinal("name"));

                        Console.WriteLine(id + " " + name);
                    }
                }
            }
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
                Employee[] engineers = QueryUnassignedEngineers(((QATeam)teamCB1.SelectedItem).id);

                testerCB1.Items.Clear();
                testerCB1.Items.AddRange(engineers);
                if (engineers.Length > 0)
                    testerCB1.SelectedIndex = 0;
            }
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
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        team_lead = QueryEmployee(reader.GetInt64(reader.GetOrdinal("team_lead")))
                    };

                    List<Employee> engineers = new List<Employee>();

                    using (DbDataReader reader2 = CreateCommand(conn1, "SELECT employee.* FROM qa_team_engineer, employee " +
                        "WHERE employee.id = qa_team_engineer.qa_engineer AND qa_team_engineer.qa_team = " + team.id).ExecuteReader())
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
            Employee[] team_leads = QueryEmployees(1);
            Employee[] project_managers = QueryProjectManagers();
            Employee[] combined = new Employee[team_leads.Length + project_managers.Length];

            Array.Copy(team_leads, 0, combined, 0, team_leads.Length);
            Array.Copy(project_managers, 0, combined, team_leads.Length, project_managers.Length);

            return combined;
        }

        private Employee[] QueryEngineers()
        {
            Employee[] team_leads = QueryTeamLeads();
            Employee[] engineers = QueryEmployees(0);
            Employee[] combined = new Employee[team_leads.Length + engineers.Length];

            Array.Copy(engineers, 0, combined, 0, engineers.Length);
            Array.Copy(team_leads, 0, combined, engineers.Length, team_leads.Length);

            return combined;
        }

        private Employee[] QueryUnassignedEngineers(long qa_team_id)
        {
            List<Employee> employees = new List<Employee>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT employee.* FROM employee WHERE " +
                "(SELECT COUNT(1) FROM qa_team_engineer WHERE qa_team_engineer.qa_engineer = employee.id " +
                "AND qa_team_engineer.qa_team = " + qa_team_id + ") = 0").ExecuteReader())
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
                if (reader.HasRows && reader.Read())
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
                if (reader.HasRows && reader.Read())
                {
                    Project project = new Project
                    {
                        id = reader.GetInt64(reader.GetOrdinal("id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        price = reader.GetDecimal(reader.GetOrdinal("price")),
                        term_start = reader.GetString(reader.GetOrdinal("term_start")),
                        term_end = reader.GetString(reader.GetOrdinal("term_end")),
                        manager = QueryEmployee(reader.GetInt64(reader.GetOrdinal("manager"))),
                        qa_team_id = reader.GetInt64(reader.GetOrdinal("qa_team"))
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
            Project[] projects = QueryCustomerProjects(((Customer)customerCB4.SelectedItem).id);

            foreach (Project project in projects) {
                ListViewItem item = new ListViewItem(project.name);
                item.SubItems.Add(project.price.ToString());
                item.SubItems.Add(project.term_start.ToString());
                item.SubItems.Add(project.term_end.ToString());
                item.SubItems.Add(project.manager.name);
                item.SubItems.Add(project.qa_team_id.ToString());
                projectsLV4.Items.Add(item);
            }
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
            phoneTB2.Clear();
            emailTB2.Clear();
            emailTB2.Clear();

            updateCustomers();
        }

        private void createTeamB_Click(object sender, EventArgs e)
        {
            CreateCommand(conn0, "INSERT INTO qa_team (team_lead) VALUES(" + ((Employee)teamLeadCB1.SelectedItem).id + ")").ExecuteNonQuery();
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
            long qa_team_id = ((QATeam)teamCB3.SelectedItem).id;

            CreateCommand(conn0, "INSERT INTO project (name, customer, price, term_start, term_end, info, manager, qa_team) VALUES(" +
                "'" + name + "'," + customer_id + "," + price + ",'" + term_start + "','" + term_end + "','" + add_info + "'," + manager_id + "," + qa_team_id + ")").ExecuteNonQuery();

            nameTB3.Clear();
            priceN3.Value = 1.0M;
            addInfoTB3.Clear();
        }

        private void addTesterToTeamB_Click(object sender, EventArgs e)
        {
            long team_id = ((QATeam)teamCB1.SelectedItem).id;
            long engineer_id = ((Employee)testerCB1.SelectedItem).id;

            CreateCommand(conn0, "INSERT INTO qa_team_engineer (qa_team, qa_engineer) VALUES(" + team_id + "," + engineer_id + ")").ExecuteNonQuery();
            updateTeamUnassignedEngineers();
        }
    }
}
