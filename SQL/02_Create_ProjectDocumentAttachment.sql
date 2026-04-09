-- SQL para crear la tabla ProjectDocumentAttachments en Informix
-- Esta tabla almacena los archivos adjuntos relacionados a los documentos de proyecto.

CREATE TABLE ProjectDocumentAttachments (
    Id SERIAL PRIMARY KEY,
    ProjectDocumentId INTEGER NOT NULL,
    Section VARCHAR(255) NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath TEXT NOT NULL,
    FileSize BIGINT,
    ContentType VARCHAR(255),
    CreatedAt DATETIME YEAR TO SECOND NOT NULL,
    UpdatedAt DATETIME YEAR TO SECOND,
    IsActive BOOLEAN NOT NULL,
    FOREIGN KEY (ProjectDocumentId) REFERENCES ProjectDocuments (Id) CONSTRAINT fk_ProjectDocumentAttachment_ProjectDocument
);
