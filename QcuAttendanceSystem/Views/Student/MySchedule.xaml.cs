using QcuAttendanceSystem.Core;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Data;
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
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using QcuAttendanceSystem.Core;

namespace QcuAttendanceSystem.Views.Student
{
    public partial class MySchedule : Page
    {
        public MySchedule()
        {
            InitializeComponent();
            LoadStudentSchedule();
        }

        private void LoadStudentSchedule()
        {
            try
            {
                // Fetch classes linked to this student via Enrollments table
                string query = @"
            SELECT 
                c.SubjectCode,
                c.Description,
                c.Room,
                c.Day,
                c.StartTime,
                c.EndTime,
                u.FullName AS ProfessorName
            FROM Enrollments e
            JOIN Classes c ON e.ClassID = c.ClassID
            JOIN Users u ON c.ProfessorID = u.UserID
            WHERE e.StudentID = @StudentID
            ORDER BY c.Day, c.StartTime";

                SqlParameter[] parameters = {
            new SqlParameter("@StudentID", Session.CurrentUserID)
        };

                DataTable dt = Database.ExecuteQuery(query, parameters);
                List<StudentScheduleModel> myClasses = new List<StudentScheduleModel>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string timeSlot = $"{row["Day"]} • {row["StartTime"]} - {row["EndTime"]}";

                        myClasses.Add(new StudentScheduleModel
                        {
                            SubjectCode = row["SubjectCode"].ToString(),
                            Description = row["Description"].ToString(),
                            Room = row["Room"].ToString(),
                            Instructor = row["ProfessorName"].ToString(),
                            TimeSlot = timeSlot
                        });
                    }
                }

                StudentScheduleGrid.ItemsSource = myClasses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading schedule: " + ex.Message);
            }
        }
    }

    // Helper Class
    public class StudentScheduleModel
    {
        public string TimeSlot { get; set; }
        public string SubjectCode { get; set; }
        public string Description { get; set; }
        public string Room { get; set; }
        public string Instructor { get; set; }
    }
}