using Microsoft.Data.SqlClient; // Required for SQL Connection
using QcuAttendanceSystem.Core; // Access Database & Session
using System.Data; // Required for DataTable
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace QcuAttendanceSystem.Views.Professor
{
    public partial class ProfDashboard : Page
    {
        public ProfDashboard()
        {
            InitializeComponent();
            txtCurrentDate.Text = DateTime.Now.ToString("MMM dd, yyyy");

            LoadSchedule();
            LoadDashboardStats(); ;

        }

        private void LoadSchedule()
        {
            try
            {
                if (Session.CurrentUserID == 0)
                {
                    MessageBox.Show("Session Error: No user logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string query = "SELECT * FROM Classes WHERE ProfessorID = @ProfID";
                SqlParameter[] parameters = { new SqlParameter("@ProfID", Session.CurrentUserID) };

                DataTable result = Database.ExecuteQuery(query, parameters);
                List<ClassSchedule> myClasses = new List<ClassSchedule>();

                if (result != null)
                {
                    foreach (DataRow row in result.Rows)
                    {
                        string timeFormat = $"{row["Day"]} • {row["StartTime"]} - {row["EndTime"]}";

                        myClasses.Add(new ClassSchedule
                        {
                            ClassID = Convert.ToInt32(row["ClassID"]), // IMPORTANT: Get the ID
                            SubjectCode = row["SubjectCode"].ToString(),
                            Description = row["Description"].ToString(),
                            Section = row["Section"].ToString(),
                            Room = row["Room"].ToString(),
                            TimeRange = timeFormat
                        });
                    }
                }

                ScheduleGrid.ItemsSource = myClasses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading schedule:\n" + ex.Message);
            }
        }
        private void LoadDashboardStats()
        {
            try
            {
                int profId = Session.CurrentUserID;

                // 1. Total Classes
                object classCount = Database.ExecuteScalar("SELECT COUNT(*) FROM Classes WHERE ProfessorID = @ID",
                    new[] { new SqlParameter("@ID", profId) });
                txtTotalClasses.Text = classCount?.ToString() ?? "0";

                // 2. Total Students (Distinct students enrolled in my classes)
                string studentQuery = @"
                    SELECT COUNT(DISTINCT e.StudentID) 
                    FROM Enrollments e
                    JOIN Classes c ON e.ClassID = c.ClassID
                    WHERE c.ProfessorID = @ID";

                object studentCount = Database.ExecuteScalar(studentQuery, new[] { new SqlParameter("@ID", profId) });
                txtTotalStudents.Text = studentCount?.ToString() ?? "0";

                // 3. Average Attendance %
                string avgQuery = @"
                    SELECT 
                        (CAST(SUM(CASE WHEN a.Status IN ('Present', 'Late') THEN 1 ELSE 0 END) AS FLOAT) 
                         / NULLIF(COUNT(*), 0)) * 100
                    FROM Attendance a
                    JOIN Enrollments e ON a.EnrollmentID = e.EnrollmentID
                    JOIN Classes c ON e.ClassID = c.ClassID
                    WHERE c.ProfessorID = @ID";

                object avgResult = Database.ExecuteScalar(avgQuery, new[] { new SqlParameter("@ID", profId) });

                if (avgResult != null && avgResult != DBNull.Value)
                {
                    double avg = Convert.ToDouble(avgResult);
                    txtAvgAttendance.Text = $"{avg:F0}%";

                    if (avg >= 85) txtAvgAttendance.Foreground = Brushes.Green;
                    else if (avg >= 70) txtAvgAttendance.Foreground = Brushes.Orange;
                    else txtAvgAttendance.Foreground = Brushes.Red;
                }
                else
                {
                    txtAvgAttendance.Text = "0%";
                    txtAvgAttendance.Foreground = Brushes.Gray;
                }
            }
            catch (Exception ex)
            {
                // Optionally log error
                Console.WriteLine("Stats Error: " + ex.Message);
            }
        }
        private void CheckAttendance_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ClassSchedule selectedSchedule = btn.DataContext as ClassSchedule;

            if (selectedSchedule != null)
            {
                // Pass the correctly fetched ClassID to the next page
                ClassModel classData = new ClassModel
                {
                    ClassID = selectedSchedule.ClassID,
                    SubjectCode = selectedSchedule.SubjectCode,
                    Description = selectedSchedule.Description,
                    Section = selectedSchedule.Section,
                    Room = selectedSchedule.Room
                };

                this.NavigationService.Navigate(new AttendanceSheet(classData));
            }
        }
    }

    public class ClassSchedule
    {
        public int ClassID { get; set; } // Critical for the bug fix
        public string TimeRange { get; set; }
        public string SubjectCode { get; set; }
        public string Description { get; set; }
        public string Room { get; set; }
        public string Section { get; set; }
    }
}
