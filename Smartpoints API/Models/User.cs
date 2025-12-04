using System.ComponentModel.DataAnnotations;

namespace Smartpoints_Api.Models;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(100, ErrorMessage = "Naam mag maximaal 100 tekens bevatten.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email is verplicht.")]
    [EmailAddress(ErrorMessage = "Voer een geldig e-mailadres in.")]
    [StringLength(150, ErrorMessage = "E-mail mag maximaal 150 tekens bevatten.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Wachtwoord is verplicht.")]
    [StringLength(255, ErrorMessage = "Wachtwoord mag maximaal 255 tekens bevatten.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Klas is verplicht.")]
    public int ClassId { get; set; }
    public Clazz Class { get; set; }

    [Required(ErrorMessage = "Rol is verplicht.")]
    public Role Role { get; set; } = Role.STUDENT;
    
    [StringLength(255, ErrorMessage = "Session token mag maximaal 255 tekens bevatten.")]
    public string SessionToken { get; set; }
}