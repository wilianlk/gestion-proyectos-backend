pipeline {
    agent any

    options {
        disableConcurrentBuilds()
        timestamps()
        buildDiscarder(logRotator(numToKeepStr: '20', artifactNumToKeepStr: '20'))
    }

    triggers {
        githubPush()
    }

    parameters {
        string(name: 'JIRA_ISSUE_KEY', defaultValue: '', description: 'Clave del issue en Jira (ej: PROJ-123). Dejar vacio para omitir.')
    }

    environment {
        PROJECT     = "ProjectManagementApi.sln"
        API_CSPROJ  = "ProjectManagementApi\\ProjectManagementApi.csproj"
        ARTIFACT    = "GestionProyectos_${BUILD_NUMBER}.zip"
        PUBLISH_DIR = "publish"
    }

    stages {
        stage('Clonar repositorio') {
            steps { checkout scm }
        }

        stage('Restaurar dependencias') {
            steps { bat "dotnet restore ${env.PROJECT}" }
        }

        stage('Compilar') {
            steps { bat "dotnet build ${env.PROJECT} --configuration Release --no-restore" }
        }

        stage('Publicar') {
            steps {
                bat "if exist ${env.PUBLISH_DIR} rmdir /s /q ${env.PUBLISH_DIR}"
                bat "dotnet publish ${env.API_CSPROJ} -c Release -o ${env.PUBLISH_DIR} --no-build"
                bat "powershell -NoProfile -Command \"Compress-Archive -Path ${env.PUBLISH_DIR}\\* -DestinationPath ${env.ARTIFACT} -Force\""
            }
        }

        stage('Archivar artefactos') {
            steps { archiveArtifacts artifacts: "${env.ARTIFACT}", fingerprint: true }
        }

        stage('Desplegar en KSCSERVER') {
            steps {
                powershell '''
                    $ErrorActionPreference = "Stop"
                    $appCmd      = Join-Path $env:windir "System32\\inetsrv\\appcmd.exe"
                    $dest        = "C:\\Users\\admcliente\\Documents\\GestionProyectos"
                    $zip         = Join-Path $env:WORKSPACE $env:ARTIFACT
                    $backupRoot  = "C:\\Users\\admcliente\\Documents\\Publicacion"
                    $stamp       = Get-Date -Format "yyyyMMdd_HHmmss"
                    $currentBackup = Join-Path $backupRoot ("GestionProyectos_" + $stamp)
                    $tempConfig  = Join-Path $env:TEMP ("gestionproyectos_cfg_" + $stamp)
                    $configFiles = @("appsettings.json", "appsettings.Development.json")

                    & $appCmd stop apppool /apppool.name:"GestionProyectos" | Out-Null
                    if (!(Test-Path $dest)) { New-Item -Path $dest -ItemType Directory -Force | Out-Null }
                    if (!(Test-Path $backupRoot)) { New-Item -Path $backupRoot -ItemType Directory -Force | Out-Null }
                    if (Test-Path $dest) {
                        New-Item -Path $tempConfig -ItemType Directory -Force | Out-Null
                        foreach ($cfg in $configFiles) {
                            $cfgPath = Join-Path $dest $cfg
                            if (Test-Path $cfgPath) { Copy-Item $cfgPath (Join-Path $tempConfig $cfg) -Force }
                        }
                        # Backup completo de seguridad (no se usa para restaurar automaticamente,
                        # solo queda disponible por si hay que recuperar algo manualmente).
                        # Se excluye uploads: son adjuntos de usuario que no cambian con el deploy,
                        # no tiene sentido duplicarlos en cada build.
                        robocopy $dest $currentBackup /E /XD uploads /R:1 /W:1 | Out-Null
                    }
                    # Extraer ENCIMA sin borrar la carpeta primero (evita perder wwwroot, lo
                    # despliega el job de frontend aparte, y otro contenido no versionado).
                    Expand-Archive -Path $zip -DestinationPath $dest -Force
                    if (Test-Path $tempConfig) {
                        foreach ($cfg in $configFiles) {
                            $cfgPath = Join-Path $tempConfig $cfg
                            if (Test-Path $cfgPath) { Copy-Item $cfgPath (Join-Path $dest $cfg) -Force }
                        }
                        Remove-Item $tempConfig -Recurse -Force -ErrorAction SilentlyContinue
                    }
                    & $appCmd start apppool /apppool.name:"GestionProyectos" | Out-Null
                '''
            }
        }
    }

    post {
        always {
            // El zip ya quedo preservado por archiveArtifacts; esta copia en el
            // workspace solo se usaba para el despliegue y no debe quedar.
            bat "if exist ${env.ARTIFACT} del /f /q ${env.ARTIFACT}"
        }
        success {
            echo 'Build y despliegue completados con exito.'
            emailext(
                from: 'anticipos@rocket.recamier.com',
                to: 'wlucumi@recamier.com',
                subject: "Despliegue exitoso: ${env.JOB_NAME} (Build #${env.BUILD_NUMBER})",
                mimeType: 'text/html',
                body: """
                    <h2 style='color:#28a745;'>Despliegue exitoso</h2>
                    <p><b>Proyecto:</b> ${env.JOB_NAME}</p>
                    <p><b>Servidor:</b> KSCSERVER</p>
                    <p><b>Build:</b> #${env.BUILD_NUMBER}</p>
                    <p><b>Fecha:</b> ${new Date()}</p>
                    <p><b>URL:</b> <a href='${env.BUILD_URL}'>${env.BUILD_URL}</a></p>
                """
            )
            script {
                if (params.JIRA_ISSUE_KEY?.trim()) {
                    try {
                        jiraAddComment(
                            site: 'Recamier Jira',
                            idOrKey: params.JIRA_ISSUE_KEY,
                            comment: "Despliegue exitoso de ${env.JOB_NAME}.\nBuild: #${env.BUILD_NUMBER}\nFecha: ${new Date()}\nURL: ${env.BUILD_URL}"
                        )
                        jiraTransitionIssue(
                            site: 'Recamier Jira',
                            idOrKey: params.JIRA_ISSUE_KEY,
                            input: [transition: [id: '42']]
                        )
                    } catch (Exception e) {
                        echo "Notificacion a Jira con advertencias: ${e.message}"
                    }
                }
            }
        }
        failure {
            echo 'El pipeline fallo. Revisar logs de Jenkins.'
            emailext(
                from: 'anticipos@rocket.recamier.com',
                to: 'wlucumi@recamier.com',
                subject: "Fallo en despliegue: ${env.JOB_NAME} (Build #${env.BUILD_NUMBER})",
                mimeType: 'text/html',
                body: """
                    <h2 style='color:#dc3545;'>Error durante la publicacion</h2>
                    <p><b>Proyecto:</b> ${env.JOB_NAME}</p>
                    <p><b>Build:</b> #${env.BUILD_NUMBER}</p>
                    <p><b>Fecha:</b> ${new Date()}</p>
                    <p><b>URL:</b> <a href='${env.BUILD_URL}'>${env.BUILD_URL}</a></p>
                """
            )
            script {
                if (params.JIRA_ISSUE_KEY?.trim()) {
                    try {
                        jiraAddComment(
                            site: 'Recamier Jira',
                            idOrKey: params.JIRA_ISSUE_KEY,
                            comment: "Fallo en despliegue de ${env.JOB_NAME}.\nBuild: #${env.BUILD_NUMBER}\nFecha: ${new Date()}\nURL: ${env.BUILD_URL}"
                        )
                    } catch (Exception e) {
                        echo "No se pudo notificar el error en Jira: ${e.message}"
                    }
                }
            }
        }
    }
}
