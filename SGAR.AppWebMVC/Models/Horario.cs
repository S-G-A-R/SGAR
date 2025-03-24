using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Horario
{
    public int Id { get; set; }

    public TimeOnly HoraEntrada { get; set; }

    public TimeOnly HoraSalida { get; set; }

    public string Dia { get; set; } = null!;

    public int IdOperador { get; set; }

    public byte Turno { get; set; }

    public int IdZona { get; set; }

    public virtual Operador IdOperadorNavigation { get; set; } = null!;

    public virtual Zona IdZonaNavigation { get; set; } = null!;

    public virtual ICollection<Ubicacion> Ubicaciones { get; set; } = new List<Ubicacion>();
}
