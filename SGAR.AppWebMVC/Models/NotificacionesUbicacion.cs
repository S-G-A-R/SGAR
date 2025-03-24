using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class NotificacionesUbicacion
{
    public int IdCiudadano { get; set; }

    public int DistanciaMetros { get; set; }

    public decimal Latitud { get; set; }

    public decimal Longitud { get; set; }

    public string Titulo { get; set; } = null!;

    public byte Estado { get; set; }

    public virtual Ciudadano IdCiudadanoNavigation { get; set; } = null!;
}
