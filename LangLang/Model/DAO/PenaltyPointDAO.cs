﻿using LangLang.Observer;
using LangLang.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangLang.Model.DAO
{
    public class PenaltyPointDAO : Subject
    {
        private readonly List<PenaltyPoint> _points;
        private readonly Storage<PenaltyPoint> _storage;

        public PenaltyPointDAO()
        {
            _storage = new Storage<PenaltyPoint>("penaltyPoints.csv");
            _points = _storage.Load();
        }

        private int GenerateId()
        {
            if (_points.Count == 0) return 0;
            return _points.Last().Id + 1;
        }

        public PenaltyPoint AddPenaltyPoint(PenaltyPoint point)
        {
            point.Id = GenerateId();
            _points.Add(point);
            _storage.Save(_points);
            NotifyObservers();
            return point;
        }

        public PenaltyPoint? UpdatePenaltyPoint(PenaltyPoint point)
        {
            PenaltyPoint? oldPoint = GetPenaltyPointById(point.Id);
            if (oldPoint == null) return null;

            oldPoint.StudentId = point.StudentId;
            oldPoint.CourseId = point.CourseId;
            oldPoint.IsDeleted = point.IsDeleted;
            oldPoint.DateSent = point.DateSent;

            _storage.Save(_points);
            NotifyObservers();
            return oldPoint;
        }

        public PenaltyPoint? RemovePenaltyPoint(int id)
        {
            PenaltyPoint? point = GetPenaltyPointById(id);
            if (point == null) return null;

            _points.Remove(point);
            _storage.Save(_points);
            NotifyObservers();
            return point;
        }
        public List<PenaltyPoint> GetAllPenaltyPoints()
        {
            return _points;
        }
        public PenaltyPoint? GetPenaltyPointById(int id)
        {
            return _points.Find(v => v.Id == id);
        }
        public List<PenaltyPoint> GetPenaltyPointsByCourseId(int courseId)
        {
            return _points.Where(point => point.CourseId == courseId).ToList();
        }
        public List<PenaltyPoint> GetPenaltyPointsByStudentId(int studentId)
        {
            return _points.Where(point => point.StudentId == studentId && point.IsDeleted == false).ToList();
        }
        public List<PenaltyPoint> GetDeletedPenaltyPointsByStudentId(int studentId)
        {
            return _points.Where(point => point.StudentId == studentId && point.IsDeleted == true).ToList();
        }

    }
}
