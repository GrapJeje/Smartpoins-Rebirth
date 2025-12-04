using System.Net;
using smartpoints_api;
using smartpoints_api.apiRequests;

namespace Smartpoints_API.apiRequests;

public class SessionRequest : ApiRequest
{
    public override string GetUrl() => "session";

    protected override void GetRequests()
    {
        RegisterHandler(RequestType.GET, GetRequest);
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
            WriteJson(user);
            sendResponse(200, "Getter");
        };
        return (GetUrl() + "/{id}", logic);
    }
}