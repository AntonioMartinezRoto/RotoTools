<#
    .SINOPSIS
    Automatiza el proceso manual que se hacía tras cada "dotnet publish" de RotoTools.Suite:
    1) Comprime el ejecutable publicado (ya renombrado a RotoTools.exe por el target
       "RenombrarEjecutablePublicado" del .csproj) en un .zip "RotoTools_v<version>.zip",
       en la misma carpeta de publicación.
    2) Archiva en la subcarpeta "old" cualquier .zip que ya hubiera en la carpeta de red destino.
    3) Copia el nuevo .zip a la carpeta de red destino.

    Nota sobre el orden: el proceso manual original archivaba el .zip antiguo DESPUÉS de copiar el
    nuevo ("cualquier .zip que haya en la ruta"), lo que movería también el recién copiado a "old"
    si se hiciera en ese orden de forma literal. Aquí se archiva primero lo que YA hubiera y se
    copia el nuevo después, para terminar siempre con el .zip nuevo en la carpeta principal y
    cualquier .zip anterior en "old", que es el resultado que se busca.

    Se invoca desde RotoTools.Suite.csproj (target "ComprimirYPublicarEnRed", después de publicar).
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDir,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    # Ruta UNC en vez de la unidad de red mapeada "N:": el proceso de publicación (MSBuild/Visual
    # Studio, a veces ejecutado con permisos elevados) puede no ver unidades de red mapeadas en la
    # sesión de usuario normal, aunque el propio usuario sí tenga acceso a ellas desde el Explorador
    # (confirmado en producción: Test-Path fallaba sobre "N:\..." pese a tener acceso real a la
    # ruta). La ruta UNC no depende de ninguna letra de unidad ni de la sesión, así que funciona
    # igual esté o no mapeada "N:" y se ejecute o no el proceso elevado.
    [string]$CarpetaDestino = "\\rfspsyn1.rfle.roto-frank.com\Dept\DATASOFT\Apps\RotoTools"
)

$ErrorActionPreference = "Stop"

# La carpeta de publicación suele estar dentro de OneDrive (ver -PublishDir): justo después de que
# MSBuild escribe/renombra RotoTools.exe, OneDrive puede quedarse un instante con el fichero
# bloqueado mientras empieza a sincronizarlo, y la compresión (o la copia a red) falla con "El
# proceso no puede obtener acceso al archivo..." aunque nada más lo esté usando de verdad (visto en
# producción). Reintenta con espera creciente antes de rendirse, en vez de fallar a la primera.
function Invoke-ConReintentos {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Accion,
        [int]$Intentos = 6,
        [int]$EsperaInicialMs = 1000
    )

    $esperaMs = $EsperaInicialMs
    for ($intento = 1; $intento -le $Intentos; $intento++) {
        try {
            & $Accion
            return
        }
        catch {
            if ($intento -eq $Intentos) { throw }
            Write-Host "Intento $intento de $Intentos fallido ($($_.Exception.Message)); reintentando en ${esperaMs}ms..."
            Start-Sleep -Milliseconds $esperaMs
            $esperaMs = $esperaMs * 2
        }
    }
}

try {
    $publishDirFull = (Resolve-Path -LiteralPath $PublishDir).Path
    $exePath = Join-Path $publishDirFull "RotoTools.exe"

    if (-not (Test-Path -LiteralPath $exePath)) {
        throw "No se encuentra '$exePath'. ¿Se ha ejecutado antes el renombrado a RotoTools.exe?"
    }

    # 1) Comprimir RotoTools.exe en RotoTools_v<version>.zip, en la propia carpeta de publicación.
    $zipName = "RotoTools_v$Version.zip"
    $zipPath = Join-Path $publishDirFull $zipName

    if (Test-Path -LiteralPath $zipPath) {
        Remove-Item -LiteralPath $zipPath -Force
    }

    Invoke-ConReintentos { Compress-Archive -LiteralPath $exePath -DestinationPath $zipPath }
    Write-Host "Comprimido: $zipPath"

    # 2) Archivar en "old" cualquier .zip que YA hubiera en la carpeta de red destino (antes de
    #    copiar el nuevo, para no archivar también el que se acaba de copiar).
    if (-not (Test-Path -LiteralPath $CarpetaDestino)) {
        throw "No se encuentra la carpeta de destino '$CarpetaDestino' (¿red/VPN conectada y con acceso al recurso compartido?)."
    }

    $carpetaOld = Join-Path $CarpetaDestino "old"
    if (-not (Test-Path -LiteralPath $carpetaOld)) {
        New-Item -ItemType Directory -Path $carpetaOld | Out-Null
    }

    $zipsExistentes = Get-ChildItem -LiteralPath $CarpetaDestino -Filter "*.zip" -File -ErrorAction SilentlyContinue
    foreach ($zipExistente in $zipsExistentes) {
        Invoke-ConReintentos { Move-Item -LiteralPath $zipExistente.FullName -Destination $carpetaOld -Force }
        Write-Host "Archivado en 'old': $($zipExistente.Name)"
    }

    # 3) Copiar el nuevo .zip a la carpeta de red destino.
    Invoke-ConReintentos { Copy-Item -LiteralPath $zipPath -Destination $CarpetaDestino -Force }
    Write-Host "Copiado a destino: $(Join-Path $CarpetaDestino $zipName)"
}
catch {
    # Write-Host en vez de Write-Error: al ejecutarse desde un Exec de MSBuild (-File, sin consola
    # interactiva), Write-Error añadía en el log de compilación una línea adicional con caracteres
    # extraños (problema de codificación al capturar el flujo de error, visto en producción). El
    # "exit 1" es lo que realmente hace fallar el Exec/la publicación; el mensaje en sí basta con
    # que llegue por la salida estándar.
    $mensaje = $_.Exception.Message
    if ($mensaje -match "siendo utilizado en otro proceso|being used by another process") {
        $mensaje += " (si persiste tras los reintentos, comprueba que RotoTools.exe no esté abierto/en ejecución desde esa misma carpeta de publicación)."
    }
    Write-Host "ERROR en PostPublicar.ps1: $mensaje"
    exit 1
}
