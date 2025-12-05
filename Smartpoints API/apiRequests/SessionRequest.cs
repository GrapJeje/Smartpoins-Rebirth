using smartpoints_api;

namespace Smartpoints_API.apiRequests;

public class SessionRequest : ApiRequest
{
    public override string GetUrl() => "sessions";

    protected override void GetRequests()
    {
        RegisterHandler(RequestType.GET, GetRequest);
        RegisterHandler(RequestType.POST, SetRequest);
        RegisterHandler(RequestType.DELETE, PurgeRequest);
    }

    public (string url, Action<Dictionary<string,string>> handler) GetRequest()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            var id = parameters["id"];
            using var db = new AppDbContext();
            var user = db.Users.Find(int.Parse(id));
            if (user == null)
            {
                sendResponse(404, "User not found");
                return;
            }
            WriteJson(user.SessionToken);
            sendResponse(200, "Getter");
        };
        return (GetUrl() + "/{id}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) SetRequest()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            var id = parameters["id"];
            var token = parameters["token"];
            using var db = new AppDbContext();
            var user = db.Users.Find(int.Parse(id));
            if (user == null)
            {
                sendResponse(404, "User not found");
                return;
            }
            user.SessionToken = token;
            db.SaveChanges();
            sendResponse(200, "Token created");
        };
        return (GetUrl() + "/set/{id}/{token}", logic);
    }
    
    public (string url, Action<Dictionary<string,string>> handler) PurgeRequest()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            var id = parameters["id"];
            using var db = new AppDbContext();
            var user = db.Users.Find(int.Parse(id));
            if (user == null)
            {
                sendResponse(404, "User not found");
                return;
            }
            user.SessionToken = "";
            db.SaveChanges();
            sendResponse(200, "Token purged");
        };
        return (GetUrl() + "/purge/{id}", logic);
    }
}