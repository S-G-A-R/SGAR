using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGAR.AppWebMVC.Models;

public partial class Operador
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellido { get; set; }

    public string? TelefonoPersonal { get; set; }

    public string? CorreoPersonal { get; set; }

    public string Dui { get; set; } = null!;

    public byte[]? Foto { get; set; }

    public string? Ayudantes { get; set; }

    public string CodigoOperador { get; set; } = null!;

    public string TelefonoLaboral { get; set; } = null!;

    public string CorreoLaboral { get; set; } = null!;
    [Display(Name = "Vehiculo")]
    public int? VehiculoId { get; set; }

    public byte[] LicenciaDoc { get; set; } = null!;

    public byte[] AntecedentesDoc { get; set; } = null!;

    public byte[] SolvenciaDoc { get; set; } = null!;

    public string Password { get; set; } = null!;


    [Display(Name = "Alcaldia")]
    public int IdAlcaldia { get; set; }

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    [Display(Name = "Alcaldia")]
    public virtual Alcaldia IdAlcaldiaNavigation { get; set; } = null!;

    public virtual ICollection<Mantenimiento> Mantenimientos { get; set; } = new List<Mantenimiento>();

    public virtual Vehiculo? Vehiculo { get; set; }

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
