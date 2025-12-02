using System.ComponentModel.DataAnnotations;

namespace Smartpoints_Api.Models;

public class Subjects
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Naam is verplicht.")]
    public string Name { get; set; }
}