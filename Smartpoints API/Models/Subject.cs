using System.ComponentModel.DataAnnotations;

namespace Smartpoints_Api.Models;

public class Subjects
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(100, ErrorMessage = "Naam mag maximaal 100 tekens bevatten.")]
    public string Name { get; set; }
    
    public ICollection<Test> Tests { get; set; } = new List<Test>();
}