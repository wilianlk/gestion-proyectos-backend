-- SQL para crear la tabla ProjectDocumentRequirements en Informix
-- Esta tabla almacena los requisitos asociados a un documento de proyecto.

CREATE TABLE ProjectDocumentRequirements (
    Id SERIAL PRIMARY KEY,
    ProjectDocumentId INTEGER NOT NULL,
    Code VARCHAR(255),
    Description TEXT,
    Type VARCHAR(255),
    Priority VARCHAR(255),
    AcceptanceCriteria TEXT,
    Identification VARCHAR(255) NOT NULL,
    Username VARCHAR(255) NOT NULL,
    CreatedAt DATETIME YEAR TO SECOND NOT NULL,
    UpdatedAt DATETIME YEAR TO SECOND,
    IsActive BOOLEAN NOT NULL,
    FOREIGN KEY (ProjectDocumentId) REFERENCES ProjectDocuments (Id) CONSTRAINT fk_ProjectDocumentRequirement_ProjectDocument
);