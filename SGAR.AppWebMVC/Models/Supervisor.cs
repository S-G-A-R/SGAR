using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Supervisor
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? CorreoPersonal { get; set; }

    public string Dui { get; set; } = null!;

    public byte[]? Foto { get; set; }

    public string Codigo { get; set; } = null!;

    public string CorreoLaboral { get; set; } = null!;

    public string TelefonoLaboral { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int IdAlcaldia { get; set; }

    public virtual Alcaldia IdAlcaldiaNavigation { get; set; } = null!;

    public virtual ICollection<ReferentesSupervisor> ReferentesSupervisores { get; set; } = new List<ReferentesSupervisor>();
}
