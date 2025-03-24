using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Marca
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Modelo { get; set; } = null!;

    public string YearOfFabrication { get; set; } = null!;

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
