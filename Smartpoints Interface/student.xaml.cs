using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Microsoft.UI.Xaml.Shapes;

namespace Smartpoints_Interface
{
    public sealed partial class student : Page
    {
        private const string BaseUrl = "http://localhost:8080";
        private readonly HttpClient _httpClient;
        private int _currentUserId = 1;

        // Test subject data
        private readonly List<SubjectInfo> _subjects = new()
        {
            new SubjectInfo { Code = "PRA", Name = "Praktijk", Color = "#C6E8C6" },
            new SubjectInfo { Code = "PRO", Name = "Pro", Color = "#E8A6A6" },
            new SubjectInfo { Code = "NAT", Name = "Native", Color = "#C6E8C6" },
            new SubjectInfo { Code = "WEB", Name = "Webdevelopment", Color = "#E8E8A6" },
            new SubjectInfo { Code = "NED", Name = "Nederlands", Color = "#A6D8E8" },
            new SubjectInfo { Code = "ENG", Name = "Engels", Color = "#E8A6E8" },
            new SubjectInfo { Code = "MENTOR", Name = "Mentor", Color = "#D8A6A6" },
            new SubjectInfo { Code = "SOFTSK", Name = "Soft Skills", Color = "#A6E8C6" }
        };

        public student()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await EnsureTestDataExists();
                
                await LoadUserData();
                LoadSubjects();
                await LoadTestsAndPoints();
            }
            catch (Exception ex)
            {
                ShowError($"Fout bij laden: {ex.Message}");
            }
        }

        private async Task EnsureTestDataExists()
        {
            try
            {
                var userResponse = await _httpClient.GetAsync($"{BaseUrl}/users/{_currentUserId}");
                if (!userResponse.IsSuccessStatusCode)
                {
                    var classResponse = await _httpClient.GetAsync($"{BaseUrl}/classes/1");
                    if (!classResponse.IsSuccessStatusCode)
                    {
                        var classData = new { name = "Test Klas" };
                        var classContent = new StringContent(JsonSerializer.Serialize(classData), Encoding.UTF8, "application/json");
                        await _httpClient.PostAsync($"{BaseUrl}/classes", classContent);
                    }
                    
                    var userData = new
                    {
                        name = "Test Student",
                        email = "test@student.nl",
                        password = "test123",
                        classId = 1,
                        role = "STUDENT"
                    };
                    var userContent = new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync($"{BaseUrl}/users", userContent);
                }
            }
            catch
            {
                ShowDemoData();
            }
        }

        private async Task LoadUserData()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/users/{_currentUserId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<UserData>(json);
                }
            }
            catch
            {
            }
        }

        private void LoadSubjects()
        {
            var subjectsPanel = FindName("SubjectsPanel") as StackPanel;
            if (subjectsPanel == null) return;

            subjectsPanel.Children.Clear();
            
            foreach (var subject in _subjects)
            {
                var subjectPanel = new StackPanel
                {
                    Background = new SolidColorBrush(HexToColor(subject.Color)),
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var textBlock = new TextBlock
                {
                    Text = subject.Code,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0)),
                    Margin = new Thickness(10),
                    FontSize = 20,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold
                };

                subjectPanel.Children.Add(textBlock);
                
                subjectPanel.Children.Add(new Rectangle { Height = 50, Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)) });
                subjectPanel.PointerPressed += (s, e) => OnSubjectClicked(subject.Code);
                subjectsPanel.Children.Add(subjectPanel);
            }
        }

        private async Task LoadTestsAndPoints(string subjectCode = null)
        {
            try
            {
                var testsResponse = await _httpClient.GetAsync($"{BaseUrl}/tests");
                var pointsResponse = await _httpClient.GetAsync($"{BaseUrl}/points");

                if (testsResponse.IsSuccessStatusCode && pointsResponse.IsSuccessStatusCode)
                {
                    var testsJson = await testsResponse.Content.ReadAsStringAsync();
                    var pointsJson = await pointsResponse.Content.ReadAsStringAsync();

                    var tests = JsonSerializer.Deserialize<List<TestData>>(testsJson);
                    var points = JsonSerializer.Deserialize<List<PointData>>(pointsJson);
                    
                    if (!string.IsNullOrEmpty(subjectCode))
                    {
                        
                    }

                    UpdateTestsGrid(tests, points);
                }
                else
                {
                    ShowDemoData();
                }
            }
            catch
            {
                ShowDemoData();
            }
        }

        private void UpdateTestsGrid(List<TestData> tests, List<PointData> points)
{
    var grid = TestsGrid;
    if (grid == null) return;
    
    while (grid.RowDefinitions.Count > 1)
    {
        grid.RowDefinitions.RemoveAt(1);
    }
    var childrenToRemove = grid.Children
        .OfType<FrameworkElement>()
        .Where(child => Grid.GetRow(child) > 0)
        .ToList();
    
    foreach (var child in childrenToRemove)
    {
        grid.Children.Remove(child);
    }
    foreach (var child in childrenToRemove)
    {
        grid.Children.Remove(child);
    }

    if (tests == null || points == null) return;
    for (int i = 0; i < tests.Count; i++)
    {
        var test = tests[i];
        var userPoints = points.Where(p => p.TestId == test.Id && p.UserId == _currentUserId).ToList();
        var averageGrade = userPoints.Any() ? userPoints.Average(p => p.Grade) : 0;
        var latestGrade = userPoints.LastOrDefault()?.Grade ?? 0;

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        var codeText = new TextBlock
        {
            Text = test.Code,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            Margin = new Thickness(10, 5, 10, 5),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(codeText, i + 1);
        Grid.SetColumn(codeText, 0);
        grid.Children.Add(codeText);
        
        var weekText = new TextBlock
        {
            Text = $"W{test.Id % 52 + 1}",
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            Margin = new Thickness(10, 5, 10, 5),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(weekText, i + 1);
        Grid.SetColumn(weekText, 1);
        grid.Children.Add(weekText);

        var titleText = new TextBlock
        {
            Text = test.Title,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            Margin = new Thickness(10, 5, 10, 5),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetRow(titleText, i + 1);
        Grid.SetColumn(titleText, 2);
        grid.Children.Add(titleText);
        
        var avgText = new TextBlock
        {
            Text = averageGrade.ToString("0.0"),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            Margin = new Thickness(10, 5, 10, 5),
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = averageGrade >= 5.5 ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.Normal
        };
        Grid.SetRow(avgText, i + 1);
        Grid.SetColumn(avgText, 3);
        grid.Children.Add(avgText);
        
        var gradeText = new TextBlock
        {
            Text = latestGrade > 0 ? latestGrade.ToString("0.0") : "-",
            Foreground = new SolidColorBrush(GetGradeColor(latestGrade)),
            Margin = new Thickness(10, 5, 10, 5),
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            FontSize = 16
        };
        Grid.SetRow(gradeText, i + 1);
        Grid.SetColumn(gradeText, 4);
        grid.Children.Add(gradeText);
    }
}

        private async void OnSubjectClicked(string subjectCode)
        {
            foreach (var child in (FindName("SubjectsPanel") as StackPanel).Children)
            {
                if (child is StackPanel panel)
                {
                    var border = new Thickness(0);
                    if ((panel.Children[0] as TextBlock)?.Text == subjectCode)
                    {
                        border = new Thickness(3);
                    }
                    panel.BorderThickness = border;
                    panel.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                }
            }

            await LoadTestsAndPoints(subjectCode);
        }

        private void ShowDemoData()
        {
            var demoTests = new List<TestData>
            {
                new TestData { Id = 1, Code = "PRA-01", Title = "Project opdracht 1" },
                new TestData { Id = 2, Code = "PRO-01", Title = "C# Basis" },
                new TestData { Id = 3, Code = "WEB-01", Title = "HTML/CSS" }
            };

            var demoPoints = new List<PointData>
            {
                new PointData { Id = 1, TestId = 1, UserId = _currentUserId, Grade = 7.5 },
                new PointData { Id = 2, TestId = 2, UserId = _currentUserId, Grade = 8.0 },
                new PointData { Id = 3, TestId = 3, UserId = _currentUserId, Grade = 6.5 }
            };

            UpdateTestsGrid(demoTests, demoPoints);
        }

        private void ShowError(string message)
        {
            var grid = FindName("TestsGrid") as Grid;
            if (grid == null) return;

            var errorText = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)),
                Margin = new Thickness(20),
                FontSize = 14
            };
            Grid.SetRow(errorText, 1);
            Grid.SetColumnSpan(errorText, 5);
            grid.Children.Add(errorText);
        }

        private Windows.UI.Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte a = 255;
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }

        private Windows.UI.Color GetGradeColor(double grade)
        {
            if (grade >= 5.5) return Windows.UI.Color.FromArgb(255, 0, 128, 0); // Groen
            if (grade >= 4.5) return Windows.UI.Color.FromArgb(255, 255, 165, 0); // Oranje
            return Windows.UI.Color.FromArgb(255, 255, 0, 0); // Rood
        }
    }
    
    public class SubjectInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class UserData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int ClassId { get; set; }
        public string Role { get; set; }
        public string ClassName { get; set; }
    }

    public class TestData
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int SubjectId { get; set; }
    }

    public class PointData
    {
        public int Id { get; set; }
        public double Grade { get; set; }
        public int TestId { get; set; }
        public int UserId { get; set; }
        public string TestCode { get; set; }
        public string TestTitle { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }
}