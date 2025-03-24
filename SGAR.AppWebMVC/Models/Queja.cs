using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Queja
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int IdCiudadano { get; set; }

    public byte[]? Archivo { get; set; }

    public string TipoSituacion { get; set; } = null!;

    public virtual Ciudadano IdCiudadanoNavigation { get; set; } = null!;
}
