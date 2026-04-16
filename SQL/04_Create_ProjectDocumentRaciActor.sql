-- SQL para crear la tabla ProjectDocumentRaciActors en Informix
-- Esta tabla almacena las entradas de la matriz RACI relacionadas a un documento de proyecto.

CREATE TABLE ProjectDocumentRaciActors (
    Id SERIAL PRIMARY KEY,
    ProjectDocumentId INTEGER NOT NULL,
    Activity TEXT,
    Type LVARCHAR(255),
    Area LVARCHAR(255),
    Role LVARCHAR(255),
    Identification LVARCHAR(255) NOT NULL,
    Username LVARCHAR(255) NOT NULL,
    CreatedAt DATETIME YEAR TO SECOND NOT NULL,
    UpdatedAt DATETIME YEAR TO SECOND,
    IsActive BOOLEAN NOT NULL,
    FOREIGN KEY (ProjectDocumentId) REFERENCES ProjectDocuments (Id) CONSTRAINT fk_ProjectDocumentRaciActor_ProjectDocument
);