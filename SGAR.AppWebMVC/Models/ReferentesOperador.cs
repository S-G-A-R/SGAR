using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class ReferentesOperador
{
    public int IdOperador { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Parentesco { get; set; }

    public byte Tipo { get; set; }

    public virtual Operador IdOperadorNavigation { get; set; } = null!;
}
