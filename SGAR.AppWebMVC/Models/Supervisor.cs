using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGAR.AppWebMVC.Models;

public partial class Supervisor
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El Nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;
    [Required(ErrorMessage = "El Apellido es obligatorio.")]
    public string Apellido { get; set; } = null!;
    [StringLength(9, ErrorMessage = "El teléfono puede escribirse con o sin guión, no exceda el límite de caractéres.")]
    public string? Telefono { get; set; }

    public string? CorreoPersonal { get; set; }

    [Required(ErrorMessage = "El Dui es obligatorio.")]
    [StringLength(10, MinimumLength = 9, ErrorMessage = "El Dui puede escribirse con o sin guión, no exceda el límite de caractéres.")]
    public string Dui { get; set; } = null!;
    [NotMapped]
    public IFormFile? FotoFile { get; set; }
    public byte[]? Foto { get; set; }
    [Required(ErrorMessage = "El Código es obligatorio.")]
    public string Codigo { get; set; } = null!;
    [Required(ErrorMessage = "El Correo Laboral es obligatorio.")]
    [Display(Name = "Correo Laboral")]
    public string CorreoLaboral { get; set; } = null!;
    [Required(ErrorMessage = "El Telefono Laboral es obligatorio.")]
    [StringLength(9, ErrorMessage = "El teléfono puede escribirse con o sin guión, no exceda el límite de caractéres.")]
    public string TelefonoLaboral { get; set; } = null!;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(60, MinimumLength = 5, ErrorMessage = "La contraseña debe tener entre 5 y 60 caracteres.")]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;
    [Display(Name = "Alcaldía")]
    public int IdAlcaldia { get; set; }
    [Display(Name = "Alcaldía")]
    public virtual Alcaldia IdAlcaldiaNavigation { get; set; } = null!;
    [Display(Name = "Referentes")]
    [NotMapped]
    public virtual ICollection<ReferentesSupervisor> ReferentesSupervisores { get; set; } = new List<ReferentesSupervisor>();
}
