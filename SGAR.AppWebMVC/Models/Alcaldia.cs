using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGAR.AppWebMVC.Models;

public partial class Alcaldia
{
    public int Id { get; set; }

    [Display(Name = "Municipio")]
    [Required(ErrorMessage = "Seleccione un municipio.")]
    public int IdMunicipio { get; set; }
    [Required(ErrorMessage = "El Correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]

    [Display(Name = "Correo Electrónico")]
    public string Correo { get; set; } = null!;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(60, MinimumLength = 5, ErrorMessage = "La contraseña debe tener entre 5 y 60 caracteres.")]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;
    [Display(Name = "Municipio")]
    public virtual Municipio? IdMunicipioNavigation { get; set; } = null!;

    public virtual ICollection<Operador>? Operadores { get; set; } = new List<Operador>();

    public virtual ICollection<Supervisor>? Supervisores { get; set; } = new List<Supervisor>();

    public virtual ICollection<Zona>? Zonas { get; set; } = new List<Zona>();
}
