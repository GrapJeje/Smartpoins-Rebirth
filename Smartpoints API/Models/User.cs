using System.ComponentModel.DataAnnotations;

namespace Smartpoints_Api.Models;

public class User
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Naam is verplicht.")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Email is verplicht.")]
    [EmailAddress(ErrorMessage = "Voer een geldig e-mailadres in.")]
    public string email { get; set; }
    [Required(ErrorMessage = "Wachtwoord is verplicht.")]
    public string password { get; set; }
    public int class_id { get; set; }
}