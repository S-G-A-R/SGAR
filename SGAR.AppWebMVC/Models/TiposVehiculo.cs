using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class TiposVehiculo
{
    public int Id { get; set; }

    public byte Tipo { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
