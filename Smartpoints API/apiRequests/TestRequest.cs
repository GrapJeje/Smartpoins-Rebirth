using smartpoints_api;
using Smartpoints_Api.Models;

namespace Smartpoints_API.apiRequests;

public class TestRequest : ApiRequest
{
    public override string GetUrl() => "tests";

    protected override void GetRequests()
    {
        RegisterHandler(RequestType.GET, GetTest);
        RegisterHandler(RequestType.GET, GetAllTests);
        RegisterHandler(RequestType.POST, CreateTest);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetTest()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);

            using var db = new AppDbContext();
            var test = db.Tests
                .Where(p => p.Id == id)
                .Select(p => new {
                    p.Id,
                    p.Code,
                    p.SubjectId,
                    p.Title,
                    p.Week
                })
                .FirstOrDefault();

            if (test == null)
            {
                sendResponse(404, "Test not found");
                return;
            }

            WriteJson(test);
            sendResponse(200, "OK");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetAllTests()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            using var db = new AppDbContext();

            var points = db.Tests
                .Select(p => new {
                    p.Id,
                    p.Code,
                    p.Week,
                    p.Title,
                    p.SubjectId
                })
                .ToList();

            WriteJson(points);
            sendResponse(200, "OK");
        };

        return (GetUrl(), logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) CreateTest()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            var body = ReadJson<TestCreateDto>();

            if (body == null)
            {
                sendResponse(400, "Invalid request body");
                return;
            }

            if (body.week < 0 || body.week > 52)
            {
                sendResponse(400, "Week must be between 0 and 52");
                return;
            }

            using var db = new AppDbContext();

            var test = new Test
            {
                Code = body.code,
                Week = body.week,
                Title = body.title,
                SubjectId = body.subjectId,
            };

            db.Tests.Add(test);
            db.SaveChanges();

            WriteJson(test);
            sendResponse(201, "Created");
        };

        return (GetUrl(), logic);
    }
    
    public class TestCreateDto
    {
        public string code { get; set; }
        public int week { get; set; }
        public string title { get; set; }
        public int subjectId { get; set; }
    }
}