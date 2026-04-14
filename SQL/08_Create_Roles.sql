-- SQL para crear la tabla Roles en Informix
-- Esta tabla almacena los roles de usuario en el sistema.

CREATE TABLE Roles (
	Id SERIAL PRIMARY KEY NOT NULL,
	Name LVARCHAR(255) NOT NULL,
	Description LVARCHAR(255) NOT NULL,
	CreatedAt DATETIME YEAR TO SECOND NOT NULL,
	UpdatedAt DATETIME YEAR TO SECOND NULL,
	IsActive BOOLEAN NOT NULL
);