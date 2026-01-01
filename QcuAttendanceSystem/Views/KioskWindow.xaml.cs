using Microsoft.Data.SqlClient;
using QcuAttendanceSystem.Core;
using QcuAttendanceSystem.Views.Login; // To link back to login
using QcuAttendanceSystem.Views.Login;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading; // Required for Timer

namespace QcuAttendanceSystem.Views
{
    public partial class KioskWindow : Window
    {
        private DispatcherTimer _clockTimer;
        private DispatcherTimer _resetTimer;

        public KioskWindow()
        {
            InitializeComponent();
            StartClock();
            txtStudentID.Focus();
        }

        private void StartClock()
        {
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) =>
            {
                txtTime.Text = DateTime.Now.ToString("h:mm:ss tt");
                txtDate.Text = DateTime.Now.ToString("dddd, MMM dd, yyyy");
            };
            _clockTimer.Start();
        }

        private void txtStudentID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string id = txtStudentID.Text.Trim();
                if (!string.IsNullOrEmpty(id))
                {
                    ProcessAttendance(id);
                }
                txtStudentID.Clear();
            }
        }

        private void ProcessAttendance(string schoolId)
        {
            if (_resetTimer != null) _resetTimer.Stop();

            try
            {
                // 1. FIND STUDENT
                string userQuery = "SELECT UserID, FullName FROM Users WHERE SchoolID = @id AND Role = 'Student'";
                SqlParameter[] p1 = { new SqlParameter("@id", schoolId) };
                DataTable userTable = Database.ExecuteQuery(userQuery, p1);

                if (userTable != null && userTable.Rows.Count > 0)
                {
                    DataRow student = userTable.Rows[0];
                    int studentId = Convert.ToInt32(student["UserID"]);
                    string name = student["FullName"].ToString();

                    // 2. FETCH CLASSES AND DETERMINE ATTENDANCE ACTION
                    HandleAttendanceLogic(studentId, name);

                    // 3. REFRESH LIST FOR UI
                    LoadStudentClasses(studentId);
                }
                else
                {
                    ShowStatus(false, "ID Not Found");
                }
            }
            catch (Exception ex)
            {
                ShowStatus(false, "System Error");
                // Log exception in production
            }

            StartResetTimer();
        }

        private void HandleAttendanceLogic(int studentId, string studentName)
        {
            string dayToday = DateTime.Now.DayOfWeek.ToString();
            DateTime now = DateTime.Now;

            // Get all classes for this student for TODAY
            string query = @"
                SELECT e.EnrollmentID, c.ClassID, c.SubjectCode, c.StartTime, c.EndTime 
                FROM Enrollments e
                JOIN Classes c ON e.ClassID = c.ClassID
                WHERE e.StudentID = @sid AND c.Day = @day";

            SqlParameter[] p = {
                new SqlParameter("@sid", studentId),
                new SqlParameter("@day", dayToday)
            };

            DataTable dt = Database.ExecuteQuery(query, p);
            bool actionTaken = false;

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    int enrollmentId = Convert.ToInt32(row["EnrollmentID"]);
                    string subject = row["SubjectCode"].ToString();

                    // Parse stored string times (e.g., "07:00 AM") to DateTime for today
                    DateTime startTime = DateTime.ParseExact(row["StartTime"].ToString(), "hh:mm tt", CultureInfo.InvariantCulture);
                    DateTime endTime = DateTime.ParseExact(row["EndTime"].ToString(), "hh:mm tt", CultureInfo.InvariantCulture);

                    // Define Valid Window: 30 mins before start until end of class
                    if (now >= startTime.AddMinutes(-30) && now <= endTime)
                    {
                        CheckAndRecord(enrollmentId, startTime, subject, studentName);
                        actionTaken = true;
                        break; // Stop after processing the relevant class
                    }
                }
            }

            if (!actionTaken)
            {
                ShowStatus(false, "No Class Scheduled Now");
            }
        }

        private void CheckAndRecord(int enrollmentId, DateTime classStartTime, string subject, string studentName)
        {
            DateTime now = DateTime.Now;
            string dateToday = now.ToString("yyyy-MM-dd");

            // Check if a record exists for today
            string checkQuery = "SELECT AttendanceID, TimeOut FROM Attendance WHERE EnrollmentID = @eid AND Date = @date";
            SqlParameter[] p = {
                new SqlParameter("@eid", enrollmentId),
                new SqlParameter("@date", dateToday)
            };

            DataTable dt = Database.ExecuteQuery(checkQuery, p);

            if (dt != null && dt.Rows.Count > 0)
            {
                // Record Exists
                DataRow row = dt.Rows[0];
                if (row["TimeOut"] == DBNull.Value)
                {
                    // === TIME OUT LOGIC ===
                    string update = "UPDATE Attendance SET TimeOut = @time WHERE AttendanceID = @aid";
                    SqlParameter[] updateParams = {
                        new SqlParameter("@time", now),
                        new SqlParameter("@aid", row["AttendanceID"])
                    };
                    Database.ExecuteNonQuery(update, updateParams);
                    ShowStatus(true, $"{studentName}\nTIMED OUT: {subject}");
                }
                else
                {
                    ShowStatus(false, "Already Completed Class");
                }
            }
            else
            {
                // === TIME IN LOGIC ===

                // Determine Status: Late if 15 mins past start time
                string status = (now > classStartTime.AddMinutes(15)) ? "Late" : "Present";

                string insert = @"
                    INSERT INTO Attendance (EnrollmentID, Date, TimeIn, Status) 
                    VALUES (@eid, @date, @time, @status)";

                SqlParameter[] insertParams = {
                    new SqlParameter("@eid", enrollmentId),
                    new SqlParameter("@date", dateToday),
                    new SqlParameter("@time", now),
                    new SqlParameter("@status", status)
                };
                Database.ExecuteNonQuery(insert, insertParams);

                string statusMsg = (status == "Late") ? "(LATE)" : "(PRESENT)";
                ShowStatus(true, $"{studentName}\nTIMED IN {statusMsg}: {subject}");
            }
        }

        private void LoadStudentClasses(int studentId)
        {
            // Get today's day name (e.g., "Monday")
            string today = DateTime.Now.DayOfWeek.ToString();

            string query = @"
                SELECT c.SubjectCode, c.Description, c.Room, c.StartTime, c.EndTime
                FROM Enrollments e
                JOIN Classes c ON e.ClassID = c.ClassID
                WHERE e.StudentID = @sid AND c.Day = @day
                ORDER BY c.StartTime";

            SqlParameter[] parameters = {
                new SqlParameter("@sid", studentId),
                new SqlParameter("@day", today)
            };

            DataTable dt = Database.ExecuteQuery(query, parameters);
            List<KioskClassModel> classes = new List<KioskClassModel>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    classes.Add(new KioskClassModel
                    {
                        SubjectCode = row["SubjectCode"].ToString(),
                        Description = row["Description"].ToString(),
                        Room = row["Room"].ToString(),
                        StartTime = row["StartTime"].ToString(),
                        EndTime = row["EndTime"].ToString()
                    });
                }
                TodaysClassesList.ItemsSource = classes;
                lblNoClasses.Visibility = Visibility.Collapsed;
            }
            else
            {
                TodaysClassesList.ItemsSource = null;
                lblNoClasses.Text = "No classes for today.";
                lblNoClasses.Visibility = Visibility.Visible;
            }
        }

        private void ShowStatus(bool success, string message)
        {
            StatusCard.Visibility = Visibility.Visible;
            if (success)
            {
                StatusCard.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // Light Green
                lblWelcome.Text = "Time-In Successful!";
                lblWelcome.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                lblStudentName.Text = message.ToUpper();
                lblTimeIn.Text = "Time: " + DateTime.Now.ToString("h:mm tt");
            }
            else
            {
                StatusCard.Background = new SolidColorBrush(Color.FromRgb(253, 236, 234)); // Light Red
                lblWelcome.Text = "Error";
                lblWelcome.Foreground = Brushes.Red;
                lblStudentName.Text = message;
                lblTimeIn.Text = "Please try again.";
                TodaysClassesList.ItemsSource = null;
                lblNoClasses.Visibility = Visibility.Visible;
                lblNoClasses.Text = "No data.";
            }
        }

        private void StartResetTimer()
        {
            _resetTimer = new DispatcherTimer();
            _resetTimer.Interval = TimeSpan.FromSeconds(6);
            _resetTimer.Tick += (s, e) =>
            {
                StatusCard.Visibility = Visibility.Hidden;
                TodaysClassesList.ItemsSource = null;
                lblNoClasses.Text = "Waiting for ID...";
                lblNoClasses.Visibility = Visibility.Visible;
                _resetTimer.Stop();
            };
            _resetTimer.Start();
        }

        private void GoToPortal_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }

    public class KioskClassModel
    {
        public string SubjectCode { get; set; }
        public string Description { get; set; }
        public string Room { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}