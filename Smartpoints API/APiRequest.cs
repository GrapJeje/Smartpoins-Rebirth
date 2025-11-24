using System.Net;

namespace smartpoints_api.apiRequests;

public abstract class APiRequest
{
    public abstract String GetUrl();

    public abstract String GetPage();

    /// <summary>
    /// Optioneel: handel het incoming HttpListenerContext af.
    /// Return true als de request is afgehandeld.
    /// Default implementatie doet niets (terug false) zodat bestaande requests blijven werken.
    /// </summary>
    public virtual bool Handle(HttpListenerContext context)
    {
        return false;
    }
}