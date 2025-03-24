using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGAR.AppWebMVC.Models;

public partial class Municipio
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;
    [Display(Name = "Departamento")]
    [Required(ErrorMessage = "Seleccione un departamento.")]
    public int IdDepartamento { get; set; }

    public virtual ICollection<Alcaldia> Alcaldia { get; set; } = new List<Alcaldia>();

    public virtual ICollection<Distrito> Distritos { get; set; } = new List<Distrito>();

    public virtual Departamento IdDepartamentoNavigation { get; set; } = null!;
}
