using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Smartpoints_Interface
{
    public sealed partial class teacher : Page
    {
        private ObservableCollection<double> Grades { get; } = new();
        private ObservableCollection<SubjectModel> Subjects { get; } = new();

        public teacher()
        {
            InitializeComponent();
            GradesListView.ItemsSource = Grades;
            SubjectComboBox.ItemsSource = Subjects;
            PopulateSubjectsFromStudentXaml();
            UpdateAverage();
        }

        private void PopulateSubjectsFromStudentXaml()
        {
            Subjects.Clear();
            Subjects.Add(new SubjectModel { Id = 0, Code = "PRA", Name = "Praktijk" });
            Subjects.Add(new SubjectModel { Id = 1, Code = "PRO", Name = "Praktijk ondersteuning" });
            Subjects.Add(new SubjectModel { Id = 2, Code = "NAT", Name = "Native" });
            Subjects.Add(new SubjectModel { Id = 3, Code = "WEB", Name = "Web" });
            Subjects.Add(new SubjectModel { Id = 4, Code = "NED", Name = "Nederlands" });
            Subjects.Add(new SubjectModel { Id = 5, Code = "ENG", Name = "Engels" });
            Subjects.Add(new SubjectModel { Id = 6, Code = "MENTOR", Name = "Mentoruur" });
            Subjects.Add(new SubjectModel { Id = 7, Code = "SOFTSK", Name = "Soft Skills" });

            SubjectComboBox.DisplayMemberPath = "Display";
        }

        private void AddGrade_Click(object sender, RoutedEventArgs e)
        {
            var value = NewGradeNumberBox.Value;
            if (value < NewGradeNumberBox.Minimum || value > NewGradeNumberBox.Maximum) return;
            Grades.Add(value);
            UpdateAverage();
        }

        private void RemoveGrade_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is double item)
            {
                Grades.Remove(item);
                UpdateAverage();
            }
        }

        private void ClearGradesButton_Click(object sender, RoutedEventArgs e)
        {
            Grades.Clear();
            UpdateAverage();
        }

        private void UpdateAverage()
        {
            if (Grades.Count == 0)
            {
                AverageTextBox.Text = string.Empty;
                return;
            }

            var avg = Grades.Average();
            AverageTextBox.Text = avg.ToString("0.##");
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var studentName = StudentNameTextBox.Text?.Trim() ?? string.Empty;
            var code = CodeTextBox.Text?.Trim() ?? string.Empty;
            var week = (int)WeekNumberBox.Value;
            var title = TitleTextBox.Text?.Trim() ?? string.Empty;
            var grades = Grades.ToArray();
            var average = grades.Length > 0 ? grades.Average() : (double?)null;
            var subject = SubjectComboBox.SelectedItem as SubjectModel;

            System.Diagnostics.Debug.WriteLine($"[Teacher] Code:{code} Student:{studentName} Week:{week} Title:{title} Subject:{subject?.Code} Avg:{average}");
            // TODO: stuur naar API (use HttpClient) of sla lokaal op
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            StudentNameTextBox.Text = string.Empty;
            CodeTextBox.Text = string.Empty;
            WeekNumberBox.Value = WeekNumberBox.Minimum;
            TitleTextBox.Text = string.Empty;
            Grades.Clear();
            SubjectComboBox.SelectedIndex = -1;
            UpdateAverage();
        }

        private class SubjectModel
        {
            public int Id { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Display => string.IsNullOrEmpty(Name) ? Code : $"{Code} - {Name}";
            public override string ToString() => Display;
        }
    }
}
