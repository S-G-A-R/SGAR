using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Ubicacion
{
    public int Id { get; set; }

    public int IdHorario { get; set; }

    public decimal Latitud { get; set; }

    public decimal Longitud { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Horario IdHorarioNavigation { get; set; } = null!;
}
