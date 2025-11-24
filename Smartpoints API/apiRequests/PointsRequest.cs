using System.Net;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Linq;
using smartpoints_api;

namespace smartpoints_api.apiRequests;

// Consolidated CRUD handler for /points and /points/{id}
public class PointsRequest : APiRequest
{
    public override string GetUrl() => "points";

    // Legacy server fallback requires a non-null page; not used for API flows.
    public override string GetPage() => string.Empty;

    public override bool Handle(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        var path = request.Url?.AbsolutePath?.TrimStart('/') ?? string.Empty;
        if (!path.StartsWith(GetUrl(), StringComparison.OrdinalIgnoreCase)) return false;

        // split into segments: "points" or "points/{id}"
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        int? id = null;
        if (segments.Length > 1 && int.TryParse(segments[1], out var parsed)) id = parsed;

        try
        {
            switch (request.HttpMethod?.ToUpperInvariant())
            {
                case "GET":
                    if (id.HasValue) return HandleGetById(response, id.Value);
                    return HandleGetAll(response);

                case "POST":
                    if (id.HasValue) { WritePlain(response, "POST to /points/{id} not allowed"); response.StatusCode = 405; return true; }
                    return HandleCreate(request, response);

                case "PUT":
                case "PATCH":
                    if (!id.HasValue) { WritePlain(response, "PUT requires /points/{id}"); response.StatusCode = 400; return true; }
                    return HandleUpdate(request, response, id.Value);

                case "DELETE":
                    if (!id.HasValue) { WritePlain(response, "DELETE requires /points/{id}"); response.StatusCode = 400; return true; }
                    return HandleDelete(response, id.Value);

                default:
                    response.StatusCode = 405;
                    WritePlain(response, "Method Not Allowed");
                    return true;
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            WritePlain(response, $"Internal Server Error: {ex.Message}");
            return true;
        }
    }

    private static bool HandleGetAll(HttpListenerResponse response)
    {
        using var db = new AppDbContext();
        var points = db.Points.ToList();
        response.StatusCode = 200;
        WriteJson(response, points);
        return true;
    }

    private static bool HandleGetById(HttpListenerResponse response, int id)
    {
        using var db = new AppDbContext();
        var point = db.Points.Find(id);
        if (point == null)
        {
            response.StatusCode = 404;
            WritePlain(response, "Not Found");
            return true;
        }
        response.StatusCode = 200;
        WriteJson(response, point);
        return true;
    }

    private static bool HandleCreate(HttpListenerRequest request, HttpListenerResponse response)
    {
        using var ms = new MemoryStream();
        request.InputStream.CopyTo(ms);
        var body = Encoding.UTF8.GetString(ms.ToArray());
        var item = JsonSerializer.Deserialize<Point>(body);
        if (item == null)
        {
            response.StatusCode = 400;
            WritePlain(response, "Invalid JSON body");
            return true;
        }

        using var db = new AppDbContext();
        db.Points.Add(item);
        db.SaveChanges();

        response.StatusCode = 201;
        WriteJson(response, item);
        return true;
    }

    private static bool HandleUpdate(HttpListenerRequest request, HttpListenerResponse response, int id)
    {
        using var ms = new MemoryStream();
        request.InputStream.CopyTo(ms);
        var body = Encoding.UTF8.GetString(ms.ToArray());
        var updated = JsonSerializer.Deserialize<Point>(body);
        if (updated == null)
        {
            response.StatusCode = 400;
            WritePlain(response, "Invalid JSON body");
            return true;
        }

        using var db = new AppDbContext();
        var existing = db.Points.Find(id);
        if (existing == null)
        {
            response.StatusCode = 404;
            WritePlain(response, "Not Found");
            return true;
        }

        // Ensure the primary key matches the path id
        updated.Id = id;

        db.Entry(existing).CurrentValues.SetValues(updated);
        db.SaveChanges();

        response.StatusCode = 200;
        WriteJson(response, updated);
        return true;
    }

    private static bool HandleDelete(HttpListenerResponse response, int id)
    {
        using var db = new AppDbContext();
        var existing = db.Points.Find(id);
        if (existing == null)
        {
            response.StatusCode = 404;
            WritePlain(response, "Not Found");
            return true;
        }

        db.Points.Remove(existing);
        db.SaveChanges();

        response.StatusCode = 204;
        // No content for 204
        response.ContentLength64 = 0;
        response.OutputStream.Close();
        return true;
    }

    private static void WriteJson(HttpListenerResponse response, object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentType = "application/json; charset=UTF-8";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private static void WritePlain(HttpListenerResponse response, string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        response.ContentType = "text/plain; charset=UTF-8";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}