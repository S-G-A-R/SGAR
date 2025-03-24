using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Alcaldia
{
    public int Id { get; set; }

    public int IdMunicipio { get; set; }

    public string Correo { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual Municipio IdMunicipioNavigation { get; set; } = null!;

    public virtual ICollection<Operador> Operadores { get; set; } = new List<Operador>();

    public virtual ICollection<Supervisor> Supervisores { get; set; } = new List<Supervisor>();

    public virtual ICollection<Zona> Zonas { get; set; } = new List<Zona>();
}
