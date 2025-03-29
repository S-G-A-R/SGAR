using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGAR.AppWebMVC.Models;

public partial class Zona
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;
    [Display(Name = "Distrito")]
    [Required(ErrorMessage = "Seleccione un distrito.")]
    public int IdDistrito { get; set; }
    [Display(Name = "Alcaldia")]
    [Required(ErrorMessage = "Asocie la alcaldía.")]
    public int IdAlcaldia { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Ciudadano> Ciudadanos { get; set; } = new List<Ciudadano>();

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();
    [Display(Name = "Alcaldia")]
    public virtual Alcaldia? IdAlcaldiaNavigation { get; set; } = null!;
    [Display(Name = "Distrito")]
    public virtual Distrito? IdDistritoNavigation { get; set; } = null!;
}
