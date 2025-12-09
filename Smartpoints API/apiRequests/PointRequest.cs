using smartpoints_api;
using Smartpoints_Api.Models;

namespace Smartpoints_API.apiRequests;

public class PointRequest : ApiRequest
{
    public override string GetUrl() => "points";

    protected override void GetRequests()
    {
        RegisterHandler(RequestType.GET, GetPoint);
        RegisterHandler(RequestType.GET, GetAllPoints);
        RegisterHandler(RequestType.POST, CreatePoint);
        RegisterHandler(RequestType.PUT, UpdatePoint);
        RegisterHandler(RequestType.DELETE, DeletePoint);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetPoint()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);

            using var db = new AppDbContext();
            var point = db.Points
                .Where(p => p.Id == id)
                .Select(p => new {
                    p.Id,
                    p.Grade,
                    p.TestId,
                    TestCode = p.Test.Code,
                    TestTitle = p.Test.Title,
                    p.UserId,
                    UserName = p.User.Name,
                    UserEmail = p.User.Email
                })
                .FirstOrDefault();

            if (point == null)
            {
                sendResponse(404, "Point not found");
                return;
            }

            WriteJson(point);
            sendResponse(200, "OK");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetAllPoints()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            using var db = new AppDbContext();

            var points = db.Points
                .Select(p => new {
                    p.Id,
                    p.Grade,
                    p.TestId,
                    TestCode = p.Test.Code,
                    TestTitle = p.Test.Title,
                    p.UserId,
                    UserName = p.User.Name,
                    UserEmail = p.User.Email
                })
                .ToList();

            WriteJson(points);
            sendResponse(200, "OK");
        };

        return (GetUrl(), logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) CreatePoint()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            var body = ReadJson<PointCreateDto>();

            if (body == null)
            {
                sendResponse(400, "Invalid request body");
                return;
            }

            if (body.grade < 0 || body.grade > 10)
            {
                sendResponse(400, "Grade must be between 0 and 10");
                return;
            }

            using var db = new AppDbContext();

            var test = db.Tests.Find(body.testId);
            if (test == null)
            {
                sendResponse(400, "Test not found");
                return;
            }

            var user = db.Users.Find(body.userId);
            if (user == null)
            {
                sendResponse(400, "User not found");
                return;
            }

            var point = new Point
            {
                Grade = body.grade,
                TestId = body.testId,
                UserId = body.userId
            };

            db.Points.Add(point);
            db.SaveChanges();

            WriteJson(point);
            sendResponse(201, "Created");
        };

        return (GetUrl(), logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) UpdatePoint()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);
            var body = ReadJson<PointUpdateDto>();

            using var db = new AppDbContext();

            var point = db.Points.Find(id);
            if (point == null)
            {
                sendResponse(404, "Point not found");
                return;
            }

            if (body.grade.HasValue)
            {
                if (body.grade < 0 || body.grade > 10)
                {
                    sendResponse(400, "Grade must be between 0 and 10");
                    return;
                }
                point.Grade = body.grade.Value;
            }

            if (body.testId.HasValue)
            {
                var test = db.Tests.Find(body.testId);
                if (test == null)
                {
                    sendResponse(400, "Test not found");
                    return;
                }
                point.TestId = body.testId.Value;
            }

            if (body.userId.HasValue)
            {
                var user = db.Users.Find(body.userId);
                if (user == null)
                {
                    sendResponse(400, "User not found");
                    return;
                }
                point.UserId = body.userId.Value;
            }

            db.SaveChanges();

            WriteJson(point);
            sendResponse(200, "Updated");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) DeletePoint()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);

            using var db = new AppDbContext();

            var point = db.Points.Find(id);
            if (point == null)
            {
                sendResponse(404, "Point not found");
                return;
            }

            db.Points.Remove(point);
            db.SaveChanges();

            sendResponse(200, "Deleted");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public class PointCreateDto
    {
        public double grade { get; set; }
        public int testId { get; set; }
        public int userId { get; set; }
    }
    
    public class PointUpdateDto
    {
        public double? grade { get; set; }
        public int? testId { get; set; }
        public int? userId { get; set; }
    }
}
