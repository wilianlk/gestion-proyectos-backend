-- SQL para crear la tabla ProjectDocumentRisks en Informix
-- Esta tabla almacena los riesgos asociados a un documento de proyecto.

CREATE TABLE ProjectDocumentRisks (
    Id SERIAL PRIMARY KEY,
    ProjectDocumentId INTEGER NOT NULL,
    Risk VARCHAR(255),
    Impact TEXT,
    Probability VARCHAR(255),
    Mitigation TEXT,
    Owner VARCHAR(255),
    Identification VARCHAR(255) NOT NULL,
    Username VARCHAR(255) NOT NULL,
    CreatedAt DATETIME YEAR TO SECOND NOT NULL,
    UpdatedAt DATETIME YEAR TO SECOND,
    IsActive BOOLEAN NOT NULL,
    FOREIGN KEY (ProjectDocumentId) REFERENCES ProjectDocuments (Id) CONSTRAINT fk_ProjectDocumentRisk_ProjectDocument
);