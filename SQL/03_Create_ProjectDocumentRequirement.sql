-- SQL para crear la tabla ProjectDocumentRequirements en Informix
-- Esta tabla almacena los requisitos asociados a un documento de proyecto.

CREATE TABLE ProjectDocumentRequirements (
    Id SERIAL PRIMARY KEY,
    ProjectDocumentId INTEGER NOT NULL,
    Code LVARCHAR(255),
    Description TEXT,
    Type LVARCHAR(255),
    Priority LVARCHAR(255),
    AcceptanceCriteria TEXT,
    Identification LVARCHAR(255) NOT NULL,
    Username LVARCHAR(255) NOT NULL,
    CreatedAt DATETIME YEAR TO SECOND NOT NULL,
    UpdatedAt DATETIME YEAR TO SECOND,
    IsActive BOOLEAN NOT NULL,
    FOREIGN KEY (ProjectDocumentId) REFERENCES ProjectDocuments (Id) CONSTRAINT fk_ProjectDocumentRequirement_ProjectDocument
);