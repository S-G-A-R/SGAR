using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace SGAR.AppWebMVC.Models;

public partial class NotificacionesUbicacion
{
    public int Id { get; set; }
    public int IdCiudadano { get; set; }

    public int DistanciaMetros { get; set; }
    [Required(ErrorMessage = "Seleccione Ubicación")]
    public decimal Latitud { get; set; }
    [Required(ErrorMessage = "Seleccione Ubicación")]
    public decimal Longitud { get; set; }

    public string Titulo { get; set; } = null!;

    public byte Estado { get; set; }

    public virtual Ciudadano IdCiudadanoNavigation { get; set; } = null!;
}
