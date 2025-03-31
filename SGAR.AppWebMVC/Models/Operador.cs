﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGAR.AppWebMVC.Models;

public partial class Operador
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El Nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;
    public string? Apellido { get; set; }

    public string? TelefonoPersonal { get; set; }
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    public string? CorreoPersonal { get; set; }
    [Required(ErrorMessage = "El Dui es obligatorio.")]
    [StringLength(10, MinimumLength = 9, ErrorMessage = "El Dui puede escribirse con o sin guión, no exceda el límite de caractéres.")]
    public string Dui { get; set; } = null!;

    public byte[]? Foto { get; set; }

    public string? Ayudantes { get; set; }
    [Required(ErrorMessage = "El Código es obligatorio.")]
    public string CodigoOperador { get; set; } = null!;

    public string TelefonoLaboral { get; set; } = null!;
    [Required(ErrorMessage = "El Correo Laboral es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    public string CorreoLaboral { get; set; } = null!;

    [Display(Name = "Vehiculo")]
    public int? VehiculoId { get; set; }
    [Display(Name = "Licencia de Conducir")]
    [Required(ErrorMessage = "La Licencia de Conducir es obligatoria.")]
    public byte[] LicenciaDoc { get; set; } = null!;
    [Display(Name = "Antecedentes Penales")]
    [Required(ErrorMessage = "El Correo Laboral son obligatorios.")]
    public byte[] AntecedentesDoc { get; set; } = null!;
    [Display(Name = "Solvencia PNC")]
    [Required(ErrorMessage = "La Solvencia es obligatoria.")]
    public byte[] SolvenciaDoc { get; set; } = null!;

    [NotMapped]
    public IFormFile? SolvenciaFile { get; set; }

    [NotMapped]
    public IFormFile? LicenciaFile { get; set; }

    [NotMapped]
    public IFormFile? AntecedentesFile { get; set; }

    [NotMapped]
    public IFormFile? FotoFile { get; set; }
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(60, MinimumLength = 5, ErrorMessage = "La contraseña debe tener entre 5 y 60 caracteres.")]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;


    [Display(Name = "Alcaldia")]
    public int IdAlcaldia { get; set; }

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    [Display(Name = "Alcaldia")]
    public virtual Alcaldia IdAlcaldiaNavigation { get; set; } = null!;

    public virtual ICollection<Mantenimiento> Mantenimientos { get; set; } = new List<Mantenimiento>();

    public virtual Vehiculo? Vehiculo { get; set; }

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    [NotMapped]
    public virtual ICollection<ReferentesOperador> ReferentesOperador { get; set; } = new List<ReferentesOperador>();
}
