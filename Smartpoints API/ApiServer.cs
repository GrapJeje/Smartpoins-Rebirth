using System.Net;
using System.Text;
using smartpoints_api.apiRequests;
using Smartpoints_API.apiRequests;

namespace smartpoints_api;

public class ApiServer
{
    private readonly List<APiRequest> requests = new();
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

            Console.WriteLine($"Received request for {request.Url}");

            bool handled = false;
            foreach (var apiRequest in requests)
            {
                if (apiRequest.GetUrl() == null) continue;

                if (request.Url.AbsolutePath.TrimStart('/').Equals(apiRequest.GetUrl(), StringComparison.OrdinalIgnoreCase))
                {
                    string page = apiRequest.GetPage();
                    byte[] buffer = Encoding.UTF8.GetBytes(page);
                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "text/html; charset=UTF-8";
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                    handled = true;
                    break;
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
