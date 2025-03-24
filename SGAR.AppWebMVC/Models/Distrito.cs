using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGAR.AppWebMVC.Models;

public partial class Distrito
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;
    [Display(Name = "Municipio")]
    [Required(ErrorMessage = "Seleccione un municipio.")]
    public int IdMunicipio { get; set; }

    public virtual Municipio IdMunicipioNavigation { get; set; } = null!;

    public virtual ICollection<Zona> Zonas { get; set; } = new List<Zona>();
}
