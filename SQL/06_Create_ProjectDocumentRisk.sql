-- SQL para crear la tabla ProjectDocumentRisks en Informix
-- Esta tabla almacena los riesgos asociados a un documento de proyecto.

CREATE TABLE ProjectDocumentRisks (
    Id SERIAL PRIMARY KEY,
    ProjectDocumentId INTEGER NOT NULL,
    Risk LVARCHAR(255),
    Impact TEXT,
    Probability LVARCHAR(255),
    Mitigation TEXT,
    Owner LVARCHAR(255),
    Identification LVARCHAR(255) NOT NULL,
    Username LVARCHAR(255) NOT NULL,
    CreatedAt DATETIME YEAR TO SECOND NOT NULL,
    UpdatedAt DATETIME YEAR TO SECOND,
    IsActive BOOLEAN NOT NULL,
    FOREIGN KEY (ProjectDocumentId) REFERENCES ProjectDocuments (Id) CONSTRAINT fk_ProjectDocumentRisk_ProjectDocument
);