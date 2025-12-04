namespace Smartpoints_API.apiRequests;

public class TestRequest : ApiRequest
{
    public override string GetUrl() => "test";

    protected override void GetRequests()
    {
        RegisterHandler(RequestType.GET, GetRequest);
    }

    public (string url, Action<Dictionary<string,string>> handler) GetRequest()
    {
        Action<Dictionary<string,string>> logic = (parameters) =>
        {
            string id = parameters["id"];
            WritePlain($"You requested test with id = {id}");
        };

        return (GetUrl() + "/{id}", logic);
    }
}