﻿using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using System;
using System.ComponentModel;
using System.Windows;
using LangLang.Model.Enums;

namespace LangLang.View.Teacher
{
    public partial class CourseRejectionForm : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private MailDTO _mail;
        public MailDTO Mail
        {
            get { return _mail; }
            set
            {
                _mail = value;
                OnPropertyChanged(nameof(Mail));
            }
        }

        private Course course;
        private Model.Teacher teacher;
        private Model.Student student;
        private TeacherController teacherController;
        private StudentsController studentController;

        public CourseRejectionForm(Course course, Model.Teacher teacher, Model.Student student, TeacherController teacherController, StudentsController studentController)
        {
            InitializeComponent();
            DataContext = this;
            Mail = new MailDTO();

            this.course = course;
            this.teacherController = teacherController;
            this.studentController = studentController;
            this.teacher = teacher;
            this.student = student;

            firstNameTextBlock.Text = student.FirstName;
            lastNameTextBlock.Text = student.LastName;
            mailBodyTextBlock.Text = " ";
        }

        public void SendRejection_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(mailBodyTextBlock.Text))
            {
                Mail.Sender = teacher;
                Mail.Recevier = student;
                Mail.TypeOfMessage = TypeOfMessage.DenyEnterCourseRequestMessage;
                Mail.DateOfMessage = DateTime.Now;
                Mail.Message = mailBodyTextBlock.Text;
                Mail.Answered = false;

                // here it gets sent

                student.RegisteredCoursesIds.Remove(course.Id);
                studentController.Update(student);

                Close();
            }
            else
            {
                MessageBox.Show("Please name the reason for rejecting the student from the course.");
            }
            
        }
    }
}
