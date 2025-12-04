using System.ComponentModel.DataAnnotations;

namespace Smartpoints_Api.Models;

public class Point
{
    public int Id { get; set; }

    [Range(0, 10, ErrorMessage = "Het cijfer moet tussen 0 en 10 liggen.")]
    public double Grade { get; set; }

    [Required(ErrorMessage = "Test is verplicht.")]
    public int TestId { get; set; }

    public Test Test { get; set; }

    [Required(ErrorMessage = "Gebruiker is verplicht.")]
    public int UserId { get; set; }

    public User User { get; set; }
}