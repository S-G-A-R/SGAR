using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGAR.AppWebMVC.Models;

public partial class Vehiculo 
{
    public int Id { get; set; }
    [Display(Name = "Marca")]
    public int IdMarca { get; set; }
    [Required(ErrorMessage = "El numero de placa es obligatorio.")]
    public string Placa { get; set; } = null!;
    [Required(ErrorMessage = "El codigo es obligatorio.")]
    public string Codigo { get; set; } = null!;
    [Display(Name = "Tipo de vehiculo")]
    public int IdTipoVehiculo { get; set; }
    [Required(ErrorMessage = "El Nombre del mecanico es obligatorio.")]
    public string? Mecanico { get; set; }
    [Required(ErrorMessage = "El Nombre del taller es obligatorio.")]
    public string? Taller { get; set; }
    [Display(Name = "Operador")]
    public int? IdOperador { get; set; }
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public byte Estado { get; set; }

    public string? Descripcion { get; set; }


    [Display(Name = "Marca")]
    public virtual Marca IdMarcaNavigation { get; set; } = null!;
    [Display(Name = "operador")]
    public virtual Operador? IdOperadorNavigation { get; set; }
    [Display(Name = "Tipo de vehiculo")]
    public virtual TiposVehiculo IdTipoVehiculoNavigation { get; set; } = null!;

    public virtual ICollection<Operador> Operadores { get; set; } = new List<Operador>();
    public byte[]? Foto { get; set; }

    [NotMapped]
    public IFormFile? fotofile { get; set; }
}
