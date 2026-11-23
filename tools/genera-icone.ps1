<#
  Genera l'icona di Depositum in tutte le misure richieste dal pacchetto MSIX,
  il file .ico della finestra e le schermate di avvio, in src/Depositum/Assets.

  L'icona: una "D" chiara su un quadrato rosso arrotondato (i colori del tema Rosso),
  attraversata da una linea che sale. È disegnata in uno spazio 100x100 e
  ricampionata da una versione 8 volte più grande, per bordi puliti anche a 16 pixel.

  Uso, dalla cartella principale del repository:
      powershell -ExecutionPolicy Bypass -File tools\genera-icone.ps1
#>
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$assets = Join-Path $PSScriptRoot '..\src\Depositum\Assets' | Resolve-Path
$fondo  = [System.Drawing.ColorTranslator]::FromHtml('#81383C')
$lettera = [System.Drawing.ColorTranslator]::FromHtml('#F1DFDF')
$linea  = [System.Drawing.ColorTranslator]::FromHtml('#1F1A1A')

function Nuova-Grafica($bmp) {
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.InterpolationMode = 'HighQualityBicubic'
    $g.PixelOffsetMode = 'HighQuality'
    $g.CompositingQuality = 'HighQuality'
    $g
}

# L'icona quadrata, di lato $lato pixel, su sfondo trasparente.
function Disegna-Icona([int]$lato) {
    $s = 8 * $lato
    $grande = New-Object System.Drawing.Bitmap $s, $s
    $g = Nuova-Grafica $grande
    $g.ScaleTransform($s / 100, $s / 100)

    # Quadrato arrotondato (raggio 22)
    $r = 22; $d = 2 * $r
    $q = New-Object System.Drawing.Drawing2D.GraphicsPath
    $q.AddArc(0, 0, $d, $d, 180, 90); $q.AddArc(100 - $d, 0, $d, $d, 270, 90)
    $q.AddArc(100 - $d, 100 - $d, $d, $d, 0, 90); $q.AddArc(0, 100 - $d, $d, $d, 90, 90)
    $q.CloseFigure()
    $g.FillPath((New-Object System.Drawing.SolidBrush $fondo), $q)

    # La D: contorno esterno (asta x=26, curva di raggio 28) meno l'interno (asta x=38, raggio 17)
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath ([System.Drawing.Drawing2D.FillMode]::Alternate)
    $p.AddLine(26, 22, 48, 22); $p.AddArc(20, 22, 56, 56, -90, 180); $p.AddLine(48, 78, 26, 78); $p.CloseFigure()
    $p.AddLine(38, 33, 48, 33); $p.AddArc(31, 33, 34, 34, -90, 180); $p.AddLine(48, 67, 38, 67); $p.CloseFigure()
    $g.FillPath((New-Object System.Drawing.SolidBrush $lettera), $p)

    # La linea che sale: prima un bordo del colore di fondo che "taglia" la D, poi la linea scura
    $punti = [System.Drawing.PointF[]]@(
        (New-Object System.Drawing.PointF 16, 76), (New-Object System.Drawing.PointF 40, 58),
        (New-Object System.Drawing.PointF 54, 64), (New-Object System.Drawing.PointF 80, 32))
    foreach ($tratto in @(@($fondo, 12), @($linea, 5))) {
        $penna = New-Object System.Drawing.Pen $tratto[0], $tratto[1]
        $penna.StartCap = 'Round'; $penna.EndCap = 'Round'; $penna.LineJoin = 'Round'
        $g.DrawLines($penna, $punti)
    }
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $linea), 73.5, 25.5, 13, 13)
    $g.Dispose()

    $bmp = New-Object System.Drawing.Bitmap $lato, $lato
    $g = Nuova-Grafica $bmp
    $g.DrawImage($grande, 0, 0, $lato, $lato)
    $g.Dispose(); $grande.Dispose()
    $bmp
}

# Un'immagine $larghezza x $altezza trasparente con l'icona al centro, alta $quota dell'altezza.
function Disegna-Centrata([int]$larghezza, [int]$altezza, [double]$quota) {
    $lato = [int][Math]::Round($altezza * $quota)
    $icona = Disegna-Icona $lato
    $bmp = New-Object System.Drawing.Bitmap $larghezza, $altezza
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.DrawImage($icona, [int](($larghezza - $lato) / 2), [int](($altezza - $lato) / 2), $lato, $lato)
    $g.Dispose(); $icona.Dispose()
    $bmp
}

function Salva($bmp, $nome) {
    $bmp.Save((Join-Path $assets $nome), [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $nome
}

# Le immagini segnaposto del modello di progetto, sostituite da quelle qui sotto
Get-ChildItem $assets -Filter *.png | Remove-Item

# Icona dell'app (menu Start, barra delle applicazioni, impostazioni)
foreach ($k in @{100 = 44; 125 = 55; 150 = 66; 200 = 88; 400 = 176}.GetEnumerator()) {
    Salva (Disegna-Icona $k.Value) "Square44x44Logo.scale-$($k.Key).png"
}
foreach ($t in 16, 20, 24, 30, 32, 36, 40, 48, 60, 64, 72, 80, 96, 256) {
    Salva (Disegna-Icona $t) "Square44x44Logo.targetsize-$t.png"
    Salva (Disegna-Icona $t) "Square44x44Logo.targetsize-${t}_altform-unplated.png"
}
# Riquadri del menu Start: icona al centro su fondo trasparente
foreach ($k in @{100 = 150; 125 = 188; 150 = 225; 200 = 300; 400 = 600}.GetEnumerator()) {
    Salva (Disegna-Centrata $k.Value $k.Value 0.6) "Square150x150Logo.scale-$($k.Key).png"
}
foreach ($k in @{100 = 1; 200 = 2}.GetEnumerator()) {
    Salva (Disegna-Centrata (310 * $k.Value) (150 * $k.Value) 0.6) "Wide310x150Logo.scale-$($k.Key).png"
}
# Logo mostrato dall'installer
foreach ($k in @{100 = 50; 200 = 100; 400 = 200}.GetEnumerator()) {
    Salva (Disegna-Icona $k.Value) "StoreLogo.scale-$($k.Key).png"
}
Salva (Disegna-Icona 24) 'LockScreenLogo.scale-100.png'
Salva (Disegna-Icona 48) 'LockScreenLogo.scale-200.png'
# Schermata di avvio
Salva (Disegna-Centrata 620 300 0.4) 'SplashScreen.scale-100.png'
Salva (Disegna-Centrata 1240 600 0.4) 'SplashScreen.scale-200.png'

# File .ico della finestra e dell'eseguibile: più misure, ognuna salvata come PNG
$misure = 16, 20, 24, 32, 40, 48, 64, 256
$immagini = foreach ($m in $misure) {
    $bmp = Disegna-Icona $m; $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
    , $ms.ToArray()
}
$out = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter $out
$w.Write([uint16]0); $w.Write([uint16]1); $w.Write([uint16]$misure.Count)
$offset = 6 + 16 * $misure.Count
for ($i = 0; $i -lt $misure.Count; $i++) {
    $m = $misure[$i] % 256   # 0 significa 256
    $w.Write([byte]$m); $w.Write([byte]$m); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([uint16]1); $w.Write([uint16]32)
    $w.Write([uint32]$immagini[$i].Length); $w.Write([uint32]$offset)
    $offset += $immagini[$i].Length
}
foreach ($dati in $immagini) { $w.Write($dati) }
[System.IO.File]::WriteAllBytes((Join-Path $assets 'Depositum.ico'), $out.ToArray())
'Depositum.ico'
