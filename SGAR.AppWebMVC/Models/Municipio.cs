using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Municipio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdDepartamento { get; set; }

    public virtual ICollection<Alcaldia> Alcaldia { get; set; } = new List<Alcaldia>();

    public virtual ICollection<Distrito> Distritos { get; set; } = new List<Distrito>();

    public virtual Departamento IdDepartamentoNavigation { get; set; } = null!;
}
