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
using System.Windows;
using Microsoft.Data.SqlClient; // Required for SQL
using QcuAttendanceSystem.Core;   // Required for Database & Session

namespace QcuAttendanceSystem.Views.Professor
{
    public partial class AddClassWindow : Window
    {
        public AddClassWindow()
        {
            InitializeComponent();
            PopulateTimeSelectors();
        }

        // 1. Generate 30-minute time slots (7:00 AM - 9:00 PM)
        private void PopulateTimeSelectors()
        {
            List<string> timeSlots = new List<string>();
            DateTime startTime = DateTime.Today.AddHours(7); // 7:00 AM
            DateTime endTime = DateTime.Today.AddHours(21);  // 9:00 PM

            while (startTime <= endTime)
            {
                timeSlots.Add(startTime.ToString("hh:mm tt"));
                startTime = startTime.AddMinutes(30);
            }

            cmbStart.ItemsSource = timeSlots;
            cmbEnd.ItemsSource = timeSlots;

            // Set Defaults
            cmbStart.SelectedItem = "07:00 AM";
            cmbEnd.SelectedItem = "10:00 AM";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 2. Validate Input
            if (string.IsNullOrWhiteSpace(txtSubject.Text) || string.IsNullOrWhiteSpace(txtSection.Text))
            {
                MessageBox.Show("Please fill in the Subject and Section.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbStart.SelectedIndex == -1 || cmbEnd.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a valid time range.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Prepare Data for Insertion
            string day = (cmbDay.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Monday";
            string startTime = cmbStart.SelectedItem.ToString();
            string endTime = cmbEnd.SelectedItem.ToString();

            try
            {
                // 4. SQL Insert Command
                string query = @"
                    INSERT INTO Classes (SubjectCode, Description, Section, Room, Day, StartTime, EndTime, ProfessorID) 
                    VALUES (@Subject, @Desc, @Section, @Room, @Day, @Start, @End, @ProfID)";

                SqlParameter[] parameters = {
                    new SqlParameter("@Subject", txtSubject.Text.Trim()),
                    new SqlParameter("@Desc", txtDesc.Text.Trim()),
                    new SqlParameter("@Section", txtSection.Text.Trim()),
                    new SqlParameter("@Room", txtRoom.Text.Trim()),
                    new SqlParameter("@Day", day),
                    new SqlParameter("@Start", startTime),
                    new SqlParameter("@End", endTime),
                    new SqlParameter("@ProfID", Session.CurrentUserID) // Link to logged-in Prof
                };

                // 5. Execute
                int rows = Database.ExecuteNonQuery(query, parameters);

                if (rows > 0)
                {
                    MessageBox.Show("Class created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; // Tells parent window that we succeeded
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}