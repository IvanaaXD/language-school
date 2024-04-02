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
using System.Windows.Shapes;

using LangLang.DTO;
using LangLang.Controller;
using LangLang.Model;
using LangLang.Observer;
using LangLang.Model.Enums;

namespace LangLang.View.Student
{
    /// <summary>
    /// Interaction logic for AvailableCoursesForm.xaml
    /// </summary>
    public partial class AvailableCoursesForm : Window, IObserver
    {
        public class ViewModel
        {
            public ObservableCollection<CourseDTO> Courses { get; set; }

            public ViewModel()
            {
                Courses = new ObservableCollection<CourseDTO>();
            }

        }

        public ViewModel TableViewModel { get; set; }
        public CourseDTO SelectedCourse { get; set; }
        private StudentsController studentsController { get; set; }
        private TeacherController teacherController { get; set; }

        private int studentId { get; set; }
        private bool isSearchButtonClicked = false;


        public AvailableCoursesForm(int studentId)
        {
            InitializeComponent();
            TableViewModel = new ViewModel();
            studentsController = new StudentsController();
            teacherController = new TeacherController();
            this.studentId = studentId;

            languageComboBox.ItemsSource = Enum.GetValues(typeof(Language));
            levelComboBox.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            DataContext = this;
            studentsController.Subscribe(this);
            Update();
        }

        public void Update()
        {
            try
            {
                TableViewModel.Courses.Clear();
                var courses = GetFilteredCourses();
                
                if (courses != null)
                {
                    foreach (Course course in courses)
                        TableViewModel.Courses.Add(new CourseDTO(course));
                }
                else
                {
                    MessageBox.Show("No courses found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void btnSingUp_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            Update();
            isSearchButtonClicked = true;
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            isSearchButtonClicked = false;
            Update();
            ResetSearchElements();
        }

        private void ResetSearchElements()
        {
            languageComboBox.SelectedItem = null;
            levelComboBox.SelectedItem = null;
            startDateDatePicker.SelectedDate = null;
            durationTextBox.Text = string.Empty;
            onlineCheckBox.IsChecked = false;
        }
        private List<Course> GetFilteredCourses()
        {
            Language? selectedLanguage = (Language?) languageComboBox.SelectedItem;
            LanguageLevel? selectedLevel = (LanguageLevel?) levelComboBox.SelectedItem;
            DateTime? selectedStartDate = startDateDatePicker.SelectedDate;
            int selectedDuration = 0;
            if (!string.IsNullOrEmpty(durationTextBox.Text))
            {
                if (int.TryParse(durationTextBox.Text, out int duration))
                {
                    selectedDuration = duration;
                }
            }

            List<Course> studentsAvailableCourses = studentsController.GetAvailableCourses(studentId);
            List<Course> finalCourses = new List<Course>();

            if (isSearchButtonClicked)
            {
                bool isOnline = onlineCheckBox.IsChecked ?? false;
                List<Course> allFilteredCourses = teacherController.FindCoursesByCriteria(selectedLanguage, selectedLevel, selectedStartDate, selectedDuration, isOnline);

                foreach (Course course in allFilteredCourses)
                {
                    foreach (Course studentCourse in studentsAvailableCourses)
                    {
                        if (studentCourse.CourseID == course.CourseID && !finalCourses.Contains(course))
                        {
                            finalCourses.Add(course);
                        }
                    }
                }
            }
            else
            {
                foreach (Course studentCourse in studentsAvailableCourses)
                {
                    finalCourses.Add(studentCourse);
                }
            }
            return finalCourses;
        }
    }
}