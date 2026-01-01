## 🚀 Overview

The QCU Attendance System is a Windows Presentation Foundation (WPF) application built on **.NET 8.0**. It facilitates seamless interaction between administrators, professors, and students, replacing manual paper-based tracking with a digital, database-driven approach.

## 🛠 Technologies Used

* **Framework:** .NET 8.0 (Windows)
* **UI Framework:** WPF (Windows Presentation Foundation)
* **Language:** C#
* **Database:** Microsoft SQL Server
* **Data Access:** Microsoft.Data.SqlClient / System.Data.SqlClient
* **IDE:** Visual Studio 2022

## ✨ Key Features

The system is divided into functional modules based on user roles:

### 1. 🎓 Student Module

* **Dashboard:** Personalized landing page for students.
* **Enrollment:** Digital enrollment capabilities for specific courses.
* **Schedule Viewer:** View current class schedules.
* **Attendance History:** Track personal attendance records and absences.

### 2. 👨‍🏫 Professor Module

* **Dashboard:** Overview of handled classes and daily schedules.
* **Class Management:** Create and configure new classes.
* **Digital Attendance Sheet:** Interface for marking and viewing daily attendance.
* **Reporting:** Generate and view attendance reports.

### 3. 🛡️ Admin Module

* **User Management:** Add, edit, and manage user accounts (Students and Professors).
* **System Configuration:** Global settings for the attendance environment.

### 4. 🖥️ Kiosk Mode

* **Quick Access:** A dedicated Kiosk Window designed for rapid attendance logging (e.g., via ID scanning or quick entry).

---

## 📂 Project Structure

* **Core/**: Contains essential logic including Database connections and Session management.
* **Views/**:
* `Admin/`: User management and administrative forms.
* `Login/`: Authentication screens.
* `Professor/`: Teacher-specific dashboards and reporting tools.
* `Student/`: Student history, schedule, and enrollment views.
* `KioskWindow.xaml`: Standalone interface for attendance terminals.


* **Assets/**: Contains application resources such as `logo_qcu.png` and background images.

---

## ⚙️ Installation & Setup

1. **Prerequisites:**
* Install **Visual Studio 2022** (or later) with the **.NET Desktop Development** workload.
* Install **Microsoft SQL Server**.


2. **Database Setup:**
* Locate the `DATABASE.txt` file in the root directory.
* Run the SQL script inside `DATABASE.txt` in SQL Server Management Studio (SSMS) to set up the required tables and schema.
* Update the connection string in `Core/Database.cs` to match your local SQL Server instance.


3. **Build and Run:**
* Open `QcuAttendanceSystem.slnx` or `QcuAttendanceSystem.csproj` in Visual Studio.
* Restore NuGet packages (specifically `Microsoft.Data.SqlClient`).
* Build the solution (Ctrl + Shift + B).
* Start the application (F5).



---

## 👥 Contributors

**Created with ❤️ by the Aspiring IT Students of QCU San Francisco Campus**
*Year: 2025*

*This project was developed to demonstrate proficiency in C#, WPF, and Database Management Systems within an academic setting.*

---

## 📄 License

This project is proprietary to the development team at Quezon City University. Unauthorized reproduction or distribution without permission is prohibited.
