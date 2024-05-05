﻿using System.Collections.Generic;
using System.Linq;
using LangLang.Observer;
using LangLang.Storage;

namespace LangLang.Model.DAO
{
    public class ExamTermGradeDAO : Subject
    {
        private readonly List<ExamTermGrade> _grades;
        private readonly Storage<ExamTermGrade> _storage;

        public ExamTermGradeDAO()
        {
            _storage = new Storage<ExamTermGrade>("examTermGrades.csv");
            _grades = _storage.Load();
        }

        private int GenerateId()
        {
            if (_grades.Count == 0) return 0;
            return _grades.Last().Id + 1;
        }

        public ExamTermGrade AddGrade(ExamTermGrade grade)
        {
            grade.Id = GenerateId();
            _grades.Add(grade);
            _storage.Save(_grades);
            NotifyObservers();
            return grade;
        }

        public ExamTermGrade? UpdateGrade(ExamTermGrade grade)
        {
            ExamTermGrade? oldGrade = GetGradeById(grade.Id);
            if (oldGrade == null) return null;

            oldGrade.StudentId = grade.StudentId;
            oldGrade.TeacherId = grade.TeacherId;
            oldGrade.ExamId = grade.ExamId;
            oldGrade.ReadingPoints = grade.ReadingPoints;
            oldGrade.SpeakingPoints = grade.SpeakingPoints;
            oldGrade.WritingPoints = grade.WritingPoints;
            oldGrade.ListeningPoints = grade.ListeningPoints;
            oldGrade.Value = grade.Value;

            _storage.Save(_grades);
            NotifyObservers();
            return oldGrade;
        }

        public ExamTermGrade? RemoveGrade(int id)
        {
            ExamTermGrade? grade = GetGradeById(id);
            if (grade == null) return null;

            _grades.Remove(grade);
            _storage.Save(_grades);
            NotifyObservers();
            return grade;
        }

        public bool IsStudentGraded(int studentId)
        {
            foreach (var grade in _grades)
            {
                if (grade.StudentId == studentId)
                {
                    return true;
                }
            }
            return false;
        }

        public ExamTermGrade? GetGradeById(int id)
        {
            return _grades.Find(v => v.Id == id);
        }

        public ExamTermGrade? GetExamTermGradeByStudentTeacherExam(int studentId, int teacherId, int examId)
        {
            return _grades.Find(grade => grade.StudentId == studentId && grade.TeacherId == teacherId && grade.ExamId == examId);
        }

        public ExamTermGrade? GetExamTermGradeByStudentExam(int studentId, int examId)
        {
            return _grades.Find(grade => grade.StudentId == studentId && grade.ExamId == examId);
        }

        public List<ExamTermGrade> GetExamTermGradesByTeacherExam(int teacherId, int examId)
        {
            return _grades.Where(grade => grade.TeacherId == teacherId && grade.ExamId == examId).ToList();
        }

        public List<ExamTermGrade> GetAllExamTermGrades()
        {
            return _grades;
        }
    }
}