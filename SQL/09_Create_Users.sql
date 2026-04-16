-- SQL para crear la tabla Users en Informix
-- Esta tabla almacena los usuarios del sistema, incluyendo su información personal, credenciales y rol asociado.

CREATE TABLE Users (
	Id SERIAL PRIMARY KEY NOT NULL,
	Name LVARCHAR(100) NULL,
	LastName LVARCHAR(100) NULL,
	Username LVARCHAR(50) NOT NULL,
	Email LVARCHAR(150) NULL,
	Password LVARCHAR(255) NULL,
	RoleId INT NOT NULL,
	CreatedAt DATETIME YEAR TO SECOND NOT NULL,
	UpdatedAt DATETIME YEAR TO SECOND NULL,
	IsActive BOOLEAN NOT NULL,
	Identification NVARCHAR(20)  NULL,
	FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE CONSTRAINT FK_Users_Roles_RoleId
);