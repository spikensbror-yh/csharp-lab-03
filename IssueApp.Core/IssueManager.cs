using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace IssueApp.Core
{
    public static class IssueManager
    {
        #region Properties

        public static SqlConnection Connection { get; set; }
        public static List<Issue> Issues { get; set; }

        #endregion

        #region Initialization/Deinitialization Methods

        public static void Initialize(string connection)
        {
            Connection = new SqlConnection(connection);
            Connection.Open();

            Reload();
        }

        public static void Deinitialize()
        {
            Connection.Close();
        }

        #endregion

        #region SQL Methods

        public static void Reload()
        {
            Issues = new List<Issue>();

            SqlCommand command = Connection.CreateCommand();
            command.CommandText = "SELECT * FROM [issues]";
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Issue issue = new Issue();
                    issue.Id = reader.GetInt32(0);

                    issue.Title = reader.GetString(reader.GetOrdinal("title"));
                    issue.Description = reader.GetString(reader.GetOrdinal("description"));
                    issue.Priority = (IssuePriority)reader.GetInt32(reader.GetOrdinal("priority"));

                    int actionOrdinal = reader.GetOrdinal("action");
                    int reportedAtOrdinal = reader.GetOrdinal("reported_at");
                    int completedAtOrdinal = reader.GetOrdinal("completed_at");

                    if (!reader.IsDBNull(actionOrdinal))
                        issue.Action = reader.GetString(actionOrdinal);

                    if (!reader.IsDBNull(reportedAtOrdinal))
                        issue.ReportedAt = reader.GetDateTime(reportedAtOrdinal);

                    if (!reader.IsDBNull(completedAtOrdinal))
                        issue.CompletedAt = reader.GetDateTime(completedAtOrdinal);

                    Issues.Add(issue);
                }
            }
        }

        public static void Insert(Issue issue)
        {
            SqlCommand command = Connection.CreateCommand();
            command.CommandText = "INSERT INTO [issues] VALUES(@Title, @Description, @Action, @ReportedAt, @CompletedAt, @Priority); " +
                                    "SELECT SCOPE_IDENTITY();";
            SetParametersForIssue(command, issue);
            issue.Id = Convert.ToInt32(command.ExecuteScalar());

            Issues.Add(issue);
        }

        public static void Update(Issue issue)
        {
            if (issue.Id < 1)
            {
                throw new Exception("The issue to update does not have a valid id!");
            }

            SqlCommand command = Connection.CreateCommand();
            command.CommandText = "UPDATE [issues] SET [title] = @Title, " +
                                    "[description] = @Description, " +
                                    "[action] = @Action, " +
                                    "[reported_at] = @ReportedAt, " +
                                    "[completed_at] = @CompletedAt, " +
                                    "[priority] = @Priority " +
                                    "WHERE [id] = @Id";
            SetParametersForIssue(command, issue);
            command.Parameters.AddWithValue("@Id", issue.Id);
            command.ExecuteNonQuery();
        }

        public static void Delete(Issue issue)
        {
            if (issue.Id < 1)
            {
                throw new Exception("The issue to delete does not have a valid id!");
            }

            SqlCommand command = Connection.CreateCommand();
            command.CommandText = "DELETE FROM [issues] WHERE [id] = @Id";
            command.Parameters.AddWithValue("@Id", issue.Id);
            command.ExecuteNonQuery();

            Issues.Remove(issue);
        }

        private static void SetParametersForIssue(SqlCommand command, Issue issue)
        {
            command.Parameters.AddWithValue("@Title", issue.Title);
            command.Parameters.AddWithValue("@Description", issue.Description);
            command.Parameters.AddWithValue("@Action", (object)issue.Action ?? DBNull.Value);
            command.Parameters.AddWithValue("@ReportedAt", (object)issue.ReportedAt ?? DBNull.Value);
            command.Parameters.AddWithValue("@CompletedAt", (object)issue.CompletedAt ?? DBNull.Value);
            command.Parameters.AddWithValue("@Priority", (int)issue.Priority);
        }

        #endregion

        #region Import/Export Methods

        public static void ImportFromCSV(string path)
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            foreach (string line in lines)
            {
                string[] data = line.Split(',');
                if (data.Length != 5)
                {
                    throw new Exception("Invalid data in the CSV file, halting import operation.");
                }

                Issue issue = new Issue();
                issue.Title = data[0];
                issue.Description = data[1];
                issue.ReportedAt = (data[2] == "NULL") ? null : (DateTime?)DateTime.Parse(data[2]);
                issue.CompletedAt = (data[3] == "NULL") ? null : (DateTime?)DateTime.Parse(data[3]);
                issue.Priority = (IssuePriority)Enum.Parse(typeof(IssuePriority), data[4]);

                Insert(issue);
            }
        }

        public static void ExportToCSV(string path)
        {
            string output = "";
            foreach (Issue issue in Issues)
            {
                string[] data = new string[] {
                    issue.Title,
                    issue.Description,
                    issue.ReportedAt.HasValue ? issue.ReportedAt.Value.ToString() : "NULL",
                    issue.CompletedAt.HasValue ? issue.CompletedAt.Value.ToString() : "NULL",
                    issue.Priority.ToString()
                };

                output += string.Join(",", data) + "\r\n";
            }

            File.WriteAllText(path, output);
        }

        #endregion
    }
}
