﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGAR.AppWebMVC.Models;

public partial class ReferentesSupervisor
{
    public int Id { get; set; }

    public int IdSupervisor { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Parentesco { get; set; }

    public byte Tipo { get; set; }

    public virtual Supervisor IdSupervisorNavigation { get; set; } = null!;
    [NotMapped]
    public int NumItem { get; set; }
}
