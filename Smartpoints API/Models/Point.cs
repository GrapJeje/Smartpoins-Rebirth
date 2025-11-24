namespace smartpoints_api;

// Minimal entity so DbContext and JSON binding compile.
// Add additional properties as your domain requires.
public class Point
{
    public int Id { get; set; }
}