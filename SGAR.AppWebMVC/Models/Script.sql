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

ALTER TABLE ReferentesOperadores
ADD Id INT PRIMARY KEY IDENTITY(1,1);

ALTER TABLE Zonas
ADD CONSTRAINT FK_Zonas_Alcaldias
FOREIGN KEY (IdAlcaldia)
REFERENCES Alcaldias(Id)
ON DELETE CASCADE
ON UPDATE CASCADE
GO

ALTER TABLE Supervisores
ADD CONSTRAINT FK_Supervisores_Alcaldias
FOREIGN KEY (IdAlcaldia)
REFERENCES Alcaldias(Id)
ON DELETE CASCADE
ON UPDATE CASCADE
GO

ALTER TABLE Quejas
ADD CONSTRAINT FK_Quejas_Ciudadanos
FOREIGN KEY (IdCiudadano)
REFERENCES Ciudadanos(Id)
ON DELETE CASCADE
ON UPDATE CASCADE
GO

ALTER TABLE NotificacionesUbicaciones
ADD CONSTRAINT FK_NotificacionesUbicaciones_Ciudadanos
FOREIGN KEY (IdCiudadano)
REFERENCES Ciudadanos(Id)
ON DELETE CASCADE
ON UPDATE CASCADE
GO

ALTER TABLE Ciudadanos
ADD CONSTRAINT FK_Ciudadanos_Zonas
FOREIGN KEY (ZonaId)
REFERENCES Zonas(Id)
ON UPDATE CASCADE
GO

ALTER TABLE Horarios
ADD CONSTRAINT FK_Horarios_Zonas
FOREIGN KEY (IdZona)
REFERENCES Zonas(Id)
GO



ALTER TABLE Operadores
ADD CONSTRAINT FK_Operadores_Alcaldias
FOREIGN KEY (IdAlcaldia)
REFERENCES Alcaldias(Id)
ON DELETE CASCADE
ON UPDATE CASCADE
GO

ALTER TABLE Mantenimientos
ADD CONSTRAINT FK_Mantenimientos_Operadores
FOREIGN KEY (IdOperador)
REFERENCES Operadores(Id)
ON DELETE CASCADE
ON UPDATE CASCADE
GO

ALTER TABLE ReferentesOperadores
ADD CONSTRAINT FK_ReferentesOperadores_Operadores
FOREIGN KEY (IdOperador)
REFERENCES Operadores(Id)
ON DELETE CASCADE
ON UPDATE CASCADE
GO

ALTER TABLE ReferentesSupervisores
ADD CONSTRAINT FK_ReferentesSupervisores_Supervisores
FOREIGN KEY (IdSupervisor)
REFERENCES Supervisores(Id)
ON DELETE CASCADE
ON UPDATE CASCADE
GO

ALTER TABLE Vehiculos
ADD CONSTRAINT FK_Vehiculos_Marcas
FOREIGN KEY (IdMarca)
REFERENCES Marcas(Id)
ON DELETE CASCADE
ON UPDATE CASCADE
GO

ALTER TABLE Operadores ADD FOREIGN KEY (VehiculoId) REFERENCES Vehiculos(Id)
ALTER TABLE Zonas ADD FOREIGN KEY (IdAlcaldia) REFERENCES Alcaldias(Id)
GO

INSERT INTO Departamentos(Nombre) VALUES ('TestDepartamento')
INSERT INTO Municipios(Nombre, IdDepartamento) VALUES ('TestMunicipio', 1)
INSERT INTO Distritos(Nombre, IdMunicipio) VALUES ('TestDistrito', 1)
INSERT INTO Alcaldias(IdMunicipio, Correo, Password) VALUES (1, 'admin@gmail.com', '5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5') --Password: 12345
INSERT INTO Zonas(Nombre, IdDistrito, IdAlcaldia, Descripcion) VALUES ('TestZona', 1, 1, 'Zona de Prueba')

INSERT INTO Departamentos(Nombre) VALUES
('Ahuachapán'),
('Cabañas'),
('Chalatenango'),
('Cuscatlán'),
('La Libertad'),
('Morazán'),
('La Paz'),
('La Unión'),
('Santa Ana'),
('San Miguel'),
('San Salvador'),
('San Vicente'),
('Sonsonate'),
('Usulután')
GO

INSERT INTO Municipios (Nombre, IdDepartamento) VALUES
('Ahuachapán Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'Ahuachapán')),
('Ahuachapán Centro', (SELECT Id FROM Departamentos WHERE Nombre = 'Ahuachapán')),
('Ahuachapán Sur', (SELECT Id FROM Departamentos WHERE Nombre = 'Ahuachapán')),
('Cabañas Este', (SELECT Id FROM Departamentos WHERE Nombre = 'Cabañas')),
('Cabañas Oeste', (SELECT Id FROM Departamentos WHERE Nombre = 'Cabañas')),
('Chalatenango Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'Chalatenango')),
('Chalatenango Centro', (SELECT Id FROM Departamentos WHERE Nombre = 'Chalatenango')),
('Chalatenango Sur', (SELECT Id FROM Departamentos WHERE Nombre = 'Chalatenango')),
('Cuscatlán Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'Cuscatlán')),
('Cuscatlán Sur', (SELECT Id FROM Departamentos WHERE Nombre = 'Cuscatlán')),
('La Libertad Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'La Libertad')),
('La Libertad Centro', (SELECT Id FROM Departamentos WHERE Nombre = 'La Libertad')),
('La Libertad Oeste', (SELECT Id FROM Departamentos WHERE Nombre = 'La Libertad')),
('La Libertad Este', (SELECT Id FROM Departamentos WHERE Nombre = 'La Libertad')),
('La Libertad Costa', (SELECT Id FROM Departamentos WHERE Nombre = 'La Libertad')),
('La Libertad Sur', (SELECT Id FROM Departamentos WHERE Nombre = 'La Libertad')),
('La Paz Oeste', (SELECT Id FROM Departamentos WHERE Nombre = 'La Paz')),
('La Paz Centro', (SELECT Id FROM Departamentos WHERE Nombre = 'La Paz')),
('La Paz Este', (SELECT Id FROM Departamentos WHERE Nombre = 'La Paz')),
('La Unión Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'La Unión')),
('La Unión Sur', (SELECT Id FROM Departamentos WHERE Nombre = 'La Unión')),
('Morazán Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'Morazán')),
('Morazán Sur', (SELECT Id FROM Departamentos WHERE Nombre = 'Morazán')),
('San Miguel Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'San Miguel')),
('San Miguel Centro', (SELECT Id FROM Departamentos WHERE Nombre = 'San Miguel')),
('San Miguel Oeste', (SELECT Id FROM Departamentos WHERE Nombre = 'San Miguel')),
('San Salvador Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'San Salvador')),
('San Salvador Oeste', (SELECT Id FROM Departamentos WHERE Nombre = 'San Salvador')),
('San Salvador Este', (SELECT Id FROM Departamentos WHERE Nombre = 'San Salvador')),
('San Salvador Centro', (SELECT Id FROM Departamentos WHERE Nombre = 'San Salvador')),
('San Salvador Sur', (SELECT Id FROM Departamentos WHERE Nombre = 'San Salvador')),
('San Vicente Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'San Vicente')),
('San Vicente Sur', (SELECT Id FROM Departamentos WHERE Nombre = 'San Vicente')),
('Santa Ana Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'Santa Ana')),
('Santa Ana Centro', (SELECT Id FROM Departamentos WHERE Nombre = 'Santa Ana')),
('Santa Ana Este', (SELECT Id FROM Departamentos WHERE Nombre = 'Santa Ana')),
('Santa Ana Oeste', (SELECT Id FROM Departamentos WHERE Nombre = 'Santa Ana')),
('Sonsonate Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'Sonsonate')),
('Sonsonate Centro', (SELECT Id FROM Departamentos WHERE Nombre = 'Sonsonate')),
('Sonsonate Este', (SELECT Id FROM Departamentos WHERE Nombre = 'Sonsonate')),
('Sonsonate Oeste', (SELECT Id FROM Departamentos WHERE Nombre = 'Sonsonate')),
('Usulután Norte', (SELECT Id FROM Departamentos WHERE Nombre = 'Usulután')),
('Usulután Este', (SELECT Id FROM Departamentos WHERE Nombre = 'Usulután')),
('Usulután Oeste', (SELECT Id FROM Departamentos WHERE Nombre = 'Usulután'))
GO

INSERT INTO Distritos(Nombre, IdMunicipio) VALUES 
('Atiquizaya', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Norte')),
('El Refugio', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Norte')),
('San Lorenzo', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Norte')),
('Turín', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Norte')),
('Ahuachapán', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Centro')),
('Apaneca', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Centro')),
('Concepción de Ataco', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Centro')),
('Tacuba', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Centro')),
('Guaymango', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Sur')),
('Jujutla', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Sur')),
('San Francisco Menéndez', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Sur')),
('San Pedro Puxtla', (SELECT Id FROM Municipios WHERE Nombre = 'Ahuachapán Sur')),
('Guacotecti', (SELECT Id FROM Municipios WHERE Nombre = 'Cabañas Este')),
('San Isidro', (SELECT Id FROM Municipios WHERE Nombre = 'Cabañas Este')),
('Sensuntepeque', (SELECT Id FROM Municipios WHERE Nombre = 'Cabañas Este')),
('Victoria', (SELECT Id FROM Municipios WHERE Nombre = 'Cabañas Este')),
('Dolores', (SELECT Id FROM Municipios WHERE Nombre = 'Cabañas Este')),
('Cinquera', (SELECT Id FROM Municipios WHERE Nombre = 'Cabañas Oeste')),
('Ilobasco', (SELECT Id FROM Municipios WHERE Nombre = 'Cabañas Oeste')),
('Jutiapa', (SELECT Id FROM Municipios WHERE Nombre = 'Cabañas Oeste')),
('Tejutepeque', (SELECT Id FROM Municipios WHERE Nombre = 'Cabañas Oeste')),
('Citalá', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Norte')),
('La Palma', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Norte')),
('San Ignacio', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Norte')),
('Agua Caliente', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('Dulce Nombre de María', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('El Paraíso', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('La Reina', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('Nueva Concepción', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('San Fernando', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('San Francisco Morazán', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('San Rafael', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('Santa Rita', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('Tejutla', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Centro')),
('Arcatao', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Azacualpa', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Cancasque', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Chalatenango', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Comalapa', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Concepción Quezaltepeque', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('El Carrizal', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('La Laguna', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Las Vueltas', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Las Flores', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Nombre de Jesús', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Nueva Trinidad', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Ojos de Agua', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Potonico', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('San Antonio de la Cruz', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('San Antonio Los Ranchos', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('San Francisco Lempa', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('San Isidro Labrador', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('San Luis del Carmen', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('San Miguel de Mercedes', (SELECT Id FROM Municipios WHERE Nombre = 'Chalatenango Sur')),
('Suchitoto', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Norte')),
('San José Guayabal', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Norte')),
('Oratorio de Concepción', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Norte')),
('San Bartolomé Perulapía', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Norte')),
('San Pedro Perulapán', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Norte')),
('Cojutepeque', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('Candelaria', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('El Carmen', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('El Rosario', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('Monte San Juan', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('San Cristóbal', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('San Rafael Cedros', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('San Ramón', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('Santa Cruz Analquito', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('Santa Cruz Michapa', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('Tenancingo', (SELECT Id FROM Municipios WHERE Nombre = 'Cuscatlán Sur')),
('Quezaltepeque', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Norte')),
('San Matías', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Norte')),
('San Pablo Tacachico', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Norte')),
('San Juan Opico', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Centro')),
('Ciudad Arce', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Centro')),
('Colón', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Oeste')),
('Jayaque', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Oeste')),
('Sacacoyo', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Oeste')),
('Tepecoyo', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Oeste')),
('Talnique', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Oeste')),
('Antiguo Cuscatlán', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Este')),
('Huizúcar', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Este')),
('Nuevo Cuscatlán', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Este')),
('San José Villanueva', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Este')),
('Zaragoza', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Este')),
('Chiltiupán', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Costa')),
('Jicalapa', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Costa')),
('La Libertad', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Costa')),
('Tamanique', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Costa')),
('Teotepeque', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Costa')),
('Santa Tecla', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Sur')),
('Comasagua', (SELECT Id FROM Municipios WHERE Nombre = 'La Libertad Sur')),
('Cuyultitán', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Oeste')),
('Olocuilta', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Oeste')),
('San Juan Talpa', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Oeste')),
('San Luis Talpa', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Oeste')),
('San Pedro Masahuat', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Oeste')),
('Tapalhuaca', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Oeste')),
('San Francisco Chinameca', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Oeste')),
('El Rosario', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('Jerusalén', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('Mercedes La Ceiba', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('Paraíso de Osorio', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('San Antonio Masahuat', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('San Emigdio', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('San Juan Tepezontes', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('San Luis La Herradura', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('San Miguel Tepezontes', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('San Pedro Nonualco', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('Santa María Ostuma', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('Santiago Nonualco', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Centro')),
('San Juan Nonualco', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Este')),
('San Rafael Obrajuelo', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Este')),
('Zacatecoluca', (SELECT Id FROM Municipios WHERE Nombre = 'La Paz Este')),
('Anamorós', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('Bolívar', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('Concepción de Oriente', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('El Sauce', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('Lislique', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('Nueva Esparta', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('Pasaquina', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('Polorós', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('San José', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('Santa Rosa de Lima', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Norte')),
('Conchagua', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Sur')),
('El Carmen', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Sur')),
('Intipucá', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Sur')),
('La Unión', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Sur')),
('Meanguera del Golfo', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Sur')),
('San Alejo', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Sur')),
('Yayantique', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Sur')),
('Yucuaiquín', (SELECT Id FROM Municipios WHERE Nombre = 'La Unión Sur')),
('Arambala', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('Cacaopera', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('Corinto', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('El Rosario', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('Joateca', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('Jocoaitique', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('Meanguera', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('Perquín', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('San Fernando', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('San Isidro', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('Torola', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Norte')),
('Chilanga', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Delicias de Concepción', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('El Divisadero', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Gualococti', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Guatajiagua', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Jocoro', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Lolotiquillo', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Osicala', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('San Carlos', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('San Francisco Gotera', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('San Simón', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Sensembra', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Sociedad', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Yamabal', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Yoloaiquín', (SELECT Id FROM Municipios WHERE Nombre = 'Morazán Sur')),
('Ciudad Barrios', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Norte')),
('Sesori', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Norte')),
('Nuevo Edén de San Juan', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Norte')),
('San Gerardo', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Norte')),
('San Luis de la Reina', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Norte')),
('Carolina', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Norte')),
('San Antonio', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Norte')),
('Chapeltique', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Norte')),
('San Miguel', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Centro')),
('Comacarán', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Centro')),
('Uluazapa', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Centro')),
('Moncagua', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Centro')),
('Quelepa', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Centro')),
('Chirilagua', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Centro')),
('Chinameca', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Oeste')),
('El Tránsito', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Oeste')),
('Lolotique', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Oeste')),
('Nueva Guadalupe', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Oeste')),
('San Jorge', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Oeste')),
('San Rafael Oriente', (SELECT Id FROM Municipios WHERE Nombre = 'San Miguel Oeste')),
('Aguilares', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Norte')),
('El Paisnal', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Norte')),
('Guazapa', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Norte')),
('Apopa', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Oeste')),
('Nejapa', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Oeste')),
('Ilopango', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Este')),
('San Martín', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Este')),
('Soyapango', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Este')),
('Tonacatepeque', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Este')),
('Ayutuxtepeque', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Centro')),
('Mejicanos', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Centro')),
('Cuscatancingo', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Centro')),
('Ciudad Delgado', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Centro')),
('San Salvador', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Centro')),
('San Marcos', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Sur')),
('Santo Tomás', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Sur')),
('Santiago Texacuangos', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Sur')),
('Rosario de Mora', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Sur')),
('Panchimalco', (SELECT Id FROM Municipios WHERE Nombre = 'San Salvador Sur')),
('Apastepeque', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Norte')),
('Santa Clara', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Norte')),
('San Ildefonso', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Norte')),
('San Esteban Catarina', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Norte')),
('San Sebastián', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Norte')),
('San Lorenzo', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Norte')),
('Santo Domingo', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Norte')),
('San Vicente', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Sur')),
('Guadalupe', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Sur')),
('San Cayetano Istepeque', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Sur')),
('Tecoluca', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Sur')),
('Tepetitán', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Sur')),
('Verapaz', (SELECT Id FROM Municipios WHERE Nombre = 'San Vicente Sur')),
('Masahuat', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Norte')),
('Metapán', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Norte')),
('Santa Rosa Guachipilín', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Norte')),
('Texistepeque', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Norte')),
('Santa Ana', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Centro')),
('El Congo', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Este')),
('Coatepeque', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Este')),
('Candelaria de la Frontera', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Oeste')),
('Chalchuapa', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Oeste')),
('El Porvenir', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Oeste')),
('San Antonio Pajonal', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Oeste')),
('San Sebastián Salitrillo', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Oeste')),
('Santiago de la Frontera', (SELECT Id FROM Municipios WHERE Nombre = 'Santa Ana Oeste')),
('Juayúa', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Norte')),
('Nahuizalco', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Norte')),
('Salcoatitán', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Norte')),
('Santa Catarina Masahuat', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Norte')),
('Sonsonate', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Centro')),
('Sonzacate', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Centro')),
('San Antonio del Monte', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Centro')),
('Santo Domingo de Guzmán', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Centro')),
('Nahulingo', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Centro')),
('Armenia', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Este')),
('Caluco', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Este')),
('Cuisnahuat', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Este')),
('Izalco', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Este')),
('San Julián', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Este')),
('Santa Isabel Ishuatán', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Este')),
('Acajutla', (SELECT Id FROM Municipios WHERE Nombre = 'Sonsonate Oeste')),
('Alegría', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Norte')),
('Berlín', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Norte')),
('El Triunfo', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Norte')),
('Estanzuelas', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Norte')),
('Jucuapa', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Norte')),
('Mercedes Umaña', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Norte')),
('Nueva Granada', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Norte')),
('San Buenaventura', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Norte')),
('Santiago de María', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Norte')),
('California', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('Concepción Batres', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('Ereguayquín', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('Jucuarán', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('Ozatlán', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('Santa Elena', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('San Dionisio', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('Santa María', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('Tecapán', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('Usulután', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Este')),
('Jiquilisco', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Oeste')),
('Puerto El Triunfo', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Oeste')),
('San Agustín', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Oeste')),
('San Francisco Javier', (SELECT Id FROM Municipios WHERE Nombre = 'Usulután Oeste'))
GO
