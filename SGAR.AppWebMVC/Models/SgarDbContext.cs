using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SGAR.AppWebMVC.Models;

public partial class SgarDbContext : DbContext
{
    public SgarDbContext()
    {
    }

    public SgarDbContext(DbContextOptions<SgarDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alcaldia> Alcaldias { get; set; }

    public virtual DbSet<Ciudadano> Ciudadanos { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Distrito> Distritos { get; set; }

    public virtual DbSet<Horario> Horarios { get; set; }

    public virtual DbSet<Mantenimiento> Mantenimientos { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Municipio> Municipios { get; set; }

    public virtual DbSet<NotificacionesUbicacion> NotificacionesUbicaciones { get; set; }

    public virtual DbSet<Operador> Operadores { get; set; }

    public virtual DbSet<Queja> Quejas { get; set; }

    public virtual DbSet<ReferentesOperador> ReferentesOperadores { get; set; }

    public virtual DbSet<ReferentesSupervisor> ReferentesSupervisores { get; set; }

    public virtual DbSet<Supervisor> Supervisores { get; set; }

    public virtual DbSet<TiposVehiculo> TiposVehiculos { get; set; }

    public virtual DbSet<Ubicacion> Ubicaciones { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    public virtual DbSet<Zona> Zonas { get; set; }

    //Scaffold-DbContext "Data Source=localhost\SQLEXPRESS;Initial Catalog=SGAR_DB;Integrated Security=True;Encrypt=False" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alcaldia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Alcaldia__3214EC07E0EF226D");

            entity.HasIndex(e => e.Correo, "UQ__Alcaldia__60695A1900548DEE").IsUnique();

            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.Alcaldia)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Alcaldias__IdMun__4316F928");
        });

        modelBuilder.Entity<Ciudadano>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ciudadan__3214EC075FBA3A88");

            entity.HasIndex(e => e.Correo, "UQ__Ciudadan__60695A196FCB382E").IsUnique();

            entity.HasIndex(e => e.Dui, "UQ__Ciudadan__C03671B9460C3673").IsUnique();

            entity.Property(e => e.Apellido)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Dui)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DUI");
            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.Zona).WithMany(p => p.Ciudadanos)
                .HasForeignKey(d => d.ZonaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ciudadano__ZonaI__534D60F1");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departam__3214EC07AC748740");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Distrito>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Distrito__3214EC07D3045C56");

            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .IsUnicode(false);

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.Distritos)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Distritos__IdMun__3C69FB99");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Horarios__3214EC073DEB7A01");

            entity.Property(e => e.Dia)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.IdOperadorNavigation).WithMany(p => p.Horarios)
                .HasForeignKey(d => d.IdOperador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Horarios__IdOper__6383C8BA");

            entity.HasOne(d => d.IdZonaNavigation).WithMany(p => p.Horarios)
                .HasForeignKey(d => d.IdZona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Horarios__IdZona__6477ECF3");
        });

        modelBuilder.Entity<Mantenimiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Mantenim__3214EC072F6B7ED7");

            entity.Property(e => e.Descripcion).IsUnicode(false);
            entity.Property(e => e.TipoSituacion)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Titulo)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.Motivo).IsUnicode(false);

            entity.HasOne(d => d.IdOperadorNavigation).WithMany(p => p.Mantenimientos)
                .HasForeignKey(d => d.IdOperador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Mantenimi__IdOpe__6C190EBB");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marcas__3214EC073C3121E4");

            entity.Property(e => e.Modelo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.YearOfFabrication)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Municipi__3214EC071E2FAAAE");

            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .IsUnicode(false);

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Municipios)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Municipio__IdDep__398D8EEE");
        });

        modelBuilder.Entity<NotificacionesUbicacion>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Latitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Titulo)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCiudadanoNavigation).WithMany()
                .HasForeignKey(d => d.IdCiudadano)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificac__IdCiu__5812160E");
        });

        modelBuilder.Entity<Operador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Operador__3214EC07D0994805");

            entity.HasIndex(e => e.TelefonoPersonal, "UQ__Operador__0214CB2D53F4846A").IsUnique();

            entity.HasIndex(e => e.CorreoLaboral, "UQ__Operador__02761142CDDE22BA").IsUnique();

            entity.HasIndex(e => e.CodigoOperador, "UQ__Operador__62F78FE3CD629C08").IsUnique();

            entity.HasIndex(e => e.CorreoPersonal, "UQ__Operador__8E93C53CAB67FA37").IsUnique();

            entity.HasIndex(e => e.TelefonoLaboral, "UQ__Operador__9F4A49755FDBF799").IsUnique();

            entity.HasIndex(e => e.Dui, "UQ__Operador__C03671B9BE5FA982").IsUnique();

            entity.Property(e => e.Apellido)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Ayudantes)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CodigoOperador)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CorreoLaboral)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CorreoPersonal)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Dui)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("DUI");
            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TelefonoLaboral)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TelefonoPersonal)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.IdAlcaldiaNavigation).WithMany(p => p.Operadores)
                .HasForeignKey(d => d.IdAlcaldia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Operadore__IdAlc__60A75C0F");

            entity.HasOne(d => d.Vehiculo).WithMany(p => p.Operadores)
                .HasForeignKey(d => d.VehiculoId)
                .HasConstraintName("FK__Operadore__Vehic__778AC167");
        });

        modelBuilder.Entity<Queja>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Quejas__3214EC07E772BA25");

            entity.Property(e => e.Descripcion).IsUnicode(false);
            entity.Property(e => e.TipoSituacion)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Titulo)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.Motivo).IsUnicode(false);

            entity.HasOne(d => d.IdCiudadanoNavigation).WithMany(p => p.Quejas)
                .HasForeignKey(d => d.IdCiudadano)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quejas__IdCiudad__5629CD9C");
        });

        modelBuilder.Entity<ReferentesOperador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Referent__3214EC075BBA809D");

            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Parentesco)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdOperadorNavigation).WithMany()
                .HasForeignKey(d => d.IdOperador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Referente__IdOpe__693CA210");
        });

        modelBuilder.Entity<ReferentesSupervisor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Referent__3214EC076B032FCC");

            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Parentesco)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.IdSupervisorNavigation).WithMany()
                .HasForeignKey(d => d.IdSupervisor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Referente__IdSup__4E88ABD4");
        });

        modelBuilder.Entity<Supervisor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Supervis__3214EC07F51AE1BA");

            entity.HasIndex(e => e.CorreoLaboral, "UQ__Supervis__027611420C291430").IsUnique();

            entity.HasIndex(e => e.Codigo, "UQ__Supervis__06370DACE50095E5").IsUnique();

            entity.HasIndex(e => e.Telefono, "UQ__Supervis__4EC50480BBD24D80").IsUnique();

            entity.HasIndex(e => e.CorreoPersonal, "UQ__Supervis__8E93C53C516C03A5").IsUnique();

            entity.HasIndex(e => e.TelefonoLaboral, "UQ__Supervis__9F4A4975B2F17CB4").IsUnique();

            entity.HasIndex(e => e.Dui, "UQ__Supervis__C03671B9E407267D").IsUnique();

            entity.Property(e => e.Apellido)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CorreoLaboral)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CorreoPersonal)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Dui)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DUI");
            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Telefono)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TelefonoLaboral)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.IdAlcaldiaNavigation).WithMany(p => p.Supervisores)
                .HasForeignKey(d => d.IdAlcaldia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Superviso__IdAlc__4BAC3F29");
        });

        modelBuilder.Entity<TiposVehiculo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TiposVeh__3214EC07F27CB684");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ubicacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ubicacio__3214EC0718E1B337");

            entity.Property(e => e.FechaActualizacion).HasColumnType("datetime");
            entity.Property(e => e.Latitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(9, 6)");

            entity.HasOne(d => d.IdHorarioNavigation).WithMany(p => p.Ubicaciones)
                .HasForeignKey(d => d.IdHorario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ubicacion__IdHor__6754599E");
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vehiculo__3214EC077347421F");

            entity.HasIndex(e => e.Codigo, "UQ__Vehiculo__06370DAC6113F1CF").IsUnique();

            entity.HasIndex(e => e.Placa, "UQ__Vehiculo__8310F99D569B3D15").IsUnique();

            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Mecanico)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Placa)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Taller)
                .HasMaxLength(120)
                .IsUnicode(false);

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.IdMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vehiculos__IdMar__74AE54BC");

            entity.HasOne(d => d.IdOperadorNavigation).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.IdOperador)
                .HasConstraintName("FK__Vehiculos__IdOpe__76969D2E");

            entity.HasOne(d => d.IdTipoVehiculoNavigation).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.IdTipoVehiculo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vehiculos__IdTip__75A278F5");
        });

        modelBuilder.Entity<Zona>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Zonas__3214EC07F93907F5");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .IsUnicode(false);

            entity.HasOne(d => d.IdAlcaldiaNavigation).WithMany(p => p.Zonas)
                .HasForeignKey(d => d.IdAlcaldia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Zonas__IdAlcaldi__787EE5A0");

            entity.HasOne(d => d.IdDistritoNavigation).WithMany(p => p.Zonas)
                .HasForeignKey(d => d.IdDistrito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Zonas__IdDistrit__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
