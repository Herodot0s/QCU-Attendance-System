using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Data.SqlClient;
using QcuAttendanceSystem.Core;

namespace QcuAttendanceSystem.Views.Professor
{
    public partial class ClassSelection : Page
    {
        public ClassSelection()
        {
            InitializeComponent();
            LoadClasses();
        }

        public void LoadClasses()
        {
            try
            {
                if (Session.CurrentUserID == 0) return;

                // UPDATED QUERY: Added StartTime and EndTime
                string query = @"
                    SELECT 
                        c.ClassID, 
                        c.SubjectCode, 
                        c.Description, 
                        c.Section, 
                        c.Room,
                        c.StartTime,
                        c.EndTime,
                        (SELECT COUNT(*) FROM Enrollments e WHERE e.ClassID = c.ClassID) as StudentCount
                    FROM Classes c
                    WHERE c.ProfessorID = @ProfID";

                SqlParameter[] parameters = {
                    new SqlParameter("@ProfID", Session.CurrentUserID)
                };

                DataTable result = Database.ExecuteQuery(query, parameters);

                List<ClassModel> myClasses = new List<ClassModel>();

                Brush[] colors = { Brushes.Orange, Brushes.RoyalBlue, Brushes.Crimson, Brushes.ForestGreen, Brushes.Purple, Brushes.Teal };
                int colorIndex = 0;

                if (result != null)
                {
                    foreach (DataRow row in result.Rows)
                    {
                        myClasses.Add(new ClassModel
                        {
                            ClassID = Convert.ToInt32(row["ClassID"]),
                            SubjectCode = row["SubjectCode"].ToString(),
                            Description = row["Description"].ToString(),
                            Section = row["Section"].ToString(),
                            Room = row["Room"].ToString(),
                            // Handle potential DBNulls for times
                            StartTime = row["StartTime"] != DBNull.Value ? row["StartTime"].ToString() : "",
                            EndTime = row["EndTime"] != DBNull.Value ? row["EndTime"].ToString() : "",
                            StudentCount = Convert.ToInt32(row["StudentCount"]),
                            ColorCode = colors[colorIndex % colors.Length]
                        });
                        colorIndex++;
                    }
                }

                ClassesList.ItemsSource = myClasses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading classes:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageClass_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ClassModel selectedClass = btn.Tag as ClassModel;

            if (selectedClass != null)
            {
                this.NavigationService.Navigate(new AttendanceSheet(selectedClass));
            }
        }

        private void AddClass_Click(object sender, RoutedEventArgs e)
        {
            AddClassWindow addWindow = new AddClassWindow();
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                LoadClasses();
            }
        }
    }

    // === FIXED DATA MODEL ===
    public class ClassModel
    {
        public int ClassID { get; set; }
        public string SubjectCode { get; set; }
        public string Description { get; set; }
        public string Section { get; set; }
        public string Room { get; set; }

        // Fixed: Changed from internal ReadOnlySpan (invalid) to public string properties
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public int StudentCount { get; set; }
        public Brush ColorCode { get; set; }
    }
}