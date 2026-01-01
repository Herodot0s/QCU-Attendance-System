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
using QcuAttendanceSystem.Views.Login;
using QcuAttendanceSystem.Core; // Required for Session and Database
using Microsoft.Data.SqlClient; // Required for SQL
using QcuAttendanceSystem.Views.Admin;
namespace QcuAttendanceSystem.Views.Professor
{
    public partial class ProfMainLayout : Window
    {
        public ProfMainLayout()
        {
            InitializeComponent();

            // Load the dashboard initially
            MainFrame.Navigate(new ProfDashboard());
            PageTitleText.Text = "Dashboard Overview";

            // Load the user's name in the top right
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            try
            {
                // 1. Get name from Session
                string fullName = Session.CurrentFullName;

                // 2. If Session is empty but we have an ID, fetch from DB (Safety check)
                if (string.IsNullOrEmpty(fullName) && Session.CurrentUserID > 0)
                {
                    string query = "SELECT FullName FROM Users WHERE UserID = @ID";
                    SqlParameter[] p = { new SqlParameter("@ID", Session.CurrentUserID) };
                    object result = Database.ExecuteScalar(query, p);

                    if (result != null)
                    {
                        fullName = result.ToString();
                        Session.CurrentFullName = fullName; // Update Session for next time
                    }
                }

                // 3. Update the UI
                if (!string.IsNullOrEmpty(fullName))
                {
                    txtProfName.Text = "Prof. " + fullName;

                    // Generate Initials (e.g. "Juan Dela Cruz" -> "JC")
                    string[] parts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string initials = "";

                    if (parts.Length > 0)
                    {
                        // First Letter of First Name
                        initials += parts[0][0];

                        // First Letter of Last Name (if exists)
                        if (parts.Length > 1)
                        {
                            initials += parts[parts.Length - 1][0];
                        }
                    }

                    txtProfInitials.Text = initials.ToUpper();
                }
                else
                {
                    txtProfName.Text = "Guest Professor";
                    txtProfInitials.Text = "?";
                }
            }
            catch (Exception ex)
            {
                // Fallback in case of error
                Console.WriteLine("Profile Load Error: " + ex.Message);
                txtProfName.Text = "Professor";
            }
        }

        private void Nav_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfDashboard());
            PageTitleText.Text = "Dashboard Overview";
        }

        private void Nav_Classes_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClassSelection());
            PageTitleText.Text = "Manage Classes";
        }

        private void Nav_Reports_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AttendanceReport());
            PageTitleText.Text = "Attendance Reports";
        }

        private void Nav_Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear Session
                Session.Logout();

                // Open Login Window
                LoginWindow login = new LoginWindow();
                login.Show();

                // Close this current window
                this.Close();
            }
        }

        //user management
        private void Nav_Users_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the UserManagement Page
            MainFrame.Navigate(new UserManagement());

            // Update the Title Text
            PageTitleText.Text = "User Management";
        }
    }
}