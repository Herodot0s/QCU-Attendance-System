using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Required for ObservableCollection
using System.Data;                    // Required for DataTable
using System.Linq;                    // Required for LINQ extensions (Count, etc.)
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;      // Required for NavigationService
using Microsoft.Data.SqlClient;       // Required for SQL connection
using QcuAttendanceSystem.Core;       // Required for Database access

namespace QcuAttendanceSystem.Views.Professor
{
    public partial class AttendanceSheet : Page
    {
        private ClassModel _currentClass;
        public ObservableCollection<AttendanceModel> Students { get; set; }

        public AttendanceSheet()
        {
            InitializeComponent();
        }

        // Constructor receiving the passed ClassModel
        public AttendanceSheet(ClassModel classInfo) : this()
        {
            _currentClass = classInfo;

            // Display Header Info
            lblSubjectCode.Text = _currentClass.SubjectCode;
            lblDetails.Text = $"{_currentClass.Description} • {_currentClass.Section}";
            txtDate.Text = DateTime.Now.ToString("MMM dd, yyyy");

            LoadStudents();
            AttendanceGrid.ItemsSource = Students;
        }

        private void LoadStudents()
        {
            try
            {
                // 1. GET CLASS SCHEDULE FIRST (To determine if we should Auto-Absent)
                string scheduleQuery = "SELECT StartTime, EndTime FROM Classes WHERE ClassID = @cid";
                SqlParameter[] schParams = { new SqlParameter("@cid", _currentClass.ClassID) };
                DataTable schDt = Database.ExecuteQuery(scheduleQuery, schParams);

                bool isClassOver = false;
                if (schDt != null && schDt.Rows.Count > 0)
                {
                    string endStr = schDt.Rows[0]["EndTime"].ToString();
                    if (DateTime.TryParse(endStr, out DateTime endTime))
                    {
                        // Compare today's time with class end time
                        DateTime classEndToday = DateTime.Today.Add(endTime.TimeOfDay);
                        if (DateTime.Now > classEndToday) isClassOver = true;
                    }
                }

                // 2. GET STUDENTS AND ATTENDANCE
                string query = @"
                    SELECT 
                        u.SchoolID, 
                        u.FullName, 
                        e.EnrollmentID,
                        a.Status, 
                        a.Remarks,
                        a.TimeIn,
                        a.TimeOut
                    FROM Enrollments e
                    JOIN Users u ON e.StudentID = u.UserID
                    LEFT JOIN Attendance a ON e.EnrollmentID = a.EnrollmentID AND a.Date = CAST(GETDATE() AS DATE)
                    WHERE e.ClassID = @ClassID
                    ORDER BY u.FullName";

                SqlParameter[] parameters = {
                    new SqlParameter("@ClassID", _currentClass.ClassID)
                };

                DataTable dt = Database.ExecuteQuery(query, parameters);
                Students = new ObservableCollection<AttendanceModel>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var student = new AttendanceModel
                        {
                            EnrollmentID = Convert.ToInt32(row["EnrollmentID"]),
                            StudentID = row["SchoolID"].ToString(),
                            FullName = row["FullName"].ToString(),
                            Remarks = row["Remarks"] != DBNull.Value ? row["Remarks"].ToString() : ""
                        };

                        // Handle Time In Display
                        if (row["TimeIn"] != DBNull.Value)
                        {
                            DateTime ti = Convert.ToDateTime(row["TimeIn"]);
                            student.TimeInDisplay = ti.ToString("h:mm tt");
                        }

                        // Handle Time Out Display
                        if (row["TimeOut"] != DBNull.Value)
                        {
                            DateTime to = Convert.ToDateTime(row["TimeOut"]);
                            student.TimeOutDisplay = to.ToString("h:mm tt");
                        }

                        // Handle Status (Present/Late/Absent)
                        string status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "";

                        if (status == "Present") student.IsPresent = true;
                        else if (status == "Late") student.IsLate = true;
                        else if (status == "Absent") student.IsAbsent = true;
                        else
                        {
                            // AUTO-ABSENT LOGIC:
                            // If no status yet, no TimeIn, and class is finished -> Mark Absent automatically
                            if (isClassOver && string.IsNullOrEmpty(student.TimeInDisplay))
                            {
                                student.IsAbsent = true;
                            }
                        }

                        Students.Add(student);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading students:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MarkAllPresent_Click(object sender, RoutedEventArgs e)
        {
            foreach (var student in Students)
            {
                student.IsPresent = true;
                student.IsLate = false;
                student.IsAbsent = false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Check if any data exists to save
            int presentCount = Students.Count(s => s.IsPresent);
            int lateCount = Students.Count(s => s.IsLate);
            int absentCount = Students.Count(s => s.IsAbsent);

            if (presentCount + lateCount + absentCount == 0)
            {
                MessageBox.Show("No attendance status marked.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Save changes to attendance records?", "Confirm Save", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var student in Students)
                    {
                        // Determine selected status
                        string status = "";
                        if (student.IsPresent) status = "Present";
                        else if (student.IsLate) status = "Late";
                        else if (student.IsAbsent) status = "Absent";

                        // Skip if nothing selected
                        if (string.IsNullOrEmpty(status)) continue;

                        // SQL MERGE: Updates existing OR Inserts new.
                        // IMPORTANT: 'UPDATE' clause does NOT touch TimeIn/TimeOut, preserving Kiosk data.
                        string saveQuery = @"
                            MERGE Attendance AS target
                            USING (SELECT @EnrollmentID AS EnrollID, CAST(GETDATE() AS DATE) AS AttDate) AS source
                            ON (target.EnrollmentID = source.EnrollID AND target.Date = source.AttDate)
                            WHEN MATCHED THEN
                                UPDATE SET Status = @Status, Remarks = @Remarks
                            WHEN NOT MATCHED THEN
                                INSERT (EnrollmentID, Date, TimeIn, Status, Remarks)
                                VALUES (@EnrollmentID, CAST(GETDATE() AS DATE), NULL, @Status, @Remarks);";

                        SqlParameter[] parameters = {
                            new SqlParameter("@EnrollmentID", student.EnrollmentID),
                            new SqlParameter("@Status", status),
                            new SqlParameter("@Remarks", student.Remarks ?? "")
                        };

                        Database.ExecuteNonQuery(saveQuery, parameters);
                    }

                    MessageBox.Show("Attendance saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Optional: Reload to refresh data
                    // LoadStudents(); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving data:\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Back_Click(object sender, MouseButtonEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }
    }

    // === DATA MODEL FOR THE GRID ===
    public class AttendanceModel : System.ComponentModel.INotifyPropertyChanged
    {
        public int EnrollmentID { get; set; }
        public string StudentID { get; set; }
        public string FullName { get; set; }
        public string Remarks { get; set; }

        public string TimeInDisplay { get; set; } = "";
        public string TimeOutDisplay { get; set; } = "";

        // Radio Button Properties
        private bool _isPresent;
        public bool IsPresent
        {
            get { return _isPresent; }
            set { _isPresent = value; OnPropertyChanged("IsPresent"); }
        }

        private bool _isLate;
        public bool IsLate
        {
            get { return _isLate; }
            set { _isLate = value; OnPropertyChanged("IsLate"); }
        }

        private bool _isAbsent;
        public bool IsAbsent
        {
            get { return _isAbsent; }
            set { _isAbsent = value; OnPropertyChanged("IsAbsent"); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
}