<#
  Publicación de una versión (F6, D-64). Desde la raíz del repositorio:

    powershell -ExecutionPolicy Bypass -File escritorio/publicar.ps1 -Version 1.0.0

  Deja en dist/ (no se versiona):
    - BaseLunar-escritorio-v<versión>-win-x64.zip: el juego para Windows, sin instalar y con .NET incluido.
    - BaseLunar-v<versión>-repositorio.zip: el repositorio en el tag v<versión>. La web se abre con doble clic
      en index.html, sin servidor.
    - SHA256SUMS.txt: la huella de cada ZIP.
#>
param([string]$Version = '1.0.0')

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
Set-Location $raiz
$dist = Join-Path $raiz 'dist'
$salida = Join-Path $dist 'publicacion'
$carpeta = Join-Path $dist 'BaseLunar'
foreach ($vieja in @($salida, $carpeta)) { if (Test-Path $vieja) { Remove-Item $vieja -Recurse -Force } }

# 1. Un solo .exe con .NET dentro. Las DLL nativas de WPF van al lado: así el juego no escribe nada en %TEMP%.
dotnet publish escritorio/src/BaseLunar.App -c Release -r win-x64 --self-contained true -m:1 -nologo -o $salida `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=false -p:EnableCompressionInSingleFile=true `
  -p:DebugType=none -p:Version=$Version
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish falló' }

New-Item -ItemType Directory -Force (Join-Path $carpeta 'licencias') | Out-Null
Copy-Item (Join-Path $salida 'BaseLunar.App.exe') (Join-Path $carpeta 'BaseLunar.exe')
Copy-Item (Join-Path $salida '*.dll') $carpeta
Copy-Item 'escritorio/LEEME.txt' $carpeta

# 2. Licencias: tipografía, íconos y .NET (el runtime viaja dentro del .exe).
$paquetes = ((dotnet nuget locals global-packages --list) -replace '^global-packages:\s*', '').Trim()
function Ultima([string]$paquete) {
  Get-ChildItem (Join-Path $paquetes $paquete) -Directory | Sort-Object { [version]($_.Name -replace '-.*$', '') } | Select-Object -Last 1
}
$net = (Ultima 'microsoft.netcore.app.runtime.win-x64').FullName
$escritorio = (Ultima 'microsoft.windowsdesktop.app.runtime.win-x64').FullName
$licencias = Join-Path $carpeta 'licencias'
Copy-Item 'escritorio/src/BaseLunar.App/Temas/Fuentes/OFL.txt' (Join-Path $licencias 'Atkinson-Hyperlegible-OFL.txt')
Copy-Item 'escritorio/src/BaseLunar.App/Temas/Lucide-LICENSE.txt' (Join-Path $licencias 'Lucide-LICENSE.txt')
Copy-Item (Join-Path $net 'LICENSE.TXT') (Join-Path $licencias 'dotnet-LICENSE.txt')
Copy-Item (Join-Path $net 'THIRD-PARTY-NOTICES.TXT') (Join-Path $licencias 'dotnet-THIRD-PARTY-NOTICES.txt')
Copy-Item (Join-Path $escritorio 'LICENSE') (Join-Path $licencias 'windowsdesktop-LICENSE.txt')
Copy-Item (Join-Path $paquetes 'system.speech/10.0.0/THIRD-PARTY-NOTICES.TXT') (Join-Path $licencias 'System.Speech-THIRD-PARTY-NOTICES.txt')

# 3. Los ZIP y sus huellas.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zipEscritorio = Join-Path $dist "BaseLunar-escritorio-v$Version-win-x64.zip"
if (Test-Path $zipEscritorio) { Remove-Item $zipEscritorio }
[IO.Compression.ZipFile]::CreateFromDirectory($carpeta, $zipEscritorio, [IO.Compression.CompressionLevel]::Optimal, $true)

$zipRepositorio = Join-Path $dist "BaseLunar-v$Version-repositorio.zip"
git rev-parse -q --verify "refs/tags/v$Version" | Out-Null
$origen = if ($LASTEXITCODE -eq 0) { "v$Version" } else { Write-Warning "No existe el tag v${Version}: se empaqueta HEAD."; 'HEAD' }
git archive --format=zip --prefix="BaseLunar-v$Version/" -o $zipRepositorio $origen
if ($LASTEXITCODE -ne 0) { throw 'git archive falló' }

$huellas = foreach ($zip in @($zipEscritorio, $zipRepositorio)) {
  '{0}  {1}' -f (Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant(), (Split-Path $zip -Leaf)
}
$huellas | Set-Content (Join-Path $dist 'SHA256SUMS.txt') -Encoding ascii
Remove-Item $salida -Recurse -Force
Get-ChildItem $dist -File | Format-Table Name, @{ n = 'MB'; e = { [math]::Round($_.Length / 1MB, 1) } } -AutoSize
