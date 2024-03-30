﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using LangLang.Model.Enums;
using LangLang.Observer;
using LangLang.Storage;

namespace LangLang.Model.DAO
{
    public class DirectorDAO : Subject
    {

        private readonly List<Teacher> _teachers;
        private readonly Storage<Teacher> _storage;
        
        public DirectorDAO() { }

        private int GenerateId()
        {
            if (_teachers.Count == 0) return 0;
            return _teachers.Last().Id + 1;
        }

        public Teacher AddTeacher(Teacher teacher)
        {
            teacher.Id = GenerateId();
            _teachers.Add(teacher);
            _storage.Save(_teachers);
            NotifyObservers();
            return teacher;
        }

        public Teacher UpdateTeacher(Teacher teacher)
        {
            Teacher oldTeacher = GetTeacherById(teacher.Id);
            if (oldTeacher == null) return null;

            oldTeacher.FirstName = teacher.FirstName;
            oldTeacher.LastName = teacher.LastName;
            oldTeacher.Gender = teacher.Gender;
            oldTeacher.DateOfBirth = teacher.DateOfBirth;
            oldTeacher.PhoneNumber = teacher.PhoneNumber;
            oldTeacher.Email = teacher.Email;
            oldTeacher.Password = teacher.Password;
            oldTeacher.Title = teacher.Title;
            oldTeacher.Languages = teacher.Languages;
            oldTeacher.LevelOfLanguages = teacher.LevelOfLanguages;
            oldTeacher.StartedWork = teacher.StartedWork;
            oldTeacher.AverageRating = teacher.AverageRating;

            _storage.Save(_teachers);
            NotifyObservers();
            return oldTeacher;
        }

        public Teacher RemoveTeacher(int id)
        {
            Teacher teacher = GetTeacherById(id);
            if (teacher == null) return null;

            _teachers.Remove(teacher);
            _storage.Save(_teachers);
            NotifyObservers();
            return teacher;
        }

        private Teacher GetTeacherById(int id)
        {
            return _teachers.Find(t => t.Id == id);
        }

        public List<Teacher> GetAllTeachers()
        {
            return _teachers;
        }

        public List<Teacher> SearchAllTeachers(Language language, LanguageLevel levelOfLanguage, DateTime startedWork)
        {
            List<Teacher> teachers = GetAllTeachers();

            var filteredTeachers = teachers.Where(teacher =>
                (language == null || teacher.Languages.Contains(language)) &&
                (levelOfLanguage == null || teacher.LevelOfLanguages.Contains(levelOfLanguage)) &&
                (startedWork == DateTime.MinValue || teacher.StartedWork.Date == startedWork.Date)
            ).ToList();

            return filteredTeachers;
        }
    }
}
