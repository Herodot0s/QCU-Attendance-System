using QcuAttendanceSystem.Views.Login;
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
using QcuAttendanceSystem.Core; // Import Core for Session and Database
using QcuAttendanceSystem.Views.Login;
using System.Windows;
using QcuAttendanceSystem.Views.Professor;


namespace QcuAttendanceSystem.Views.Student
{
    public partial class StudentMainLayout : Window
    {
        public StudentMainLayout()
        {
            InitializeComponent();

            // Navigate to default page
            StudentFrame.Navigate(new MySchedule());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStudentData();
        }

        private void LoadStudentData()
        {

            if (Session.IsLoggedIn())
            {
                TxtStudentName.Text = Session.CurrentFullName;

                // NEW: Format the ID line as "21-00123 • BSCS 4A"
                string courseInfo = "";
                if (!string.IsNullOrEmpty(Session.CurrentCourse) || !string.IsNullOrEmpty(Session.CurrentSection))
                {
                    courseInfo = $" • {Session.CurrentCourse} {Session.CurrentSection}";
                }

                // Result: "21-00123 • BSCS 4A"
                TxtStudentID.Text = $"{Session.CurrentSchoolID}{courseInfo}";
                // Initials logic...
                if (!string.IsNullOrEmpty(Session.CurrentFullName))
                {
                    var parts = Session.CurrentFullName.Split(' ');
                    string initials = "";
                    if (parts.Length >= 1) initials += parts[0][0];
                    if (parts.Length >= 2) initials += parts[parts.Length - 1][0];
                    TxtInitials.Text = initials.ToUpper();
                }
            }
            else
            {
                TxtStudentName.Text = "Guest Student";
                TxtStudentID.Text = "00-00000";
                TxtInitials.Text = "GS";
            }
        }

        private void Nav_Enrollment_Click(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "Subject Enrollment";
            StudentFrame.Navigate(new EnrollmentPage());
        }

        private void Nav_Schedule_Click(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "My Class Schedule";
            StudentFrame.Navigate(new MySchedule());
        }

        private void Nav_History_Click(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "Attendance History";
            StudentFrame.Navigate(new StudentHistory()); 
        }

        private void Nav_Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear Session Data
                Session.Logout();

                // Return to Login Screen
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }
        }

    }
}