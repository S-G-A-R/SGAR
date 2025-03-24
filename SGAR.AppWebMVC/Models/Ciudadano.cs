using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Ciudadano
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellido { get; set; }

    public string Dui { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int ZonaId { get; set; }

    public virtual ICollection<Queja> Quejas { get; set; } = new List<Queja>();

    public virtual Zona Zona { get; set; } = null!;
}
