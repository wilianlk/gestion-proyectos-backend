-- SQL para crear la tabla DeploymentModels en Informix
-- Esta tabla almacena las opciones administrables de modelo de despliegue.

CREATE TABLE DeploymentModels (
	Id SERIAL PRIMARY KEY NOT NULL,
	Name LVARCHAR(255) NOT NULL,
	CreatedAt DATETIME YEAR TO SECOND NOT NULL,
	UpdatedAt DATETIME YEAR TO SECOND NULL,
	IsActive BOOLEAN NOT NULL
);
