using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Smartpoints_Interface
{
    public sealed partial class teacher : Page
    {
        private ObservableCollection<double> Grades { get; } = new();
        private ObservableCollection<SubjectModel> Subjects { get; } = new();
        
        private const string BaseUrl = "http://localhost:8080";
        private readonly HttpClient _httpClient;

        public teacher()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
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

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var subject = SubjectComboBox.SelectedItem as SubjectModel;
                if (subject == null)
                {
                    return;
                }

                var code = CodeTextBox.Text?.Trim();
                var title = TitleTextBox.Text?.Trim();
                var week = (int)WeekNumberBox.Value;
                var grades = Grades.ToList();

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title))
                {
                    return;
                }
                
                var testData = new
                {
                    code = code,
                    title = title,
                    week = week,
                    subjectId = subject.Id
                };

                var testContent = new StringContent(
                    JsonSerializer.Serialize(testData),
                    Encoding.UTF8,
                    "application/json"
                );

                var testResponse = await _httpClient.PostAsync(
                    $"{BaseUrl}/tests" + getCode(),
                    testContent
                );

                if (!testResponse.IsSuccessStatusCode)
                {
                    return;
                }

                var testJson = await testResponse.Content.ReadAsStringAsync();
                var createdTest = JsonSerializer.Deserialize<TestData>(testJson);

                if (createdTest == null)
                {
                    return;
                }
                
                foreach (var grade in grades)
                {
                    var pointData = new
                    {
                        grade = grade,
                        testId = createdTest.Id,
                        userId = 1
                    };

                    var pointContent = new StringContent(
                        JsonSerializer.Serialize(pointData),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var pointResponse = await _httpClient.PostAsync(
                        $"{BaseUrl}/points" + getCode(),
                        pointContent
                    );

                    if (!pointResponse.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private String getCode()
        {
            var code = Environment.GetEnvironmentVariable("AUTH_CODE") ?? "123";
            return "?code=" + code;
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

        public class TestCreateDto
        {
            public string code { get; set; }
            public int week { get; set; }
            public string title { get; set; }
            public int subjectId { get; set; }
        }

        public class PointCreateDto
        {
            public double grade { get; set; }
            public int testId { get; set; }
            public int userId { get; set; }
        }

        public class TestResponseDto
        {
            public int id { get; set; }
        }
    }
}