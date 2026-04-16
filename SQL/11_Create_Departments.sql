-- SQL para crear la tabla Departments en Informix
-- Esta tabla almacena las áreas del negocio.

CREATE TABLE Departments (
	Id SERIAL PRIMARY KEY NOT NULL,
	Name LVARCHAR(255) NOT NULL,
	CreatedAt DATETIME YEAR TO SECOND NOT NULL,
	UpdatedAt DATETIME YEAR TO SECOND NULL,
	IsActive BOOLEAN NOT NULL
);