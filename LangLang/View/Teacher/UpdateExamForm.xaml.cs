﻿using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model.DAO;
using LangLang.Model;
using LangLang.Model.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace LangLang.View.Teacher
{
    public partial class UpdateExamForm : Window
    {
        public ExamTermDTO ExamTerm { get; set; }
        public TeacherDTO Teacher { get; set; }

        private TeacherController teacherController;
        private readonly DirectorController directorController;
        private int teacherId;
        private int examId;

        public UpdateExamForm(TeacherController teacherController, DirectorController directorController, int teacherId, int examId)
        {
            ExamTerm examTerm = teacherController.GetExamTermById(examId);
            ExamTerm = new ExamTermDTO(examTerm);
            DataContext = ExamTerm;


            InitializeComponent();
            this.teacherController = teacherController;
            this.directorController = directorController;
            this.teacherId = teacherId;
            this.examId = examId;

            Teacher = new TeacherDTO(directorController.GetTeacherById(teacherId));
            TeacherDAO teacherDAO = new TeacherDAO();
            string languageAndLevel = teacherDAO.FindLanguageAndLevel(examTerm.CourseID);

            string[] parts = languageAndLevel.Split(',');
            languageAndLevel = parts[0].Trim() + " " + parts[1].Trim();
            languageComboBox.SelectedItem = languageAndLevel;

            List<Course> courses = teacherController.GetAllCourses();
            List<string> levelLanguageStr = new List<string>();

            foreach (Course course in courses)
            {
                if (Teacher.CoursesId.Contains(course.Id))
                {
                    string languageLevel = $"{course.Language} {course.Level}";
                    if (!levelLanguageStr.Contains(languageLevel))
                        levelLanguageStr.Add(languageLevel);
                }
            }

            languageComboBox.ItemsSource = levelLanguageStr;

            
            examDatePicker.SelectedDate = ExamTerm.ExamDate;
            examTimeTextBox.Text = ExamTerm.ExamDate.ToString("HH:mm"); 
            maxStudentsTextBox.Text = ExamTerm.MaxStudents.ToString();

        }
        private void PickLanguageAndLevel()
        {
            Language lang = Model.Enums.Language.German;
            LanguageLevel lvl = LanguageLevel.A1;

            if (languageComboBox.SelectedItem != null)
            {
                string selectedLanguageAndLevel = (string)languageComboBox.SelectedItem;
                string[] parts = selectedLanguageAndLevel.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    if (Enum.TryParse(parts[0], out Language language))
                        lang = language;
                    else
                        MessageBox.Show($"Invalid language: {parts[0]}");

                    if (Enum.TryParse(parts[1], out LanguageLevel level))
                        lvl = level;
                    else
                        MessageBox.Show($"Invalid level: {parts[1]}");
                }
                else
                {
                    MessageBox.Show("Invalid language and level format.");
                }

                FindCourseIdForExam(lang, lvl);

            }
        }
        private void FindCourseIdForExam(Language lang, LanguageLevel lvl)
        {
            TeacherDAO teacherDAO = new TeacherDAO();
            List<Course> courses = teacherDAO.GetAllCourses();

            foreach (Course course in courses)
            {
                if (course.Language == lang && course.Level == lvl)
                {
                    ExamTerm.CourseID = course.Id;
                    break;
                }
            }
        }
        private void PickDataFromDatePicker()
        {
            if (examDatePicker.SelectedDate.HasValue && !string.IsNullOrWhiteSpace(examTimeTextBox.Text))
            {
                DateTime startDate = examDatePicker.SelectedDate.Value.Date;
                DateTime startTime;
                if (DateTime.TryParseExact(examTimeTextBox.Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                {
                    ExamTerm.ExamDate = startDate.Add(startTime.TimeOfDay);
                }
                else
                {
                    MessageBox.Show("Please enter a valid start time (HH:mm).");
                }
            }
            else
            {
                MessageBox.Show("Please select a valid start date and time.");
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            PickDataFromDatePicker();
            PickLanguageAndLevel();
            if (ExamTerm.IsValid)
            {
                teacherController.UpdateExamTerm(ExamTerm.ToExamTerm());
                Close();
            }
            else
            {
                MessageBox.Show("Exam Term can not be updated. Not all fields are valid.");
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
