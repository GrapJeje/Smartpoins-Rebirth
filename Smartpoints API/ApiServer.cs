using System.Net;
using System.Text;
using Smartpoints_API;
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
        requests.Add(new SessionRequest());
        requests.Add(new SubjectRequest());
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
            HttpListenerContext? context = null;
            try
            {
                context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                Console.WriteLine($"Received request for {request.Url?.ToString() ?? "(no url)"}");

                var handled = false;

                foreach (var apiRequest in requests)
                {
                    var baseUrl = apiRequest.GetUrl();
                    if (baseUrl == null) continue;

                    var urlPath = request.Url?.AbsolutePath?.TrimStart('/') ?? string.Empty;

                    if (!urlPath.Equals(baseUrl, StringComparison.OrdinalIgnoreCase)
                        && !urlPath.StartsWith(baseUrl + "/", StringComparison.OrdinalIgnoreCase)) continue;
                    try
                    {
                        apiRequest.Handle(context);
                    }
                    catch (Exception ex)
                    {
                        WriteSafeError(response, $"500 Internal Server Error: {ex.Message}");
                    }

                    handled = true;
                    break;
                }

                if (!handled) WriteSafeError(response, "404 Not Found", 404);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Server exception (but continuing): {ex}");

                if (context != null)
                {
                    try
                    {
                        WriteSafeError(context.Response, "500 Internal Server Error (global catch)");
                    }
                    catch { /* ignore */ }
                }
            }
        }
    }
    
    private void WriteSafeError(HttpListenerResponse response, string message, int statusCode = 500)
    {
        try
        {
            if (response.OutputStream.CanWrite)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                response.StatusCode = statusCode;
                response.ContentType = "text/plain; charset=UTF-8";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }
        catch
        {
            // Ignore
        }
        finally
        {
            try { response.OutputStream.Close(); } catch { }
        }
    }
}
