﻿using LangLang.Controller;
using System;
using System.Collections.Generic;
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

namespace LangLang.View.Student
{
    /// <summary>
    /// Interaction logic for CancelCourseEnrollmentForm.xaml
    /// </summary>
    public partial class CancelCourseEnrollmentForm : Window
    {
        public CancelCourseEnrollmentForm()
        {
            InitializeComponent();
        }

        private void SendExplanationButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
