-- SQL para crear la tabla ProjectDocumentIntegrations en Informix
-- Esta tabla almacena las integraciones de sistemas relacionadas a un documento de proyecto.

CREATE TABLE ProjectDocumentIntegrations (
    Id SERIAL PRIMARY KEY,
    ProjectDocumentId INTEGER NOT NULL,
    System LVARCHAR(255),
    Description TEXT,
    Identification LVARCHAR(255) NOT NULL,
    Username LVARCHAR(255) NOT NULL,
    CreatedAt DATETIME YEAR TO SECOND NOT NULL,
    UpdatedAt DATETIME YEAR TO SECOND,
    IsActive BOOLEAN NOT NULL,
    FOREIGN KEY (ProjectDocumentId) REFERENCES ProjectDocuments (Id) CONSTRAINT fk_ProjectDocumentIntegration_ProjectDocument
);