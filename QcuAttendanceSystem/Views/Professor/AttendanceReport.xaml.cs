using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.SqlClient;
using QcuAttendanceSystem.Core;

namespace QcuAttendanceSystem.Views.Professor
{
    public partial class AttendanceReport : Page
    {
        public AttendanceReport()
        {
            InitializeComponent();
            LoadClasses();
        }

        private void Back_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void LoadClasses()
        {
            try
            {
                string query = "SELECT ClassID, SubjectCode, Section FROM Classes WHERE ProfessorID = @ProfID";
                SqlParameter[] parameters = { new SqlParameter("@ProfID", Session.CurrentUserID) };

                DataTable dt = Database.ExecuteQuery(query, parameters);
                List<ClassDropdownItem> classList = new List<ClassDropdownItem>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        classList.Add(new ClassDropdownItem
                        {
                            ClassID = Convert.ToInt32(row["ClassID"]),
                            FullClassName = $"{row["SubjectCode"]} - {row["Section"]}"
                        });
                    }
                }

                cmbClasses.ItemsSource = classList;
                if (classList.Count > 0) cmbClasses.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading classes: " + ex.Message);
            }
        }

        private void cmbClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbClasses.SelectedValue != null)
            {
                int classId = (int)cmbClasses.SelectedValue;
                GeneratePivotReport(classId);
            }
        }

        // --- HANDLES 'Refresh Data' BUTTON ---
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClasses.SelectedValue != null)
            {
                GeneratePivotReport((int)cmbClasses.SelectedValue);
            }
        }

        // --- HANDLES 'Generate Report' BUTTON (Fixes missing reference) ---
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            Refresh_Click(sender, e);
        }

        // --- HANDLES 'Print / Export' BUTTON (Fixes missing reference) ---
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            Export_Click(sender, e);
        }

        // --- HANDLES 'Export' BUTTON ---
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to Excel/PDF feature coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReportGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void GeneratePivotReport(int classId)
        {
            try
            {
                // 1. Get Students
                string studentQuery = @"
                    SELECT u.SchoolID, u.FullName, e.EnrollmentID 
                    FROM Enrollments e
                    JOIN Users u ON e.StudentID = u.UserID
                    WHERE e.ClassID = @ClassID
                    ORDER BY u.FullName";

                // 2. Get Dates
                string dateQuery = @"
                    SELECT DISTINCT a.Date 
                    FROM Attendance a
                    JOIN Enrollments e ON a.EnrollmentID = e.EnrollmentID
                    WHERE e.ClassID = @ClassID
                    ORDER BY a.Date";

                // 3. Get Records
                string recordsQuery = @"
                    SELECT e.EnrollmentID, a.Date, a.Status
                    FROM Attendance a
                    JOIN Enrollments e ON a.EnrollmentID = e.EnrollmentID
                    WHERE e.ClassID = @ClassID";

                SqlParameter[] p1 = { new SqlParameter("@ClassID", classId) };
                SqlParameter[] p2 = { new SqlParameter("@ClassID", classId) };
                SqlParameter[] p3 = { new SqlParameter("@ClassID", classId) };

                DataTable dtStudents = Database.ExecuteQuery(studentQuery, p1);
                DataTable dtDates = Database.ExecuteQuery(dateQuery, p2);
                DataTable dtRecords = Database.ExecuteQuery(recordsQuery, p3);

                DataTable pivotTable = new DataTable();
                pivotTable.Columns.Add("Name", typeof(string));
                pivotTable.Columns.Add("Student ID", typeof(string));

                List<DateTime> classDates = new List<DateTime>();
                if (dtDates != null)
                {
                    foreach (DataRow row in dtDates.Rows)
                    {
                        DateTime date = Convert.ToDateTime(row["Date"]);
                        classDates.Add(date);
                        pivotTable.Columns.Add(date.ToString("MMM dd"), typeof(string));
                    }
                }

                pivotTable.Columns.Add("Total Present", typeof(int));
                pivotTable.Columns.Add("Total Absent", typeof(int));
                pivotTable.Columns.Add("Rating (%)", typeof(string));

                if (dtStudents != null)
                {
                    foreach (DataRow studentRow in dtStudents.Rows)
                    {
                        DataRow newRow = pivotTable.NewRow();
                        int enrollmentID = Convert.ToInt32(studentRow["EnrollmentID"]);

                        newRow["Name"] = studentRow["FullName"];
                        newRow["Student ID"] = studentRow["SchoolID"];

                        int presentCount = 0;
                        int lateCount = 0;
                        int absentCount = 0;

                        foreach (DateTime date in classDates)
                        {
                            DataRow[] records = dtRecords.Select($"EnrollmentID = {enrollmentID} AND Date = '{date:yyyy-MM-dd}'");
                            string colName = date.ToString("MMM dd");

                            if (records.Length > 0)
                            {
                                string status = records[0]["Status"].ToString();
                                if (status == "Present") { newRow[colName] = "P"; presentCount++; }
                                else if (status == "Late") { newRow[colName] = "L"; lateCount++; }
                                else if (status == "Absent") { newRow[colName] = "A"; absentCount++; }
                                else { newRow[colName] = "-"; }
                            }
                            else
                            {
                                newRow[colName] = "-";
                            }
                        }

                        int totalSessions = classDates.Count;
                        double percentage = totalSessions > 0 ? ((double)(presentCount + lateCount) / totalSessions) * 100 : 0;

                        newRow["Total Present"] = presentCount + lateCount;
                        newRow["Total Absent"] = absentCount;
                        newRow["Rating (%)"] = $"{percentage:F0}%";

                        pivotTable.Rows.Add(newRow);
                    }
                }

                ReportGrid.ItemsSource = pivotTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating report: " + ex.Message);
            }
        }
    }

    public class ClassDropdownItem
    {
        public int ClassID { get; set; }
        public string FullClassName { get; set; }
    }
}