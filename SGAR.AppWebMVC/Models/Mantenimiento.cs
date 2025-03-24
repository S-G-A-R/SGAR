using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Mantenimiento
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int IdOperador { get; set; }

    public byte[]? Archivo { get; set; }

    public string TipoSituacion { get; set; } = null!;

    public virtual Operador IdOperadorNavigation { get; set; } = null!;
}
