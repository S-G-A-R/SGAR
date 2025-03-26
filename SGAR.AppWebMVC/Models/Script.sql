-- Crear la base de datos SGAR_DB
CREATE DATABASE SGAR_DB
GO

-- Usar la base de datos SGAR_DB
USE SGAR_DB
GO

-- Crear la tabla Departamentos SI
CREATE TABLE Departamentos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50) NOT NULL
)
GO

-- Crear la tabla Municipios SI
CREATE TABLE Municipios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(80) NOT NULL,
    IdDepartamento INT NOT NULL,
    FOREIGN KEY (IdDepartamento) REFERENCES Departamentos(Id)
)
GO

-- Crear la tabla Distritos SI
CREATE TABLE Distritos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(80) NOT NULL,
    IdMunicipio INT NOT NULL,
    FOREIGN KEY (IdMunicipio) REFERENCES Municipios(Id)
)
GO

-- Crear la tabla Zonas SI
CREATE TABLE Zonas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(80) NOT NULL,
    IdDistrito INT NOT NULL,
	IdAlcaldia INT NOT NULL,
	Descripcion VARCHAR(200) NULL,
    FOREIGN KEY (IdDistrito) REFERENCES Distritos(Id)
)
GO

-- Crear la tabla Alcaldias SI
CREATE TABLE Alcaldias (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdMunicipio INT NOT NULL,
    Correo VARCHAR(255) NOT NULL UNIQUE,
    Password CHAR(64) NOT NULL, -- SHA-256
    FOREIGN KEY (IdMunicipio) REFERENCES Municipios(Id)
)
GO



-- Crear la tabla Supervisores NO
CREATE TABLE Supervisores (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(120) NOT NULL,
    Apellido VARCHAR(120) NOT NULL,
    Telefono CHAR(9) UNIQUE NULL,
    CorreoPersonal VARCHAR(255) UNIQUE,
    DUI VARCHAR(10) UNIQUE NOT NULL,
    Foto VARBINARY(MAX) NULL,
    Codigo VARCHAR(20) UNIQUE NOT NULL,
    CorreoLaboral VARCHAR(255) UNIQUE NOT NULL,
    TelefonoLaboral CHAR(9) UNIQUE NOT NULL,
    Password CHAR(64) NOT NULL,
    IdAlcaldia INT NOT NULL
	FOREIGN KEY (IdAlcaldia) REFERENCES Alcaldias(Id),
)
GO

-- Crear la tabla ReferentesSupervisores NO
CREATE TABLE ReferentesSupervisores (
	Id INT PRIMARY KEY IDENTITY(1,1),
    IdSupervisor INT NOT NULL,
    Nombre VARCHAR(120) NOT NULL,
    Parentesco VARCHAR(30) NULL,
	Tipo TINYINT NOT NULL,
    FOREIGN KEY (IdSupervisor) REFERENCES Supervisores(Id)
)
GO

-- Crear la tabla Ciudadanos SI
CREATE TABLE Ciudadanos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(120) NOT NULL,
    Apellido VARCHAR(120) NULL,
    DUI VARCHAR(10) UNIQUE NOT NULL,
    Correo VARCHAR(255) UNIQUE NOT NULL,
    Password CHAR(64) NOT NULL,
    ZonaId INT NOT NULL,
    FOREIGN KEY (ZonaId) REFERENCES Zonas(Id),
)
GO

-- Crear la tabla Quejas NO
CREATE TABLE Quejas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Titulo VARCHAR(80) NOT NULL,
    Descripcion VARCHAR(MAX),
    IdCiudadano INT NOT NULL,
    Archivo VARBINARY(MAX) NULL,
    TipoSituacion VARCHAR(20) NOT NULL,
    FOREIGN KEY (IdCiudadano) REFERENCES Ciudadanos(Id)
)
GO

-- Crear la tabla NotificacionesUbicaciones NO
CREATE TABLE NotificacionesUbicaciones (
    IdCiudadano INT NOT NULL,
    DistanciaMetros INT NOT NULL,
    Latitud DECIMAL(9, 6) NOT NULL,
    Longitud DECIMAL(9, 6) NOT NULL,
    Titulo VARCHAR(60) NOT NULL,
    Estado TINYINT NOT NULL,
    FOREIGN KEY (IdCiudadano) REFERENCES Ciudadanos(Id)
)
GO

-- Crear la tabla Operadores NO
CREATE TABLE Operadores (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(120) NOT NULL,
    Apellido VARCHAR(120) NULL,
    TelefonoPersonal CHAR(9) UNIQUE NULL,
    CorreoPersonal VARCHAR(255) UNIQUE NULL,
    DUI CHAR(10) UNIQUE NOT NULL,
    Foto VARBINARY(MAX) NULL,
    Ayudantes VARCHAR(500) NULL,
    CodigoOperador VARCHAR(20) UNIQUE NOT NULL,
    TelefonoLaboral CHAR(9) UNIQUE NOT NULL,
    CorreoLaboral VARCHAR(255) UNIQUE NOT NULL,
    VehiculoId INT NULL,
    LicenciaDoc VARBINARY(MAX) NOT NULL,
    AntecedentesDoc VARBINARY(MAX) NOT NULL,
    SolvenciaDoc VARBINARY(MAX) NOT NULL,
    Password CHAR(64) NOT NULL,
    IdAlcaldia INT NOT NULL,
    FOREIGN KEY (IdAlcaldia) REFERENCES Alcaldias(Id)
	
)
GO

-- Crear la tabla Horarios NO
CREATE TABLE Horarios (
	Id INT PRIMARY KEY IDENTITY(1,1),
    HoraEntrada TIME NOT NULL,
    HoraSalida TIME NOT NULL,
    Dia CHAR(7) NOT NULL, -- 0000000 cada dígito para un dia de semana
    IdOperador INT NOT NULL,
    Turno TINYINT NOT NULL, -- 1 = Matutino, 2 = Vespertino
    IdZona INT NOT NULL,
    FOREIGN KEY (IdOperador) REFERENCES Operadores(Id),
    FOREIGN KEY (IdZona) REFERENCES Zonas(Id)
)
GO

-- Crear la tabla Ubicaciones NO
CREATE TABLE Ubicaciones (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdHorario INT NOT NULL,
    Latitud DECIMAL(9, 6) NOT NULL,
    Longitud DECIMAL(9, 6) NOT NULL,
    FechaActualizacion DATETIME NOT NULL,
    FOREIGN KEY (IdHorario) REFERENCES Horarios(Id)
);
GO

-- Crear la tabla ReferentesOperadores NO
CREATE TABLE ReferentesOperadores (
    IdOperador INT NOT NULL,
    Nombre VARCHAR(120) NOT NULL,
    Parentesco VARCHAR(50) NULL,
	Tipo TINYINT NOT NULL,
    FOREIGN KEY (IdOperador) REFERENCES Operadores(Id)
)
GO

-- Crear la tabla Mantenimientos NO
CREATE TABLE Mantenimientos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Titulo VARCHAR(80) NOT NULL,
    Descripcion VARCHAR(MAX) NULL,
    IdOperador INT NOT NULL,
    Archivo VARBINARY(MAX) NULL,
    TipoSituacion VARCHAR(20) NOT NULL,
    FOREIGN KEY (IdOperador) REFERENCES Operadores(Id)
)
GO

-- Crear la tabla Marcas SI
CREATE TABLE Marcas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100) NOT NULL,
	Modelo VARCHAR(150) NOT NULL,
    YearOfFabrication CHAR(4) NOT NULL,
)
GO

-- Crear la tabla TiposVehiculos SI
CREATE TABLE TiposVehiculos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Tipo TINYINT NOT NULL,
	Descripcion VARCHAR(200) NOT NULL,
)
GO

-- Crear la tabla Vehiculos NO
CREATE TABLE Vehiculos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdMarca INT NOT NULL,
    Placa VARCHAR(20) UNIQUE NOT NULL,
    Codigo VARCHAR(20) UNIQUE NOT NULL,
    IdTipoVehiculo INT NOT NULL,
    Mecanico VARCHAR(120) NULL,
    Taller VARCHAR(120) NULL,
    IdOperador INT NULL,
    Estado TINYINT NOT NULL,
    Descripcion VARCHAR(500) NULL,
    Foto VARBINARY(MAX) NULL,
    FOREIGN KEY (IdMarca) REFERENCES Marcas(Id),
    FOREIGN KEY (IdTipoVehiculo) REFERENCES TiposVehiculos(Id),
    FOREIGN KEY (IdOperador) REFERENCES Operadores(Id)
)
GO

ALTER TABLE Operadores ADD FOREIGN KEY (VehiculoId) REFERENCES Vehiculos(Id)
ALTER TABLE Zonas ADD FOREIGN KEY (IdAlcaldia) REFERENCES Alcaldias(Id)
GO

INSERT INTO Departamentos(Nombre) VALUES ('TestDepartamento')
INSERT INTO Municipios(Nombre, IdDepartamento) VALUES ('TestMunicipio', 1)
INSERT INTO Distritos(Nombre, IdMunicipio) VALUES ('TestDistrito', 1)
INSERT INTO Alcaldias(IdMunicipio, Correo, Password) VALUES (1, 'admin@gmail.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92') --Password: 12345
INSERT INTO Zonas(Nombre, IdDistrito, IdAlcaldia, Descripcion) VALUES ('TestZona', 1, 1, 'Zona de Prueba')