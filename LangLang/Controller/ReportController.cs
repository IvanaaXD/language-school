﻿using LangLang.Domain.IRepository;
using LangLang.Domain.Model;
using System;
using System.Collections.Generic;
using LangLang.Domain.Model.Enums;
using System.Linq;
using LangLang.Domain.Model.Reports;

namespace LangLang.Controller
{
    public class ReportController
    {
        public void GenerateReport(IReportGenerator reportGenerator)
        {
            reportGenerator.GenerateReport();
        }
        /* private readonly IDirectorRepository _directors;
         private readonly ITeacherRepository? _teachers;
         private readonly IExamTermRepository? _examTerms;
         private readonly IStudentGradeRepository? _studentGrades;
         private readonly IPenaltyPointRepository? _penaltyPoints;
         private readonly ICourseGradeRepository? _courseGrade;
         private readonly DirectorController? _directorController;
         private readonly ExamTermController? _examTermController;
         private readonly StudentsController? _studentController;
         private readonly CourseController? _courseController;
         private readonly ExamTermGradeController? _examTermGradeController;

         public ReportController()
         {
             _directors = Injector.CreateInstance<IDirectorRepository>();
             _teachers = Injector.CreateInstance<ITeacherRepository>();
             _examTerms = Injector.CreateInstance<IExamTermRepository>();
             _studentGrades = Injector.CreateInstance<IStudentGradeRepository>();
             _penaltyPoints = Injector.CreateInstance<IPenaltyPointRepository>();
             _courseGrade = Injector.CreateInstance<ICourseGradeRepository>();

             _examTermController = Injector.CreateInstance<ExamTermController>();
             _courseController = Injector.CreateInstance<CourseController>();
             _directorController = Injector.CreateInstance<DirectorController>();
             _studentController = Injector.CreateInstance<StudentsController>();
             _examTermGradeController = Injector.CreateInstance<ExamTermGradeController>();
         }

         public void GenerateFirstReport()
         {
             PdfGenerator pdfGenerator = new PdfGenerator("..\\..\\..\\Data\\report1.pdf");
             pdfGenerator.AddTitle("Number of penalty points in the last year");
             pdfGenerator.AddNewLine();
             pdfGenerator.AddTable(_courseController.GetPenaltyPointsLastYearPerCourse(), "Course", "Penalties");
             pdfGenerator.AddNewPage();
             pdfGenerator.AddTitle("Average points of students by penalties");
             pdfGenerator.AddNewLine();
             for (int i = 0; i <= 3; i++)
             {
                 pdfGenerator.AddSubtitle("Number of penalty points: " + i);
                 pdfGenerator.AddTable(_studentController.GetStudentsAveragePointsPerPenalty()[i], "Student", "Average points");
             }
             pdfGenerator.SaveAndClose();
         }
         public void GenerateSecondReport()
         {
             PdfGenerator pdfGenerator = new PdfGenerator("..\\..\\..\\Data\\report2.pdf");
             pdfGenerator.AddTitle("Average teacher and course grades in the past year");
             pdfGenerator.AddNewLine();

             pdfGenerator.AddTupleTable(GetTeacherCourseReport(), "Course", "Teacher Grade", "Knowledge Grade", "Activity Grade");
             pdfGenerator.SaveAndClose();
         }
         public void GenerateThirdReport()
         {
             PdfGenerator pdfGenerator = new PdfGenerator("..\\..\\..\\Data\\report3.pdf");
             pdfGenerator.AddTitle("Statistics on the points of passed exams in the last year");
             pdfGenerator.AddNewLine();

             pdfGenerator.AddTable(GetPartsOfExamReport(), "Each part of exam", "Average points");

             pdfGenerator.AddNewLine();
             pdfGenerator.AddTitle("Course statistics in the last year");
             pdfGenerator.AddNewLine();

             pdfGenerator.AddDifTypeTupleTable(GetStudentsCourseReport(), "Course", "Participants", "Passed", "Success Rate");

             pdfGenerator.SaveAndClose();
         }

         public void GenerateFourthReport()
         {
             PdfGenerator pdfGenerator = new PdfGenerator("..\\..\\..\\Data\\report4.pdf");

             pdfGenerator.AddTitle("Statistics on created courses in the last year");
             pdfGenerator.AddNewLine();
             pdfGenerator.AddTable(GetNumberOfCourses(), "Languages", "Number of courses");

             pdfGenerator.AddNewPage();

             pdfGenerator.AddTitle("Statistics on created exams in the last year");
             pdfGenerator.AddNewLine();
             pdfGenerator.AddTable(GetNumberOfExamTerms(), "Languages", "Number of exams");

             pdfGenerator.AddNewPage();

             pdfGenerator.AddTitle("Statistics on penalty points");
             pdfGenerator.AddNewLine();
             pdfGenerator.AddTable(GetNumberOfPenaltyPoints(), "Languages", "Average number of penalty points");

             pdfGenerator.AddNewPage();

             pdfGenerator.AddTitle("Statistics on exam points");
             pdfGenerator.AddNewLine();
             pdfGenerator.AddTable(GetNumberOfPoints(), "Languages", "Average number of points on exams");

             pdfGenerator.SaveAndClose();
         }

         public Dictionary<Course, (double, double, double)> GetTeacherCourseReport()
         {
             Dictionary<Course, (double, double, double)> finalCourses = new();
             Dictionary<Course, double> averageTeacherGrade = GetAverageTeacherGradeByCourse();
             Dictionary<Course, double> averageKnowledgeGrade = CalculateAverageGrade("knowledge");
             Dictionary<Course, double> averageActivityGrade = CalculateAverageGrade("activity");
             List<Course> lastYearCourses = _courseController.GetCoursesLastYear();

             foreach (Course course in lastYearCourses)
             {
                 if (_courseController.HasGradingPeriodStarted(course))
                 {
                     finalCourses[course] = (averageTeacherGrade[course], averageKnowledgeGrade[course], averageActivityGrade[course]);
                 }
             }

             return finalCourses;
         }

         public Dictionary<string, double> GetPartsOfExamReport()
         {
             Dictionary<string, double> examAverageResult = new();
             examAverageResult["reading"] = CalculateAveragePoints("reading");
             examAverageResult["listening"] = CalculateAveragePoints("listening");
             examAverageResult["speaking"] = CalculateAveragePoints("speaking");
             examAverageResult["writing"] = CalculateAveragePoints("writing");
             return examAverageResult;
         }

         public Dictionary<Course, (int, int, double)> GetStudentsCourseReport()
         {
             Dictionary<Course, (int, int, double)> finalCourses = new();
             List<Course> lastYearCourses = _courseController.GetCoursesLastYear();

             foreach (Course course in lastYearCourses)
             {
                 if (_courseController.HasGradingPeriodStarted(course))
                 {
                     int attendedCount = GetAttendedCount(course.Id);
                     int passedCount = GetPassedCount(course.Id);
                     finalCourses[course] = (attendedCount, passedCount, CalculatePassPercentage(passedCount, attendedCount));
                 }
             }

             return finalCourses;
         }

         public Dictionary<Language, T> GetLanguages<T>() where T : struct
         {
             Dictionary<Language, T> languages = new Dictionary<Language, T>();
             var langs = Enum.GetValues(typeof(Language)).Cast<Language>().ToList();

             foreach (Language language in langs)
                 if (language != Language.NULL)
                     languages.Add(language, default(T));

             return languages;
         }

         public Dictionary<Language, int> GetNumberOfCourses()
         {
             Dictionary<Language, int> numberOfCourses = GetLanguages<int>();
             var courses = _courseController.FindCoursesByDate(DateTime.Today.AddYears(-1));

             foreach (var course in courses)
                 numberOfCourses[course.Language] += 1;

             return numberOfCourses;
         }

         public Dictionary<Language, int> GetNumberOfExamTerms()
         {
             Dictionary<Language, int> numberOfExamTerms = GetLanguages<int>();
             var examTerms = _examTermController.FindExamTermsByDate(DateTime.Today.AddYears(-1));

             foreach (var examTerm in examTerms)
                 numberOfExamTerms[examTerm.Language] += 1;

             return numberOfExamTerms;
         }

         public Dictionary<Language, double> GetNumberOfPenaltyPoints()
         {
             Dictionary<Language, double> numberOfPenaltyPoints = GetLanguages<double>();
             var penaltyPoints = _penaltyPoints.GetAllPenaltyPoints();

             if (penaltyPoints.Count == 0)
                 return numberOfPenaltyPoints;

             foreach (var number in numberOfPenaltyPoints)
             {
                 List<LanguageLevel> levels = new List<LanguageLevel>();
                 int sum = 0;

                 foreach (var penaltyPoint in penaltyPoints)
                 {
                     var course = _courseController.GetById(penaltyPoint.CourseId);

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
             Dictionary<Language, double> numberOfPoints = GetLanguages<double>();
             var examTerms = _examTerms.GetAllExamTerms();

             foreach (var number in numberOfPoints)
             {
                 List<LanguageLevel> levels = new List<LanguageLevel>();
                 int sum = 0;

                 foreach (var examTerm in examTerms)
                 {
                     var grades = _examTermGradeController.GetExamTermGradeByExam(examTerm.ExamID);

                     if (examTerm.Language == number.Key)
                     {
                         if (!levels.Contains(examTerm.Level))
                             levels.Add(examTerm.Level);

                         foreach (var grade in grades)
                             sum += grade.ListeningPoints + grade.ReadingPoints + grade.SpeakingPoints + grade.WritingPoints;
                     }
                 }

                 double averageNumber = 0;
                 if (sum != 0)
                     averageNumber = sum / levels.Count();

                 numberOfPoints[number.Key] = averageNumber;
             }
             return numberOfPoints;
         }

         public double CalculateAveragePoints(string typeOfPoints)
         {
             int result = 0, count = 0;
             List<ExamTermGrade> examGrades = _examTermGradeController.GetAllExamTermGrades();
             foreach (ExamTermGrade grade in examGrades)
             {
                 ExamTerm exam = _examTerms.GetExamTermById(grade.ExamId);
                 if (exam == null)
                     continue;
                 else if (exam.ExamTime >= DateTime.Now.AddYears(-1))
                 {
                     if (typeOfPoints == "listening")
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

         public Dictionary<Course, double> GetAverageTeacherGradeByCourse()
         {
             Dictionary<Course, double> finalResult = new();
             foreach (Course course in _courseController.GetCoursesLastYear())
             {

                 int result = 0;
                 Teacher teacher = _directorController.GetTeacherByCourse(course.Id);
                 if (teacher == null)
                     continue;
                 List<StudentGrade> teachersGrades = _studentGrades.GetStudentGradesByTeacherCourse(teacher.Id, course.Id);

                 foreach (StudentGrade studentGrade in teachersGrades)
                     result += studentGrade.Value;

                 if (teachersGrades.Count == 0)
                     finalResult[course] = 0;
                 else
                     finalResult[course] = result / teachersGrades.Count;

             }
             return finalResult;
         }

         public Dictionary<Course, double> CalculateAverageGrade(string typeOfGrade)
         {
             Dictionary<Course, double> finalResult = new();
             foreach (Course course in _courseController.GetCoursesLastYear())
             {
                 int result = 0;

                 List<CourseGrade> studentGrades = _courseGrade.GetCourseGradesByCourse(course.Id);

                 foreach (CourseGrade grade in studentGrades)
                 {
                     if (typeOfGrade == "knowledge")
                         result += grade.StudentKnowledgeValue;
                     else
                         result += grade.StudentActivityValue;
                 }
                 if (result == 0)
                     finalResult[course] = 0;
                 else
                     finalResult[course] = result / studentGrades.Count;
             }
             return finalResult;
         }

         public int GetAttendedCount(int courseId)
         {
             Course course = _courseController.GetById(courseId);
             return course.CurrentlyEnrolled;
         }

         public int GetPassedCount(int courseId)
         {
             int count = 0;
             List<CourseGrade> grades = _courseGrade.GetCourseGradesByCourse(courseId);
             foreach (CourseGrade grade in grades)
             {
                 if (grade.StudentKnowledgeValue >= 6 && grade.StudentActivityValue >= 6)
                     count++;
             }
             return count;
         }
         public double CalculatePassPercentage(int passedCount, int attendedCount)
         {
             if (attendedCount == 0)
             {
                 return 0;
             }
             return (double)passedCount / attendedCount * 100;
         }*/
    }
}