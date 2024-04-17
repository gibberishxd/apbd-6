using System.ComponentModel.DataAnnotations;

namespace apbd_5.Models.DTOs;

public class UpdateAnimal
{
    [Required]
    [MinLength(3)]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(200)]
    public string Description { get; set; }

    [Required]
    [MaxLength(200)]
    public string Category { get; set; }

    [Required]
    [MaxLength(200)]
    public string Area { get; set; }
}