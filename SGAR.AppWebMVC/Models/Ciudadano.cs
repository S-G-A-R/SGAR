using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGAR.AppWebMVC.Models;

public partial class Ciudadano
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El Nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;
    [Required(ErrorMessage = "El Apellido es obligatorio.")]
    public string? Apellido { get; set; }
    [Required(ErrorMessage = "El Dui es obligatorio.")]
    [StringLength(10, MinimumLength = 9, ErrorMessage = "El Dui puede escribirse con o sin guión, no exceda el límite de caractéres.")]
    public string Dui { get; set; } = null!;
    [Required(ErrorMessage = "El Correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    public string Correo { get; set; } = null!;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(60, MinimumLength = 5, ErrorMessage = "La contraseña debe tener entre 5 y 60 caracteres.")]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;
    [Display(Name = "Zona")]
    [Required(ErrorMessage = "Seleccione su zona (tras seleccionar departamento y municipio).")]
    public int ZonaId { get; set; }

    public virtual ICollection<Queja>? Quejas { get; set; } = new List<Queja>();
    [Display(Name = "Zona")]
    public virtual Zona? Zona { get; set; } = null!;

    [NotMapped]
    [StringLength(60, MinimumLength = 5, ErrorMessage = "La contraseña debe tener entre 5 y 60 caracteres.")]
    [Display(Name = "Confirmar Contraseña")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string? ConfirmarPassword { get; set; } = null!;
}
