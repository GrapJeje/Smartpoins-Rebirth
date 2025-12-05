using smartpoints_api;
using Smartpoints_Api.Models;

namespace Smartpoints_API.apiRequests;

public class SubjectRequest : ApiRequest
{
    public override string GetUrl() => "subjects";

    protected override void GetRequests()
    {
        RegisterHandler(RequestType.GET, GetSubject);
        RegisterHandler(RequestType.GET, GetAllSubjects);
        RegisterHandler(RequestType.POST, CreateSubject);
        RegisterHandler(RequestType.PUT, UpdateSubject);
        RegisterHandler(RequestType.DELETE, DeleteSubject);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetSubject()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);

            using var db = new AppDbContext();
            var subject = db.Subjects
                .Where(s => s.Id == id)
                .Select(s => new {
                    s.Id,
                    s.Name,
                    TestCount = s.Tests.Count
                })
                .FirstOrDefault();

            if (subject == null)
            {
                sendResponse(404, "Subject not found");
                return;
            }

            WriteJson(subject);
            sendResponse(200, "OK");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetAllSubjects()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            using var db = new AppDbContext();

            var subjects = db.Subjects
                .Select(s => new {
                    s.Id,
                    s.Name,
                    TestCount = s.Tests.Count
                })
                .ToList();

            WriteJson(subjects);
            sendResponse(200, "OK");
        };

        return (GetUrl(), logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) CreateSubject()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            var body = ReadJson<SubjectCreateDto>();
            Console.WriteLine(body == null ? "body is null" : $"name={body.name}");

            if (body == null || string.IsNullOrWhiteSpace(body.name))
            {
                sendResponse(400, "Name is required");
                return;
            }

            using var db = new AppDbContext();

            var subject = new Subject
            {
                Name = body.name
            };

            db.Subjects.Add(subject);
            db.SaveChanges();

            WriteJson(subject);
            sendResponse(201, "Created");
        };

        return (GetUrl(), logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) UpdateSubject()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);
            var body = ReadJson<SubjectUpdateDto>();

            using var db = new AppDbContext();

            var subject = db.Subjects.Find(id);
            if (subject == null)
            {
                sendResponse(404, "Subject not found");
                return;
            }

            if (!string.IsNullOrWhiteSpace(body.name))
                subject.Name = body.name;

            db.SaveChanges();

            WriteJson(subject);
            sendResponse(200, "Updated");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) DeleteSubject()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);

            using var db = new AppDbContext();

            var subject = db.Subjects.Find(id);
            if (subject == null)
            {
                sendResponse(404, "Subject not found");
                return;
            }

            db.Subjects.Remove(subject);
            db.SaveChanges();

            sendResponse(200, "Deleted");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public class SubjectCreateDto
    {
        public string name { get; set; } = string.Empty;
    }
    
    public class SubjectUpdateDto
    {
        public string? name { get; set; }
    }
}
