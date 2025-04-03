using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGAR.AppWebMVC.Models;

public partial class Horario
{
    public int Id { get; set; }
    [Required(ErrorMessage = "La hora de entrada es obligatoria.")]
    public TimeOnly HoraEntrada { get; set; }

    [Required(ErrorMessage = "La hora de salida es obligatoria")]
    public TimeOnly HoraSalida { get; set; }

    [Required(ErrorMessage = "El día es obligatorio.")]
    public string Dia { get; set; } = null!;

    [Required(ErrorMessage = "El nombre del operador es obligatorio.")]
    [Display(Name = "Operador")]
    public int IdOperador { get; set; }

    [Required(ErrorMessage = "El turno es obligatorio.")]
    public byte Turno { get; set; }

    [Required(ErrorMessage = "La zona es obligatoria.")]
    [Display(Name = "Zona")]
    public int IdZona { get; set; }

    public virtual Operador? IdOperadorNavigation { get; set; } = null!;

    public virtual Zona? IdZonaNavigation { get; set; } = null!;

    
}
