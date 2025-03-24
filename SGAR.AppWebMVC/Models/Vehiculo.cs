using System;
using System.Collections.Generic;

namespace SGAR.AppWebMVC.Models;

public partial class Vehiculo
{
    public int Id { get; set; }

    public int IdMarca { get; set; }

    public string Placa { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public int IdTipoVehiculo { get; set; }

    public string? Mecanico { get; set; }

    public string? Taller { get; set; }

    public int? IdOperador { get; set; }

    public byte Estado { get; set; }

    public string? Descripcion { get; set; }

    public byte[]? Foto { get; set; }

    public virtual Marca IdMarcaNavigation { get; set; } = null!;

    public virtual Operador? IdOperadorNavigation { get; set; }

    public virtual TiposVehiculo IdTipoVehiculoNavigation { get; set; } = null!;

    public virtual ICollection<Operador> Operadores { get; set; } = new List<Operador>();
}
