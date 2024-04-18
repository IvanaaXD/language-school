﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using LangLang.View.Director;
using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using LangLang.Observer;
using LangLang.View.Teacher;
using LangLang.View.Student;
using RegistrationForm = LangLang.View.Student.RegistrationForm;
using LangLang.Model.DAO;

namespace LangLang
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private StudentsController studentController { get; set; }
        private DirectorController directorController { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            directorController = new DirectorController();
            studentController = new StudentsController();

            // Postavljanje placeholder teksta i događaja
            EmailPlaceholder.Visibility = Visibility.Visible;
            PasswordPlaceholder.Visibility = Visibility.Visible;

            Email.GotFocus += EmailTextBox_GotFocus;
            Password.GotFocus += PasswordBox_GotFocus;

            Email.LostFocus += EmailTextBox_LostFocus;
            Password.LostFocus += PasswordBox_LostFocus;

            // Omogućuje klik na mjesto gdje je placeholder kako bi se fokusiralo TextBox ili PasswordBox
            EmailPlaceholder.MouseDown += Placeholder_MouseDown;
            PasswordPlaceholder.MouseDown += Placeholder_MouseDown;
        }


        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = Email.Text; 
            string password = Password.Password; 

            foreach (Teacher teacher in directorController.GetAllTeachers())
            {
                if (teacher.Email == email && teacher.Password == password)
                {
                    CoursesTable coursesTable = new CoursesTable(teacher.Id, directorController);
                    coursesTable.Show();
                    this.Close();
                    return;
                }
            }

            foreach (Student student in studentController.GetAllStudents())
            {
                if (student.Email == email && student.Password == password)
                {
                    LangLang.View.Student.WelcomePage welcomePage = new LangLang.View.Student.WelcomePage(student.Id, studentController);
                    welcomePage.Show();
                    this.Close();
                    return;
                }
            }

            Director director = directorController.GetDirector();

            if (director.Email == email && director.Password == password)
            {
                TeachersTable table = new TeachersTable();
                table.Show();
                this.Close();
                return;
            }
            
            
             MessageBox.Show("User does not exist.");
        } 

   
        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {
            RegistrationForm registrationForm = new RegistrationForm(studentController);
            registrationForm.Show();
        }

        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EmailPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Email.Text))
            {
                EmailPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Password.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void Placeholder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == EmailPlaceholder)
            {
                EmailPlaceholder.Visibility = Visibility.Collapsed;
                Email.Focus();
            }
            else if (sender == PasswordPlaceholder)
            {
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
                Password.Focus();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordPlaceholder != null)
            {
                PasswordPlaceholder.Visibility = string.IsNullOrEmpty(Password.Password) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

    }
}

