using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Zona
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdDistrito { get; set; }

    public int IdAlcaldia { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Ciudadano> Ciudadanos { get; set; } = new List<Ciudadano>();

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    public virtual Alcaldia IdAlcaldiaNavigation { get; set; } = null!;

    public virtual Distrito IdDistritoNavigation { get; set; } = null!;
}
