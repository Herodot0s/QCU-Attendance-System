using Microsoft.Data.SqlClient;
using QcuAttendanceSystem.Core;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace QcuAttendanceSystem.Views.Admin
{
    public partial class UserFormWindow : Window
    {
        private int _userId = 0; // 0 = Add Mode

        public UserFormWindow(int id = 0)
        {
            InitializeComponent();
            _userId = id;

            if (_userId > 0)
                LoadUserData();
        }

        private void LoadUserData()
        {
            string query = "SELECT * FROM Users WHERE UserID = @id";
            SqlParameter[] p = { new SqlParameter("@id", _userId) };
            DataTable dt = Database.ExecuteQuery(query, p);

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtSchoolID.Text = row["SchoolID"].ToString();
                txtName.Text = row["FullName"].ToString();
                txtUsername.Text = row["Username"].ToString();
                txtPassword.Text = row["Password"].ToString(); // In real app, don't show password
                cmbRole.Text = row["Role"].ToString();
                txtCourse.Text = row["Course"].ToString();
                txtSection.Text = row["Section"].ToString();
            }
        }

        private void cmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only show Course/Section if Student
            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (role == "Student")
                pnlStudentInfo.Visibility = Visibility.Visible;
            else
                pnlStudentInfo.Visibility = Visibility.Collapsed;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please fill in required fields.");
                return;
            }

            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Student";

            try
            {
                string query;
                if (_userId == 0) // INSERT
                {
                    query = @"INSERT INTO Users (SchoolID, FullName, Username, Password, Role, Course, Section) 
                              VALUES (@sid, @name, @user, @pass, @role, @course, @sec)";
                }
                else // UPDATE
                {
                    query = @"UPDATE Users SET SchoolID=@sid, FullName=@name, Username=@user, 
                              Password=@pass, Role=@role, Course=@course, Section=@sec 
                              WHERE UserID=@id";
                }

                SqlParameter[] p = {
                    new SqlParameter("@sid", txtSchoolID.Text),
                    new SqlParameter("@name", txtName.Text),
                    new SqlParameter("@user", txtUsername.Text),
                    new SqlParameter("@pass", txtPassword.Text),
                    new SqlParameter("@role", role),
                    new SqlParameter("@course", txtCourse.Text),
                    new SqlParameter("@sec", txtSection.Text),
                    new SqlParameter("@id", _userId)
                };

                Database.ExecuteNonQuery(query, p);
                this.DialogResult = true; // Success
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}