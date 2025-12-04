using System.ComponentModel.DataAnnotations;

namespace Smartpoints_Api.Models;

public class Test
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Code is verplicht.")]
    [StringLength(50, ErrorMessage = "Code mag maximaal 50 tekens bevatten.")]
    public string Code { get; set; }

    [Range(1, 52, ErrorMessage = "Je moet een geldige week opgeven.")]
    public int Week { get; set; }

    [Required(ErrorMessage = "Titel is verplicht.")]
    [StringLength(100, ErrorMessage = "Titel mag maximaal 100 tekens bevatten.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Vak is verplicht.")]
    public int SubjectId { get; set; }
    
    public Subjects Subject { get; set; }
}