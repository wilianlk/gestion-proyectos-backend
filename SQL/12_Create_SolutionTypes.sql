-- SQL para crear la tabla SolutionTypes en Informix
-- Esta tabla almacena las opciones administrables de tipo de solucion.

CREATE TABLE SolutionTypes (
	Id SERIAL PRIMARY KEY NOT NULL,
	Name LVARCHAR(255) NOT NULL,
	CreatedAt DATETIME YEAR TO SECOND NOT NULL,
	UpdatedAt DATETIME YEAR TO SECOND NULL,
	IsActive BOOLEAN NOT NULL
);
