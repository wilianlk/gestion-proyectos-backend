-- SQL para crear la tabla Roles en Informix
-- Esta tabla almacena los roles de usuario en el sistema.

CREATE TABLE Roles (
	Id SERIAL PRIMARY KEY NOT NULL,
	Name NVARCHAR(255) NOT NULL,
	Description NVARCHAR(255) NOT NULL,
	CreatedAt DATETIME YEAR TO SECOND NOT NULL,
	UpdatedAt DATETIME YEAR TO SECOND NULL,
	IsActive BOOLEAN NOT NULL
);