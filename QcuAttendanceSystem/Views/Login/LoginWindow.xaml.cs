using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QcuAttendanceSystem.Views.Professor;
using QcuAttendanceSystem.Views.Student;
using QcuAttendanceSystem.Views;
using QcuAttendanceSystem.Core;
using Microsoft.Data.SqlClient;

using System.Data; // Required for DataTable and DataRow
using Microsoft.Data.SqlClient; // <--- UPDATED: Using the modern SQL client
using System.Windows;
using System.Windows.Input;
using QcuAttendanceSystem.Core; // Required to access Database & Session
using QcuAttendanceSystem.Views.Professor;
using QcuAttendanceSystem.Views.Student;
using QcuAttendanceSystem.Views; // Required for KioskWindow

namespace QcuAttendanceSystem.Views.Login
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // 1. Handle Login Logic (Connected to Database)
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            // Basic Input Validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2. Prepare SQL Query
                string query = "SELECT * FROM Users WHERE Username = @user AND Password = @pass";

                // Using SqlParameter from Microsoft.Data.SqlClient
                SqlParameter[] parameters = {
                    new SqlParameter("@user", username),
                    new SqlParameter("@pass", password)
                };

                // 3. Execute Query
                DataTable result = Database.ExecuteQuery(query, parameters);

                // 4. Check Results
                if (result != null && result.Rows.Count > 0)
                {
                    // -- LOGIN SUCCESS --
                    DataRow row = result.Rows[0];

                    Session.CurrentUserID = Convert.ToInt32(row["UserID"]);
                    Session.CurrentUserName = row["Username"].ToString();
                    Session.CurrentFullName = row["FullName"].ToString();
                    Session.CurrentRole = row["Role"].ToString();
                    Session.CurrentSchoolID = row["SchoolID"].ToString();

                    // NEW: Load Course and Section (Handle nulls using DBNull checking)
                    Session.CurrentCourse = row["Course"] != DBNull.Value ? row["Course"].ToString() : "";
                    Session.CurrentSection = row["Section"] != DBNull.Value ? row["Section"].ToString() : "";

                    // Redirect based on Role (Keep existing logic below)
                    if (Session.CurrentRole == "Professor")
                    {
                        MessageBox.Show($"Welcome, Prof. {Session.CurrentFullName}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                        ProfMainLayout mainLayout = new ProfMainLayout();
                        mainLayout.Show();
                    }
                    else if (Session.CurrentRole == "Student")
                    {
                        MessageBox.Show($"Welcome, {Session.CurrentFullName}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                        StudentMainLayout mainLayout = new StudentMainLayout();
                        mainLayout.Show();
                    }
                    else
                    {
                        MessageBox.Show("Unknown Role. Please contact Admin.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    this.Close(); // Close the Login Window
                }
                else
                {
                    // -- LOGIN FAILED --
                    MessageBox.Show("Invalid Username or Password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Connection Error:\n" + ex.Message, "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- UI HELPERS ---

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Drag Window Feature
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Please contact the QCU MIS department to reset your password.");
        }

        private void GoToKiosk_Click(object sender, RoutedEventArgs e)
        {
            KioskWindow kiosk = new KioskWindow();
            kiosk.Show();
            this.Close();
        }
    }
}
