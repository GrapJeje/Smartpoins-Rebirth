using smartpoints_api;
using Smartpoints_Api.Models;

namespace Smartpoints_API.apiRequests;

public class UserRequest : ApiRequest
{
    public override string GetUrl() => "users";

    protected override void GetRequests()
    {
        RegisterHandler(RequestType.GET, GetUser);
        RegisterHandler(RequestType.GET, GetAllUsers);
        RegisterHandler(RequestType.POST, CreateUser);
        RegisterHandler(RequestType.PUT, UpdateUser);
        RegisterHandler(RequestType.DELETE, DeleteUser);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetUser()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);

            using var db = new AppDbContext();
            var user = db.Users
                .Where(u => u.Id == id)
                .Select(u => new {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.ClassId,
                    u.Role,
                    ClassName = u.Class.Name
                })
                .FirstOrDefault();

            if (user == null)
            {
                sendResponse(404, "User not found");
                return;
            }

            WriteJson(user);
            sendResponse(200, "OK");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) GetAllUsers()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            using var db = new AppDbContext();

            var users = db.Users
                .Select(u => new {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.ClassId,
                    u.Role,
                    ClassName = u.Class.Name
                })
                .ToList();

            WriteJson(users);
            sendResponse(200, "OK");
        };

        return (GetUrl(), logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) CreateUser()
    {
        Action<Dictionary<string,string>> logic = (_) =>
        {
            var body = ReadJson<UserCreateDto>();

            if (body == null || string.IsNullOrWhiteSpace(body.name) || 
                string.IsNullOrWhiteSpace(body.email) || string.IsNullOrWhiteSpace(body.password))
            {
                sendResponse(400, "Name, email, and password are required");
                return;
            }

            if (body.name.Length > 100)
            {
                sendResponse(400, "Name cannot exceed 100 characters");
                return;
            }

            if (body.email.Length > 150)
            {
                sendResponse(400, "Email cannot exceed 150 characters");
                return;
            }

            if (body.password.Length > 255)
            {
                sendResponse(400, "Password cannot exceed 255 characters");
                return;
            }

            using var db = new AppDbContext();

            if (db.Users.Any(u => u.Email == body.email))
            {
                sendResponse(400, "Email already exists");
                return;
            }

            var clazz = db.Clazzes.Find(body.classId);
            if (clazz == null)
            {
                sendResponse(400, "Class not found");
                return;
            }

            var user = new User
            {
                Name = body.name,
                Email = body.email,
                Password = body.password,
                ClassId = body.classId,
                Role = body.role ?? Role.STUDENT
            };

            db.Users.Add(user);
            db.SaveChanges();

            WriteJson(user);
            sendResponse(201, "Created");
        };

        return (GetUrl(), logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) UpdateUser()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);
            var body = ReadJson<UserUpdateDto>();

            using var db = new AppDbContext();

            var user = db.Users.Find(id);
            if (user == null)
            {
                sendResponse(404, "User not found");
                return;
            }

            if (!string.IsNullOrWhiteSpace(body.name))
            {
                if (body.name.Length > 100)
                {
                    sendResponse(400, "Name cannot exceed 100 characters");
                    return;
                }
                user.Name = body.name;
            }

            if (!string.IsNullOrWhiteSpace(body.email))
            {
                if (body.email.Length > 150)
                {
                    sendResponse(400, "Email cannot exceed 150 characters");
                    return;
                }
                if (db.Users.Any(u => u.Email == body.email && u.Id != id))
                {
                    sendResponse(400, "Email already exists");
                    return;
                }
                user.Email = body.email;
            }

            if (!string.IsNullOrWhiteSpace(body.password))
            {
                if (body.password.Length > 255)
                {
                    sendResponse(400, "Password cannot exceed 255 characters");
                    return;
                }
                user.Password = body.password;
            }

            if (body.classId.HasValue)
            {
                var clazz = db.Clazzes.Find(body.classId);
                if (clazz == null)
                {
                    sendResponse(400, "Class not found");
                    return;
                }
                user.ClassId = body.classId.Value;
            }

            if (body.role.HasValue)
            {
                user.Role = body.role.Value;
            }

            db.SaveChanges();

            WriteJson(user);
            sendResponse(200, "Updated");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) DeleteUser()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            int id = int.Parse(parameters["id"]);

            using var db = new AppDbContext();

            var user = db.Users.Find(id);
            if (user == null)
            {
                sendResponse(404, "User not found");
                return;
            }

            db.Users.Remove(user);
            db.SaveChanges();

            sendResponse(200, "Deleted");
        };

        return ($"{GetUrl()}/{{id}}", logic);
    }
    
    public class UserCreateDto
    {
        public string name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public int classId { get; set; }
        public Role? role { get; set; }
    }
    
    public class UserUpdateDto
    {
        public string? name { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public int? classId { get; set; }
        public Role? role { get; set; }
    }
}
