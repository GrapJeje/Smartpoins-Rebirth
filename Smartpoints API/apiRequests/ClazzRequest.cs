using smartpoints_api;
using Smartpoints_Api.Models;

namespace Smartpoints_API.apiRequests;

public class ClazzRequest : ApiRequest
{
    public override string GetUrl() => "classes";

    protected override void GetRequests()
    {
        RegisterHandler(RequestType.GET, GetClazz);
        RegisterHandler(RequestType.GET, GetAllClasses);
        RegisterHandler(RequestType.POST, CreateClazz);
        RegisterHandler(RequestType.PUT, UpdateClazz);
        RegisterHandler(RequestType.DELETE, DeleteClazz);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetClazz()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);

            using var db = new AppDbContext();
            var clazz = db.Clazzes
                .Where(c => c.Id == id)
                .Select(c => new {
                    c.Id,
                    c.Name,
                    UserCount = c.Users.Count,
                    TestCount = c.Tests.Count
                })
                .FirstOrDefault();

            if (clazz == null)
            {
                sendResponse(404, "Class not found");
                return;
            }

            WriteJson(clazz);
            sendResponse(200, "OK");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetAllClasses()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            using var db = new AppDbContext();

            var classes = db.Clazzes
                .Select(c => new {
                    c.Id,
                    c.Name,
                    UserCount = c.Users.Count,
                    TestCount = c.Tests.Count
                })
                .ToList();

            WriteJson(classes);
            sendResponse(200, "OK");
        };

        return (GetUrl(), logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) CreateClazz()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            var body = ReadJson<ClazzCreateDto>();

            if (body == null || string.IsNullOrWhiteSpace(body.name))
            {
                sendResponse(400, "Name is required");
                return;
            }

            if (body.name.Length > 100)
            {
                sendResponse(400, "Name cannot exceed 100 characters");
                return;
            }

            using var db = new AppDbContext();

            var clazz = new Clazz
            {
                Name = body.name
            };

            db.Clazzes.Add(clazz);
            db.SaveChanges();

            WriteJson(clazz);
            sendResponse(201, "Created");
        };

        return (GetUrl(), logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) UpdateClazz()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);
            var body = ReadJson<ClazzUpdateDto>();

            using var db = new AppDbContext();

            var clazz = db.Clazzes.Find(id);
            if (clazz == null)
            {
                sendResponse(404, "Class not found");
                return;
            }

            if (!string.IsNullOrWhiteSpace(body.name))
            {
                if (body.name.Length > 100)
                {
                    sendResponse(400, "Name cannot exceed 100 characters");
                    return;
                }
                clazz.Name = body.name;
            }

            db.SaveChanges();

            WriteJson(clazz);
            sendResponse(200, "Updated");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) DeleteClazz()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);

            using var db = new AppDbContext();

            var clazz = db.Clazzes.Find(id);
            if (clazz == null)
            {
                sendResponse(404, "Class not found");
                return;
            }

            db.Clazzes.Remove(clazz);
            db.SaveChanges();

            sendResponse(200, "Deleted");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public class ClazzCreateDto
    {
        public string name { get; set; } = string.Empty;
    }
    
    public class ClazzUpdateDto
    {
        public string? name { get; set; }
    }
}
