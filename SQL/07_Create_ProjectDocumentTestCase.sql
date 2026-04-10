-- SQL para crear la tabla ProjectDocumentTestCases en Informix
-- Esta tabla almacena los casos de prueba asociados a un documento de proyecto.

CREATE TABLE ProjectDocumentTestCases (
    Id SERIAL PRIMARY KEY,
    ProjectDocumentId INTEGER NOT NULL,
    TestStrategy TEXT,
    AcceptanceCriteria TEXT,
    DeployProductionCriteria TEXT,
    Identification VARCHAR(255) NOT NULL,
    Username VARCHAR(255) NOT NULL,
    CreatedAt DATETIME YEAR TO SECOND NOT NULL,
    UpdatedAt DATETIME YEAR TO SECOND,
    IsActive BOOLEAN NOT NULL,
    FOREIGN KEY (ProjectDocumentId) REFERENCES ProjectDocuments (Id) CONSTRAINT fk_ProjectDocumentTestCase_ProjectDocument
);