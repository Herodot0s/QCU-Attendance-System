using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QcuAttendanceSystem.Core
{
    public static class Session
    {
        // Stores the currently logged-in user's details
        public static int CurrentUserID { get; set; }
        public static string CurrentUserName { get; set; }
        public static string CurrentFullName { get; set; }
        public static string CurrentRole { get; set; } // "Professor" or "Student"
        public static string CurrentSchoolID { get; set; }
        public static string CurrentCourse { get; set; }
        public static string CurrentSection { get; set; }

        // Helper to clear session on Logout
        public static void Logout()
        {
            CurrentUserID = 0;
            CurrentUserName = null;
            CurrentFullName = null;
            CurrentRole = null;
            CurrentSchoolID = null;
        }

        // Helper to check if someone is logged in
        public static bool IsLoggedIn()
        {
            return CurrentUserID > 0;
        }
    }
}