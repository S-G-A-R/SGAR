using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Ubicacion
{
    public int Id { get; set; }

    public int IdOperador { get; set; }

    public decimal Latitud { get; set; }

    public decimal Longitud { get; set; }

    public string? FechaActualizacion { get; set; }

    public virtual Operador IdOperadorNavigation { get; set; } = null!;
}
