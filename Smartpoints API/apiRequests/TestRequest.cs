using smartpoints_api;

namespace Smartpoints_API.apiRequests;

public class TestRequest : ApiRequest
{
    public override string GetUrl() => "tests";

    protected override void GetRequests()
    {
        RegisterHandler(RequestType.GET, GetTest);
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
}