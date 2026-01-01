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
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using QcuAttendanceSystem.Core;

namespace QcuAttendanceSystem.Views.Student
{
    public partial class EnrollmentPage : Page
    {
        public EnrollmentPage()
        {
            InitializeComponent();
            LoadAvailableClasses();
        }

        private void LoadAvailableClasses()
        {
            try
            {
                // Query: Get all classes, join with Users table to get Prof Name
                // Also check if student is ALREADY enrolled to hide/disable those (Optional logic)
                string query = @"
                    SELECT 
                        c.ClassID,
                        c.SubjectCode,
                        c.Description,
                        c.Day,
                        c.StartTime,
                        c.EndTime,
                        u.FullName AS ProfessorName
                    FROM Classes c
                    JOIN Users u ON c.ProfessorID = u.UserID
                    WHERE c.ClassID NOT IN (SELECT ClassID FROM Enrollments WHERE StudentID = @StudentID)";

                SqlParameter[] parameters = {
                    new SqlParameter("@StudentID", Session.CurrentUserID)
                };

                DataTable dt = Database.ExecuteQuery(query, parameters);
                List<EnrollmentModel> classes = new List<EnrollmentModel>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        classes.Add(new EnrollmentModel
                        {
                            ClassID = Convert.ToInt32(row["ClassID"]),
                            SubjectCode = row["SubjectCode"].ToString(),
                            Description = row["Description"].ToString(),
                            ProfessorName = row["ProfessorName"].ToString(),
                            Schedule = $"{row["Day"]} • {row["StartTime"]} - {row["EndTime"]}"
                        });
                    }
                }

                AvailableClassesList.ItemsSource = classes;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading classes: " + ex.Message);
            }
        }

        private void Enroll_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int classId = (int)btn.Tag;

            if (MessageBox.Show("Are you sure you want to enroll in this subject?", "Confirm Enrollment", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    string insertQuery = "INSERT INTO Enrollments (StudentID, ClassID) VALUES (@Sid, @Cid)";
                    SqlParameter[] parameters = {
                        new SqlParameter("@Sid", Session.CurrentUserID),
                        new SqlParameter("@Cid", classId)
                    };

                    Database.ExecuteNonQuery(insertQuery, parameters);

                    MessageBox.Show("Enrolled successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh list to remove the enrolled class
                    LoadAvailableClasses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Enrollment Failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class EnrollmentModel
    {
        public int ClassID { get; set; }
        public string SubjectCode { get; set; }
        public string Description { get; set; }
        public string ProfessorName { get; set; }
        public string Schedule { get; set; }
    }
}
