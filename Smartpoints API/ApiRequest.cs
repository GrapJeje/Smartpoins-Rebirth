using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Smartpoints_API;

public abstract class ApiRequest
{
    private Dictionary<string, Request> requests = [];

    protected HttpListenerRequest request;
    protected HttpListenerResponse response;
    
    protected ApiRequest()
    {
        GetRequests();
    }

    public abstract String GetUrl();

    protected abstract void GetRequests();
    
    public void Handle(HttpListenerContext context)
    {
        request = context.Request;
        response = context.Response;

        var path = request.Url.AbsolutePath;
        if (path.StartsWith("/")) path = path.Substring(1);

        var found = false;
        foreach (var req in requests.Values)
        {
            if (!req.TryMatch(path, out var parameters)) continue;
            req.methode(parameters);
            response.StatusCode = 200;
            found = true;
            break;
        }
        if (!found)
        {
            response.StatusCode = 404;
            WritePlain("Not Found");
        }

        response.OutputStream.Close();
    }
    
    protected void WriteJson(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentType = "application/json; charset=UTF-8";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
    
    protected void WritePlain(string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        response.ContentType = "text/plain; charset=UTF-8";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    protected void sendResponse(int statusCode, string message)
    {
        response.StatusCode = statusCode;
        WritePlain(message);
    }

    protected void RegisterHandler(RequestType type, Func<(string url, Action<Dictionary<string,string>> handler)> methode)
    {
        var (url, handler) = methode();
        var request = new Request(type, url, handler);
        requests.Add(url, request);
    }


    class Request
    {
        public RequestType type { get; }
        public Action<Dictionary<string,string>> methode { get; }
        public string pattern { get; }
        private Regex regex;

        public Request(RequestType type, string pattern, Action<Dictionary<string,string>> methode)
        {
            this.type = type;
            this.pattern = pattern;
            this.methode = methode;
        
            string regexPattern = "^" + Regex.Replace(pattern, @"\{(\w+)\}", @"(?<$1>[^/]+)") + "$";
            regex = new Regex(regexPattern, RegexOptions.Compiled);
        }

        public bool TryMatch(string path, out Dictionary<string,string> parameters)
        {
            var match = regex.Match(path);
            if (!match.Success)
            {
                parameters = null!;
                return false;
            }

            parameters = new Dictionary<string,string>();
            foreach(var name in regex.GetGroupNames())
            {
                if (int.TryParse(name, out _)) continue;
                parameters[name] = match.Groups[name].Value;
            }

            return true;
        }
    }
    
    public enum RequestType
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH
    }
}