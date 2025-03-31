using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGAR.AppWebMVC.Models;

public partial class ReferentesOperador
{
    public int Id { get; set; }
    public int IdOperador { get; set; }
    [Required(ErrorMessage = "El Nombre es obligatorio")]
    public string Nombre { get; set; } = null!;
    [Required(ErrorMessage = "El Parentesco es obligatorio")]
    public string? Parentesco { get; set; }
    [Required(ErrorMessage = "El Tipo es obligatorio")]
    public byte Tipo { get; set; }

    public virtual Operador IdOperadorNavigation { get; set; } = null!;
    [NotMapped]
    public int NumItem { get; set; }
}
