using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Distrito
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdMunicipio { get; set; }

    public virtual Municipio IdMunicipioNavigation { get; set; } = null!;

    public virtual ICollection<Zona> Zonas { get; set; } = new List<Zona>();
}
