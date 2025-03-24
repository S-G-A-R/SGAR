using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGAR.AppWebMVC.Models;

public partial class Marca
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El Nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;
    [Required(ErrorMessage = "El Modelo es obligatorio.")]
    public string Modelo { get; set; } = null!;
    [Required(ErrorMessage = "Ingrese el año de fabricación.")]
    [Display(Name = "Año de Fabricación")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "Ingrese el año únicamente.")]
    public string YearOfFabrication { get; set; } = null!;

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
