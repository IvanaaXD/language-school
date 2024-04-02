﻿using LangLang.Controller;
using LangLang.Model;
using LangLang.Model.DAO;
using LangLang.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace LangLang.DTO
{
    public class ExamTermDTO : INotifyPropertyChanged, IDataErrorInfo
    {
        private int examID;
        private int courseID;
        private DateTime examDate; // examTime -> examDate
        private string examTime;
        private int maxStudents;
        private int currentlyAttending;
        private TeacherController teacherController = new TeacherController();
        private string languageAndLevel;

        public List<string> LanguageAndLevelValues
        {
            get
            {
                List<string> languageLevelNames = new List<string>();

                TeacherDAO teacherDAO = new TeacherDAO();
                List<Course> courses = teacherDAO.GetAllCourses();

                foreach (Course course in courses)
                {
                        languageLevelNames.Add($"{course.Language} {course.Level}"); 
                }

                return languageLevelNames;
            }
        }
        public int ExamID
        {
            get { return examID; }
            set { SetProperty(ref examID, value); }
        }

        public int CourseID
        {
            get { return courseID; }
            set { SetProperty(ref courseID, value); }
        }

        public DateTime ExamDate
        {
            get { return examDate; }
            set { SetProperty(ref examDate, value); }
        }

        public string ExamTime
        {
            get { return examTime; }
            set { SetProperty(ref examTime, value); }
        }

        public int MaxStudents
        {
            get { return maxStudents; }
            set { SetProperty(ref maxStudents, value); }
        }

        public int CurrentlyAttending
        {
            get { return currentlyAttending; }
            set { SetProperty(ref currentlyAttending, value); }
        }

        public string LanguageAndLevel
        {
            get { return languageAndLevel; }
            set { SetProperty(ref languageAndLevel, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public string Error => null;

        private Regex _TimeRegex = new Regex(@"^(?:[01]\d|2[0-3]):(?:[0-5]\d)$");
        
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "ExamDate":
                        if (ExamDate < DateTime.Today)
                            return "Exam date cannot be in the past";
                        break;
                    /*
                    case "ExamTime":
                        if (!_TimeRegex.IsMatch(ExamTime))
                            return "Format is not good. Try again.";
                        break;
                    */
                     case "CurrentlyAttending":
                        if (CurrentlyAttending < 0 ||  (CurrentlyAttending > MaxStudents))
                            return "Number of attending students on the exam can't be less than 0 or greater than max number of students.";
                        break;
                      
                    case "MaxStudents":
                        if (MaxStudents <= 0)
                            return "Max number of students must be above 0 for exam";
                        if (MaxStudents > 550)
                            return "Max number of students must be <= 550 on the exam";
                        break;

                }
                return null;
            }
        }

        public bool IsValid
        {
            get
            {
                if (ExamDate < DateTime.Today)
                    return false;
                if (!_TimeRegex.IsMatch(ExamTime))
                    return false;
                if ((ExamDate - DateTime.Now).TotalDays < 14)
                        return false;
                if (!teacherController.CheckExamOverlap(ExamID, ExamDate)) // =====================
                    return false;
                if (CurrentlyAttending < 0 || (CurrentlyAttending > MaxStudents))
                    return false;
                if (MaxStudents <= 0)
                    return false;
                if (MaxStudents > 550)
                    return false;
                    
                return true;
            }
        }

        public ExamTerm ToExamTerm()
        {
            TimeSpan timeSpan = TimeSpan.Parse(examTime);

            DateTime combinedDateTime = examDate.Date + timeSpan;

            return new ExamTerm
            {
                ExamID = ExamID,
                CourseID = CourseID,
                ExamTime = combinedDateTime,
                MaxStudents = MaxStudents,
                CurrentlyAttending = CurrentlyAttending
            };
        }

        public ExamTermDTO()
        {

        }

        public ExamTermDTO(ExamTerm examTerm)
        {
            examID = examTerm.ExamID;
            courseID = examTerm.CourseID;
            examDate = examTerm.ExamTime; // preimenuj u klasi
            maxStudents = examTerm.MaxStudents;
            currentlyAttending = examTerm.CurrentlyAttending;

            TeacherDAO teacherDAO = new TeacherDAO();
            languageAndLevel = teacherDAO.FindLanguageAndLevel(courseID);
        }
    }
}