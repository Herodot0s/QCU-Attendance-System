using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace QcuAttendanceSystem.Core
{
    public class Database
    {
        // === CONNECTION CONFIGURATION ===
        private static string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=QcuAttendanceDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30";

        /// <summary>
        /// Returns a new Connection object. Always use inside a 'using' block.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Test the connection to ensure the Database exists and is accessible.
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Connection Failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Executes a SELECT query and returns a DataTable (Great for populating DataGrids).
        /// </summary>
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Query Error:\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
        }

        /// <summary>
        /// Executes INSERT, UPDATE, DELETE statements. Returns the number of rows affected.
        /// </summary>
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        return cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Execution Error:\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return -1;
                }
            }
        }

        /// <summary>
        /// Executes a query and returns the first column of the first row (e.g., getting a User ID or Count).
        /// </summary>
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        return cmd.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Scalar Error:\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
        }
    }
}