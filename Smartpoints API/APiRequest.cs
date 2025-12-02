using System.Net;

namespace smartpoints_api.apiRequests;

public abstract class APiRequest
{
    public abstract String GetUrl();

    public abstract String GetPage();
    
    public virtual bool Handle(HttpListenerContext context)
    {
        return false;
    }
}