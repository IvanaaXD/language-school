﻿using LangLang.Observer;
using System;
using System.Collections.Generic;
using LangLang.Repository;
using LangLang.Domain.Model;
using LangLang.Domain.Model.Enums;
using System.Linq;
using LangLang.Domain.IRepository;
using System.Windows.Input;

namespace LangLang.Controller
{
    public class DirectorController
    {
        private readonly IDirectorRepository _directors;
        private readonly TeacherRepository? _teachers;
        private readonly ExamTermDAO? _examTerms;
        private readonly StudentGradeRepository? _studentGrades;
        private readonly PenaltyPointDAO? _penaltyPoints;
        private readonly CourseController? _courseController;
        private readonly TeacherController? _teacherController;
        private readonly ExamTermController? _examTermController;
        private readonly ExamTermGradeRepository? _examTermGrades;

        public DirectorController(IDirectorRepository directors)
        {
            _directors = directors ?? throw new ArgumentNullException(nameof(directors));
            _teachers = new TeacherRepository();
            _examTerms = new ExamTermDAO();
            _studentGrades = new StudentGradeRepository();
            _penaltyPoints = new PenaltyPointDAO();
            _examTermGrades = new ExamTermGradeRepository();

            _teacherController = new TeacherController();
            _examTermController = new ExamTermController(new ExamTermRepository(), new TeacherController());
            _courseController = new CourseController(new CourseRepository(), new TeacherController());

        }
        public DirectorController()
        {
            _directors = Injector.CreateInstance<IDirectorRepository>();
            _teachers = new TeacherRepository();
            _examTerms = new ExamTermDAO();
            _studentGrades = new StudentGradeRepository();
            _penaltyPoints = new PenaltyPointDAO();
            _examTermGrades = new ExamTermGradeRepository();

            _teacherController = new TeacherController();
            _examTermController = new ExamTermController(new ExamTermRepository(), new TeacherController());
            _courseController = new CourseController(new CourseRepository(), new TeacherController());

        }

        public Director? GetDirector()
        {
            return _directors.GetDirector();
        }

        public Teacher? GetTeacherById(int teacherId)
        {
            return _directors.GetTeacherById(teacherId);
        }

        public List<Teacher> GetAllTeachers()
        {
            return _directors.GetAllTeachers();
        }

        public void Add(Teacher teacher)
        {
            _directors.AddTeacher(teacher);
        }

        public void Update(Teacher teacher)
        {
            _directors.UpdateTeacher(teacher);
        }

        public void Delete(int teacherId)
        {
            Teacher teacher = GetTeacherById(teacherId);

            DeleteCoursesByTeacher(teacher);
            DeleteExamTermsByTeacher(teacher);
            _directors.RemoveTeacher(teacherId);
        }

        public void Subscribe(IObserver observer)
        {
            _directors.Subscribe(observer);
        }

        public void DeleteCoursesByTeacher(Teacher teacher)
        {
            var courses = _courseController.GetAllCourses();
            var teacherCourses = teacher.CoursesId;

            foreach (var course in courses)
            {
                if (teacherCourses.Contains(course.Id) && course.StartDate > DateTime.Today.Date)
                    teacherCourses.Remove(course.Id);
            }

            teacher.CoursesId = teacherCourses;
            Update(teacher);
        }

        public void DeleteExamTermsByTeacher(Teacher teacher)
        {
            var examTerms = _examTermController.GetAllExamTerms();
            var courses = _courseController.GetAllCourses();
            var teacherExamTerms = teacher.CoursesId;

            foreach (var examTerm in examTerms)
            {
                var course = _courseController.GetCourseById(examTerm.CourseID);
                if (teacherExamTerms.Contains(examTerm.CourseID) &&  course.StartDate > DateTime.Today.Date)
                    teacherExamTerms.Remove(course.Id);
            }

            teacher.CoursesId = teacherExamTerms;
            Update(teacher);
        }

        public List<Teacher> FindTeachersByCriteria(Language language, LanguageLevel levelOfLanguage, DateTime startedWork)
        {
            List<Teacher> teachers = GetAllTeachers();

            var filteredTeachers = teachers.Where(teacher =>
                (language == Language.NULL || (teacher.Languages != null && teacher.Languages.Contains(language))) &&
                (levelOfLanguage == LanguageLevel.NULL || (teacher.LevelOfLanguages != null && teacher.LevelOfLanguages.Contains(levelOfLanguage))) &&
                (startedWork == DateTime.MinValue || (teacher.StartedWork.Date >= startedWork.Date))
            ).ToList();

            return filteredTeachers;
        }

        public Teacher? GetTeacherByCourse(int courseId)
        {
            foreach (Teacher teacher in GetAllTeachers())
            {
                if (teacher.CoursesId != null)
                {
                    foreach (int teacherCourseId in teacher.CoursesId)
                        if (teacherCourseId == courseId) return teacher;
                }
            }
            return null;
        }

        public List<Course> GetAvailableCourses(int teacherId)
        {
            Teacher? teacher = GetTeacherById(teacherId);
            List<Course>? allCourses = _teachers?.GetAllCourses();
            List<int>? allTeacherCourses = teacher?.CoursesId;
            DateTime currentTime = DateTime.Now;

            List<Course> availableCourses = new List<Course>();

            if (allCourses != null && allTeacherCourses != null)
            {
                foreach (Course course in allCourses)
                {
                    if (allTeacherCourses.Contains(course.Id))
                    {
                        availableCourses.Add(course);
                    }
                }
            }

            return availableCourses;
        }

        public void RemoveCourseFromList(int teacherId, int courseId)
        {
            Teacher? teacher = GetTeacherById(teacherId);
            teacher?.CoursesId?.Remove(courseId);
            Update(teacher);
        }
      
        public void GenerateFirstReport()
        {
            StudentsController studentController = Injector.CreateInstance<StudentsController>();
            PdfGenerator pdf = new PdfGenerator();
            pdf.AddTitle("Number of penalty points in the last year");
            pdf.AddTable(_courseController.GetPenaltyPointsLastYearPerCourse());
            pdf.AddPage();
            pdf.AddTitle("Average points of students by penalties");
            for(int i = 0; i < 3; i++)
            {
                pdf.AddSubtitle("Number of penalty points: " + i);
                pdf.AddTable(studentController.GetStudentsAveragePointsPerPenalty()[i]);
            }
           
            pdf.Save(@"C:\\Users\\Milan\\Desktop\\Projekat\\LangLang\\data\\nekipdf.pdf");
        }
      
        public int GetAverageTeacherGrade(int teacherId)
        {
            int result = 0;
            List<StudentGrade> teachersGrades = _studentGrades.GetStudentGradeByTeacher(teacherId);
            foreach (StudentGrade grade in teachersGrades)
            {
                result += grade.Value;
            }
            return result == 0 ? 0 : result / teachersGrades.Count;
        }

        public (Dictionary<Language, int> numberOfCourses, Dictionary<Language, int> numberOfExamTerms, Dictionary<Language, double> penaltyPoints, Dictionary<Language, double> values) GetLanguageReport()
        {
            Dictionary<Language, int> numberOfCourses = GetNumberOfCourses();
            Dictionary<Language, int> numberOfExamTerms = GetNumberOfExamTerms();
            Dictionary<Language, double> penaltyPoints = GetNumberOfPenaltyPoints();
            Dictionary<Language, double> values = GetNumberOfPoints();

            return (numberOfCourses, numberOfExamTerms, penaltyPoints, values);
        }

        public Dictionary<Language, int> GetLanguagesInt()
        {
            Dictionary<Language, int> languages = new Dictionary<Language, int>();
            var langs = Enum.GetValues(typeof(Language)).Cast<Language>().ToList();

            foreach (Language language in langs)
            {
                languages.Add(language, 0);
            }

            return languages;
        }

        public Dictionary<Language, double> GetLanguagesDouble()
        {
            Dictionary<Language, double> languages = new Dictionary<Language, double>();
            var langs = Enum.GetValues(typeof(Language)).Cast<Language>().ToList();

            foreach (Language language in langs)
            {
                languages.Add(language, 0);
            }

            return languages;
        }

        public Dictionary<LanguageLevel, int> GetLanguageLevels()
        {
            Dictionary<LanguageLevel, int> levels = new Dictionary<LanguageLevel, int>();
            var languages = Enum.GetValues(typeof(LanguageLevel)).Cast<LanguageLevel>().ToList();

            foreach (LanguageLevel language in languages)
            {
                levels.Add(language, 0);
            }

            return levels;
        }

        public Dictionary<Language, int> GetNumberOfCourses()
        {
            Dictionary<Language, int> numberOfCourses = GetLanguagesInt();
            var courses = _courseController.FindCoursesByDate(DateTime.Today.AddYears(-1));

            foreach (var course in courses)
                numberOfCourses[course.Language] += 1;

            return numberOfCourses;
        }

        public Dictionary<Language, int> GetNumberOfExamTerms()
        {
            Dictionary<Language, int> numberOfExamTerms = GetLanguagesInt();
            var examTerms = _examTermController.FindExamTermsByDate(DateTime.Today.AddYears(-1));

            foreach (var examTerm in examTerms)
            {
                var course = _teachers.GetCourseById(examTerm.CourseID);
                numberOfExamTerms[course.Language] += 1;
            }

            return numberOfExamTerms;
        }

        public Dictionary<Language, double> GetNumberOfPenaltyPoints()
        {
            Dictionary<Language, double> numberOfPenaltyPoints = GetLanguagesDouble();

            var penaltyPoints = _penaltyPoints.GetAllPenaltyPoints();
            if (penaltyPoints.Count == 0)
                return numberOfPenaltyPoints;

            foreach (var number in numberOfPenaltyPoints)
            {
                List<LanguageLevel> levels = new List<LanguageLevel>();
                int sum = 0;

                foreach (var penaltyPoint in penaltyPoints)
                {
                    var course = _teachers.GetCourseById(penaltyPoint.CourseId);

                    if (course.Language == number.Key)
                    {
                        if (!levels.Contains(course.Level))
                            levels.Add(course.Level);
                        sum += 1;
                    }
                }

                double averageNumber = 0;
                if (sum != 0)
                    averageNumber = sum / levels.Count();

                numberOfPenaltyPoints[number.Key] = averageNumber;
            }

            return numberOfPenaltyPoints;
        }

        public Dictionary<Language, double> GetNumberOfPoints()
        {
            Dictionary<Language, double> numberOfPoints = GetLanguagesDouble();

            var examTerms = _examTerms.GetAllExamTerms();

            foreach (var number in numberOfPoints)
            {
                List<LanguageLevel> levels = new List<LanguageLevel>();
                int sum = 0;

                foreach (var examTerm in examTerms)
                {
                    var course = _teachers.GetCourseById(examTerm.CourseID);
                    var grades = _examTermGrades.GetExamTermGradeByExam(examTerm.ExamID);

                    if (course.Language == number.Key)
                    {
                        if (!levels.Contains(course.Level))
                            levels.Add(course.Level);

                        foreach (var grade in grades)
                        {
                            sum += grade.ListeningPoints + grade.ReadingPoints + grade.SpeakingPoints + grade.WritingPoints;
                        }
                    }
                }

                double averageNumber = 0;
                if (sum != 0)
                    averageNumber = sum / levels.Count();

                numberOfPoints[number.Key] = averageNumber;
            }

            return numberOfPoints;
        }

        public double GetAverageReadingPointsLastYear()
        {
            return CalculateAveragePoints("reading");
        }
        public double GetAverageSpeakingPointsLastYear()
        {
            return CalculateAveragePoints("speaking");
        }
        public double GetAverageWritingPointsLastYear()
        {
            return CalculateAveragePoints("writing");
        }
        public double GetAverageListeningPointsLastYear()
        {
            return CalculateAveragePoints("listening");
        }
        public double CalculateAveragePoints(string typeOfPoints)
        {
            int result = 0, count = 0;
            List<ExamTermGrade> examGrades = _examTermGrades.GetAllExamTermGrades();
            foreach (ExamTermGrade grade in examGrades)
            {
                ExamTerm exam = _examTerms.GetExamTermById(grade.ExamId);
                if (exam.ExamTime >= DateTime.Now.AddYears(-1))
                {
                    if(typeOfPoints == "listening")
                        result += grade.ListeningPoints;
                    else if (typeOfPoints == "speaking")
                        result += grade.SpeakingPoints;
                    else if (typeOfPoints == "writing")
                        result += grade.WritingPoints;
                    else if (typeOfPoints == "reading")
                        result += grade.ReadingPoints;

                    count++;
                }
            }
            return result == 0 ? 0 : result / count;
        }
        public int GetAttendedCount(int courseId)
        {
            Course course = _teachers.GetCourseById(courseId);
            return course.CurrentlyEnrolled;
        }
        public int GetPassedCount(int courseId)
        {
            int count = 0;  
            Course course = _teachers.GetCourseById(courseId);
            List<ExamTermGrade> grades = _examTermGrades.GetAllExamTermGrades();
            foreach(ExamTermGrade grade in  grades) { 
                if(course.ExamTerms.Contains(grade.ExamId) && grade.Value >= 6)
                {
                    count++;
                }
            }
            return count;
        }
        public double CalculatePassPercentage(int passedCount, int attendedCount)
        {
            return (passedCount / attendedCount) * 100;
        }
    }
}
