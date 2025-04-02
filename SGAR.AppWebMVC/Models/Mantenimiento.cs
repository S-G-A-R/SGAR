using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGAR.AppWebMVC.Models;

public partial class Mantenimiento
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El Título es obligatorio")]
    [MaxLength(80, ErrorMessage = "Máximo 80 caracteres")]
    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }
    [Display(Name = "Operador")]
    public int IdOperador { get; set; }
    [NotMapped]
    public IFormFile? File { get; set; }
    public byte[]? Archivo { get; set; }
    [Required(ErrorMessage = "El tipo de su queja es requerido")]
    [Display(Name = "Tipo de Situación")]
    [MaxLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string TipoSituacion { get; set; } = null!;
    public byte Estado { get; set; }
    public string Motivo { get; set; } = null!;
    [Display(Name = "Operador")]
    public virtual Operador IdOperadorNavigation { get; set; } = null!;
}
