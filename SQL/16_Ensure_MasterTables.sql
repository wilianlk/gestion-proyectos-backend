-- Script idempotente para Informix:
-- Crea tablas maestras solo si no existen y carga datos base si estan vacias.

CREATE PROCEDURE ensure_master_tables()
    DEFINE solution_count INTEGER;
    DEFINE deployment_count INTEGER;
    DEFINE solution_rows INTEGER;
    DEFINE deployment_rows INTEGER;

    SELECT COUNT(*)
      INTO solution_count
      FROM systables
     WHERE LOWER(tabname) = 'solutiontypes'
       AND tabtype = 'T';

    IF solution_count = 0 THEN
        EXECUTE IMMEDIATE
            'CREATE TABLE SolutionTypes (
                Id SERIAL PRIMARY KEY NOT NULL,
                Name LVARCHAR(255) NOT NULL,
                CreatedAt DATETIME YEAR TO SECOND NOT NULL,
                UpdatedAt DATETIME YEAR TO SECOND NULL,
                IsActive BOOLEAN NOT NULL
            )';
    END IF;

    SELECT COUNT(*)
      INTO deployment_count
      FROM systables
     WHERE LOWER(tabname) = 'deploymentmodels'
       AND tabtype = 'T';

    IF deployment_count = 0 THEN
        EXECUTE IMMEDIATE
            'CREATE TABLE DeploymentModels (
                Id SERIAL PRIMARY KEY NOT NULL,
                Name LVARCHAR(255) NOT NULL,
                CreatedAt DATETIME YEAR TO SECOND NOT NULL,
                UpdatedAt DATETIME YEAR TO SECOND NULL,
                IsActive BOOLEAN NOT NULL
            )';
    END IF;

    SELECT COUNT(*)
      INTO solution_rows
      FROM SolutionTypes;

    IF solution_rows = 0 THEN
        INSERT INTO SolutionTypes (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('Desarrollo a la medida', CURRENT, NULL, 't');
        INSERT INTO SolutionTypes (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('Low-code', CURRENT, NULL, 't');
        INSERT INTO SolutionTypes (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('SaaS', CURRENT, NULL, 't');
        INSERT INTO SolutionTypes (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('Hibrido', CURRENT, NULL, 't');
        INSERT INTO SolutionTypes (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('App movil', CURRENT, NULL, 't');
        INSERT INTO SolutionTypes (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('Portal Web', CURRENT, NULL, 't');
        INSERT INTO SolutionTypes (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('IoT / Hardware + Software', CURRENT, NULL, 't');
    END IF;

    SELECT COUNT(*)
      INTO deployment_rows
      FROM DeploymentModels;

    IF deployment_rows = 0 THEN
        INSERT INTO DeploymentModels (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('On-premise', CURRENT, NULL, 't');
        INSERT INTO DeploymentModels (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('Nube', CURRENT, NULL, 't');
        INSERT INTO DeploymentModels (Name, CreatedAt, UpdatedAt, IsActive) VALUES ('Hibrido', CURRENT, NULL, 't');
    END IF;
END PROCEDURE;

EXECUTE PROCEDURE ensure_master_tables();
DROP PROCEDURE ensure_master_tables;
