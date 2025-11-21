using smartpoints_api.apiRequests;

namespace Smartpoints_API.apiRequests;

public class TestRequest : APiRequest
{
    public override string GetUrl()
    {
        return "test";
    }

    public override string GetPage()
    {
        return "kaas";
    }
}