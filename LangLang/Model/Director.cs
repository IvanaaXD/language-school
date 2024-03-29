﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LangLang.Model.Enums;

namespace LangLang.Model
{
    public class Director : Employee
    {
        public Director() { }

        public Director(int id, string firstName, string lastName, Gender gender, DateTime dateOfBirth, string phoneNumber, string email,
                string password, int title)
                : base(id, firstName, lastName, gender, dateOfBirth, phoneNumber, email, password, title) {}
        
        public string[] ToCsv()
        {
            return new string[] {
                Id.ToString(),
                FirstName, 
                LastName, 
                Gender.ToString(), 
                DateOfBirth.ToString(), 
                PhoneNumber, 
                Email, 
                Password, 
                Title.ToString() };
        }

        public void FromCsv(string[] values)
        {
            id = int.Parse(values[0]);
            firstName = values[1];
            lastName = values[2];
            gender = (Gender)Enum.Parse(typeof(Gender), values[3]);
            dateOfBirth = DateTime.ParseExact(values[4], "yyyy-MM-dd", null);
            phoneNumber = values[5];
            email = values[6];
            password = values[7];
            title = int.Parse(values[8]);
        }
    }
}
