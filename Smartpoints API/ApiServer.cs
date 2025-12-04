using System.Net;
using System.Text;
using Smartpoints_API;
using smartpoints_api.apiRequests;
using Smartpoints_API.apiRequests;

namespace smartpoints_api;

public class ApiServer
{
    private readonly List<ApiRequest> requests = new();
    private readonly string prefix;

    public ApiServer(string prefix)
    {
        this.prefix = prefix;
        RegisterRequests();
        Start();
    }

    private void RegisterRequests()
    {
        requests.Add(new TestRequest());
        requests.Add(new SessionRequest());
    }

    private void Start()
    {
        Console.WriteLine("Starting server...");
        using var listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        listener.Start();
        Console.WriteLine("Server is running...");

        while (true)
        {
            var context = listener.GetContext();
            var request = context.Request;
            var response = context.Response;

            Console.WriteLine($"Received request for {request.Url?.ToString() ?? "(no url)"}");

            var handled = false;
            foreach (var apiRequest in requests)
            {
                if (apiRequest.GetUrl() == null) continue;

                var urlPath = request.Url?.AbsolutePath?.TrimStart('/') ?? string.Empty;
                if (urlPath.Equals(apiRequest.GetUrl(), StringComparison.OrdinalIgnoreCase)
                    || urlPath.StartsWith(apiRequest.GetUrl() + "/", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        apiRequest.Handle(context);
                        handled = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        string error = $"500 Internal Server Error: {ex.Message}";
                        byte[] errorBuf = Encoding.UTF8.GetBytes(error);
                        response.StatusCode = 500;
                        response.ContentLength64 = errorBuf.Length;
                        response.ContentType = "text/plain; charset=UTF-8";
                        response.OutputStream.Write(errorBuf, 0, errorBuf.Length);
                        response.OutputStream.Close();
                        handled = true;
                        break;
                    }
                }
            }

            if (!handled)
            {
                string notFound = "404 Not Found";
                byte[] buffer = Encoding.UTF8.GetBytes(notFound);
                response.ContentLength64 = buffer.Length;
                response.ContentType = "text/html; charset=UTF-8";
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
        }
    }
}
