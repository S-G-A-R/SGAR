using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGAR.AppWebMVC.Models;

public partial class TiposVehiculo
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El Tipo es obligatorio.")]
    public byte Tipo { get; set; }
    [Required(ErrorMessage = "La descripción es obligatoria.")]
    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
