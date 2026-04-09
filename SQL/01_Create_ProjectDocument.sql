-- SQL para crear la tabla ProjectDocument en Informix
-- Esta tabla almacena los documentos de proyecto y sus atributos principales.

CREATE TABLE ProjectDocuments (
    Id SERIAL PRIMARY KEY,
    ProjectCode VARCHAR(100) NOT NULL,
    ProjectName VARCHAR(255) NOT NULL,
    Sponsor VARCHAR(255),
    FunctionalLead VARCHAR(255),
    TechnicalLead VARCHAR(255),
    DocumentStatus VARCHAR(100) NOT NULL,
    ProjectVision TEXT,
    GeneralObjective TEXT,
    SpecificObjectives TEXT,
    ExpectedValue TEXT,
    Scope TEXT,
    Exclusions TEXT,
    SolutionDescription TEXT,
    SolutionType VARCHAR(255),
    DeploymentModel VARCHAR(255),
    SoftwareStack VARCHAR(255),
    HardwareArchitecture VARCHAR(255),
    SecurityControl VARCHAR(255),
    ExpectedConcurrentUsers INTEGER,
    SlaResponseTime VARCHAR(255),
    Identification VARCHAR(255) NOT NULL,
    Username VARCHAR(255) NOT NULL,
    CreatedAt DATETIME YEAR TO SECOND NOT NULL,
    UpdatedAt DATETIME YEAR TO SECOND,
    IsActive BOOLEAN NOT NULL
);
