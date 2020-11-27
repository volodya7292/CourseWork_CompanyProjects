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
            updateEngineers();
            updateTeamLeads();

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

        private void updateEngineers()
        {
            Employee[] engineers = QueryEngineers();

            testerCB1.Items.AddRange(engineers);
            if (engineers.Length > 0)
                testerCB1.SelectedIndex = 0;
            addTesterToTeamB.Enabled = engineers.Length > 0;
        }

        private void updateTeamLeads()
        {
            Employee[] teamLeads = QueryTeamLeads();

            teamLeadCB1.Items.AddRange(teamLeads);
            if (teamLeads.Length > 0)
                teamLeadCB1.SelectedIndex = 0;
            createTeamB.Enabled = teamLeads.Length > 0;
        }

        private void updateTeams()
        {
            QATeam[] teams = QueryQATeams();

            teamCB1.Items.AddRange(teams);
            if (teams.Length > 0)
                teamCB1.SelectedIndex = 0;
        }

        private Position[] QueryPositions()
        {
            List<Position> positions = new List<Position>();

            using (DbDataReader reader = CreateCommand(conn0, "SELECT * FROM position").ExecuteReader())
            {
                if (reader.HasRows && reader.Read())
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

        private Employee[] QueryProjectManagers()
        {
            return QueryEmployees(2);
        }

        private void createTeamB_Click(object sender, EventArgs e)
        {
            CreateCommand(conn0, "INSERT INTO qa_team (team_lead) VALUES(" + ((Employee)teamLeadCB1.SelectedItem).id + ")").ExecuteNonQuery();
            updateTeams();
        }

        private void addTesterToTeamB_Click(object sender, EventArgs e)
        {
            long team_id = ((QATeam)teamCB1.SelectedItem).id;
            long engineer_id = ((Employee)testerCB1.SelectedItem).id;

            CreateCommand(conn0, "INSERT INTO qa_team_engineer (qa_team, qa_engineer) VALUES(" + team_id + "," + engineer_id + ")").ExecuteNonQuery();
        }
    }
}
