﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LangLang.Storage;
using LangLang.Observer;
using System.Printing;
using System.Windows.Input;
using System.Windows;
using System.Linq.Expressions;

namespace LangLang.Model.DAO
{
    public class StudentDAO : Subject
    {
        private readonly List<Student> _students;
        private readonly Storage<Student> _storage;
        private TeacherDAO teacherDAO;

        public StudentDAO()
        {
            _storage = new Storage<Student>("students.csv");
            _students = _storage.Load();
            teacherDAO = new TeacherDAO();
        }

        private int GenerateId()
        {
            if (_students.Count == 0) return 0;
            return _students.Last().Id + 1;
        }

        public Student AddStudent(Student student)
        {
            student.Id = GenerateId();
            _students.Add(student);
            _storage.Save(_students);
            NotifyObservers();
            return student;
        }

        public Student? UpdateStudent(Student student)
        {
            Student? oldStudent = GetStudentById(student.Id);
            if (oldStudent == null) return null;

            oldStudent.FirstName = student.FirstName;
            oldStudent.LastName = student.LastName;
            oldStudent.Gender = student.Gender;
            oldStudent.DateOfBirth = student.DateOfBirth;
            oldStudent.PhoneNumber = student.PhoneNumber;
            oldStudent.Email = student.Email;
            oldStudent.Password = student.Password;
            oldStudent.EducationLevel = student.EducationLevel;
            oldStudent.PenaltyPoints = student.PenaltyPoints;
            oldStudent.ActiveCourseId = student.ActiveCourseId;
            oldStudent.PassedExamsIds = student.PassedExamsIds;
            oldStudent.PendingCoursesIds = student.PendingCoursesIds;
            oldStudent.RegisteredExamsIds = student.RegisteredExamsIds;

            _storage.Save(_students);
            NotifyObservers();
            return oldStudent;
        }

        public Student? RemoveStudent(int id)
        {
            Student? student = GetStudentById(id);
            if (student == null) return null;

            DeleteStudentCoursesAndExams(student);
            _students.Remove(student);
            _storage.Save(_students);
            NotifyObservers();
            return student;
        }

        private void DeleteStudentCoursesAndExams(Student student)
        {
            if (student.ActiveCourseId != -1)
                teacherDAO.DecrementCourseCurrentlyEnrolled(student.ActiveCourseId);

            foreach(int examTermId in student.RegisteredExamsIds) {
                teacherDAO.DecrementExamTermCurrentlyAttending(examTermId);
            }
        }

        public Student? GetStudentById(int id)
        {
            return _students.Find(v => v.Id == id);
        }

        public List<Student> GetAllStudents()
        {
            return _students;
        }

        public List<Course> GetAvailableCourses(int studentId)
        {
            Student student = GetStudentById(studentId);
            List<Course> allCourses = teacherDAO.GetAllCourses();
            List<int> passedCoursesIds = GetPassedCourses(student);
            List<int> courseIdsByRegisteredExams = GetCourseIdsByRegisteredExams(student);
            DateTime currentTime = DateTime.Now;

            List<Course> availableCourses = new List<Course>();

            foreach(Course course in allCourses) {
                if(!passedCoursesIds.Contains(course.CourseID) && 
                   !courseIdsByRegisteredExams.Contains(course.CourseID) &&
                   !student.PendingCoursesIds.Contains(course.CourseID) && 
                   !student.PendingExamCoursesIds.Contains(course.CourseID) &&
                   (course.CurrentlyEnrolled < course.MaxEnrolledStudents) && 
                   (course.StartDate - currentTime).TotalDays > 6)
                {
                    availableCourses.Add(course);
                }
            }

            return availableCourses;
        }

        private List<int> GetCourseIdsByRegisteredExams(Student student)
        {
            List<int> courses = new List<int>();
            foreach (int examTermId in student.RegisteredExamsIds)
            {
                ExamTerm examTerm = teacherDAO.GetExamTermById(examTermId);
                if (!courses.Contains(examTerm.CourseID))
                    courses.Add(examTerm.CourseID);
            }
            return courses;
        }
        private List<int> GetPassedCourses(Student student)
        {
            List<int> courses = new List<int>();
            foreach(int examTermId in student.PassedExamsIds)
            {
                ExamTerm examTerm = teacherDAO.GetExamTermById(examTermId);
                courses.Add(examTerm.CourseID);

            }
            return courses;
        }

        public List<ExamTerm> GetAvailableExamTerms(int studentId)
        {
            Student student = GetStudentById(studentId);
            DateTime currentTime = DateTime.Now;

            List<ExamTerm> availableExamTerms = new List<ExamTerm>();

            foreach (int courseId in student.PendingExamCoursesIds)
            {
                List<ExamTerm> examTerms = GetExamTermsByCourse(courseId);
                foreach(ExamTerm examTerm in examTerms)
                {
                    if ((examTerm.CurrentlyAttending < examTerm.MaxStudents) &&
                        (examTerm.ExamTime - currentTime).TotalDays > 30)
                    {
                        availableExamTerms.Add(examTerm);
                    }
                }
            }

            return availableExamTerms;
        }

        private List<ExamTerm> GetExamTermsByCourse(int courseId)
        {
            Course course = teacherDAO.GetCourseById(courseId);
            return teacherDAO.FindExamTermsByCriteria(course.Language, course.Level,null);
        }

        public bool IsEmailUnique(string email)
        {
            foreach(Student student in _students)
            {
                if (student.Email.Equals(email)) return false;
            }
            return true;
        }
    }
}