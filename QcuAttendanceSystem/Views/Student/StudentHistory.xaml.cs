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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.SqlClient;
using QcuAttendanceSystem.Core;

namespace QcuAttendanceSystem.Views.Student
{
    public partial class StudentHistory : Page
    {
        public StudentHistory()
        {
            InitializeComponent();
            LoadAttendanceHistory();
        }

        private void LoadAttendanceHistory()
        {
            try
            {
                // Join Attendance -> Enrollments -> Classes to get full details
                string query = @"
                    SELECT 
                        a.Date, 
                        c.SubjectCode, 
                        c.Description, 
                        a.TimeIn, 
                        a.TimeOut, 
                        a.Status
                    FROM Attendance a
                    JOIN Enrollments e ON a.EnrollmentID = e.EnrollmentID
                    JOIN Classes c ON e.ClassID = c.ClassID
                    WHERE e.StudentID = @sid
                    ORDER BY a.Date DESC, a.TimeIn DESC";

                SqlParameter[] p = { new SqlParameter("@sid", Session.CurrentUserID) };
                DataTable dt = Database.ExecuteQuery(query, p);

                if (dt != null)
                {
                    List<AttendanceRecord> records = new List<AttendanceRecord>();
                    int presentCount = 0;
                    int lateCount = 0;
                    int absentCount = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        string status = row["Status"].ToString();

                        // Count for Summary Cards
                        if (status == "Present") presentCount++;
                        else if (status == "Late") lateCount++;
                        else if (status == "Absent") absentCount++;

                        // Add to list
                        records.Add(new AttendanceRecord
                        {
                            Date = Convert.ToDateTime(row["Date"]),
                            SubjectCode = row["SubjectCode"].ToString(),
                            Description = row["Description"].ToString(),
                            TimeIn = row["TimeIn"] != DBNull.Value ? Convert.ToDateTime(row["TimeIn"]).ToString("hh:mm tt") : "--:--",
                            TimeOut = row["TimeOut"] != DBNull.Value ? Convert.ToDateTime(row["TimeOut"]).ToString("hh:mm tt") : "--:--",
                            Status = status,
                            StatusColor = GetStatusBrush(status)
                        });
                    }

                    // Update UI
                    HistoryGrid.ItemsSource = records;
                    lblPresent.Text = presentCount.ToString();
                    lblLate.Text = lateCount.ToString();
                    lblAbsent.Text = absentCount.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load history: " + ex.Message);
            }
        }

        private SolidColorBrush GetStatusBrush(string status)
        {
            // Returns the color based on status string
            if (status == "Present") return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")); // Green
            if (status == "Late") return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF6C00"));    // Orange
            if (status == "Absent") return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));  // Red
            return Brushes.Gray;
        }

        // Helper Class for Grid Binding
        public class AttendanceRecord
        {
            public DateTime Date { get; set; }
            public string SubjectCode { get; set; }
            public string Description { get; set; }
            public string TimeIn { get; set; }
            public string TimeOut { get; set; }
            public string Status { get; set; }
            public SolidColorBrush StatusColor { get; set; }
        }
    }
}
