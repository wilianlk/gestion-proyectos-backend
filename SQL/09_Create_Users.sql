-- SQL para crear la tabla Users en Informix
-- Esta tabla almacena los usuarios del sistema, incluyendo su información personal, credenciales y rol asociado.

CREATE TABLE Users (
	Id SERIAL PRIMARY KEY NOT NULL,
	Name NVARCHAR(10)  NULL,
	LastName NVARCHAR(10)  NULL,
	Username NVARCHAR(10)  NOT NULL,
	Email NVARCHAR(70)  NULL,
	Password LVARCHAR(MAX)  NULL,
	RoleId INT NOT NULL,
	CreatedAt DATETIME YEAR TO SECOND NOT NULL,
	UpdatedAt DATETIME YEAR TO SECOND NULL,
	IsActive BOOLEAN NOT NULL,
	Identification NVARCHAR(20)  NULL,
	FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE CONSTRAINT FK_Users_Roles_RoleId
);