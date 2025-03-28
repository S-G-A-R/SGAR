﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGAR.AppWebMVC.Models;

public partial class Ciudadano
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellido { get; set; }
    [Required(ErrorMessage = "El Dui es obligatorio.")]
    [StringLength(10, MinimumLength = 9, ErrorMessage = "El Dui puede escribirse con o sin guión, no exceda el límite de caractéres.")]
    public string Dui { get; set; } = null!;
    [Required(ErrorMessage = "El Correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    public string Correo { get; set; } = null!;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(60, MinimumLength = 5, ErrorMessage = "La contraseña debe tener entre 5 y 60 caracteres.")]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;
    [Display(Name = "Zona")]
    [Required(ErrorMessage = "Seleccione su zona (tras seleccionar departamento y municipio).")]
    public int ZonaId { get; set; }

    public virtual ICollection<Queja> Quejas { get; set; } = new List<Queja>();

    public virtual Zona Zona { get; set; } = null!;
}
