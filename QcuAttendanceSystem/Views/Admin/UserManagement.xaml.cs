using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using QcuAttendanceSystem.Core;

namespace QcuAttendanceSystem.Views.Admin
{
    public partial class UserManagement : Page
    {
        public UserManagement()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers(string searchTerm = "")
        {
            try
            {
                string query = @"
                    SELECT UserID, SchoolID, FullName, Username, Role, 
                           ISNULL(Course, '') + ' ' + ISNULL(Section, '') AS CourseInfo 
                    FROM Users 
                    WHERE FullName LIKE @search OR SchoolID LIKE @search
                    ORDER BY UserID DESC";

                SqlParameter[] p = { new SqlParameter("@search", $"%{searchTerm}%") };
                DataTable dt = Database.ExecuteQuery(query, p);
                UserGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadUsers(txtSearch.Text.Trim());
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            UserFormWindow form = new UserFormWindow();
            if (form.ShowDialog() == true)
            {
                LoadUsers(); // Refresh after add
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int userId)
            {
                UserFormWindow form = new UserFormWindow(userId);
                if (form.ShowDialog() == true)
                {
                    LoadUsers(); // Refresh after edit
                }
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int userId)
            {
                if (MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    string query = "DELETE FROM Users WHERE UserID = @id";
                    SqlParameter[] p = { new SqlParameter("@id", userId) };
                    Database.ExecuteNonQuery(query, p);
                    LoadUsers();
                }
            }
        }
    }
}