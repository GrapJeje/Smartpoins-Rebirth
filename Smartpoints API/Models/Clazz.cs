using System.ComponentModel.DataAnnotations;

namespace Smartpoints_Api.Models;

public class Clazz
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Naam van de klas is verplicht.")]
    [StringLength(100, ErrorMessage = "Naam van de klas mag maximaal 100 tekens bevatten.")]
    public string Name { get; set; }
    
    public ICollection<User> Users { get; set; } = new List<User>();
    
    public ICollection<Test> Tests { get; set; } = new List<Test>();
}