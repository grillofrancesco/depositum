# Come pubblicare una release

Procedura per produrre il pacchetto MSIX firmato e pubblicarlo su GitHub.
Tutti i comandi vanno lanciati in PowerShell dalla cartella principale del repository.

Le cartelle `firma/` e `rilascio/` sono escluse da git: la prima contiene la chiave privata,
la seconda i pacchetti generati.

## Una volta sola: il certificato di firma

Il soggetto del certificato deve coincidere con il `Publisher` di `src/Depositum/Package.appxmanifest`
(`CN=grillofrancesco`), altrimenti il pacchetto non si firma.

```powershell
New-Item -ItemType Directory -Force firma | Out-Null
$cert = New-SelfSignedCertificate -Type Custom -Subject "CN=grillofrancesco" `
    -KeyUsage DigitalSignature -FriendlyName "Depositum - firma pacchetti" `
    -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(3) `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
Export-Certificate -Cert $cert -FilePath firma\Depositum.cer
Export-PfxCertificate -Cert $cert -FilePath firma\Depositum.pfx -Password (Read-Host -AsSecureString "Password per il .pfx")
$cert.Thumbprint
```

- `Depositum.cer` è la parte pubblica: va allegata a ogni release.
- `Depositum.pfx` contiene la chiave privata: è il backup del certificato, da tenere fuori dal repository
  (per esempio in un gestore di password). Senza, alla scadenza o su un altro PC serve un certificato
  nuovo e chi ha installato l'app deve fidarsi di nuovo.
- Annota l'impronta (`Thumbprint`) stampata alla fine: serve per firmare.

Per installare l'app firmata anche sul tuo PC, rendi attendibile il certificato, una volta per PC.
Apri PowerShell **come amministratore** (parte in `C:\Windows\system32`, quindi prima spostati
nella cartella del repository):

```powershell
cd "D:\Archivio\Documenti\Informatica\moneybank\codebase"
Import-Certificate -FilePath firma\Depositum.cer -CertStoreLocation Cert:\LocalMachine\TrustedPeople
```

## A ogni release

### 1. Versione

Aggiorna `Version` in `src/Depositum/Package.appxmanifest` (per esempio `1.1.0.0`: il quarto numero
resta 0), fai il commit e invialo.

### 2. Pacchetto firmato

```powershell
$impronta = (Get-ChildItem Cert:\CurrentUser\My | Where-Object Subject -eq "CN=grillofrancesco" | Sort-Object NotAfter | Select-Object -Last 1).Thumbprint
dotnet publish src/Depositum/Depositum.csproj -c Release -p:Platform=x64 -p:RuntimeIdentifier=win-x64 `
    -p:GenerateAppxPackageOnBuild=true -p:AppxBundle=Never `
    -p:AppxPackageSigningEnabled=true -p:PackageCertificateThumbprint=$impronta `
    -p:AppxPackageDir="$PWD\rilascio\"
```

Il pacchetto è in `rilascio\Depositum_<versione>_x64_Test\Depositum_<versione>_x64.msix`
(il suffisso `_Test` è solo il nome della cartella).

Note sulla build Release:
- .NET è incluso nel pacchetto e ridotto con il trimming parziale (`TrimMode=partial` nel `.csproj`):
  il trimming completo fa andare l'app in crash all'avvio.
- I componenti di intelligenza artificiale di Windows App SDK sono esclusi nel `.csproj`.

### 3. Prova

Installa il pacchetto con un doppio clic, aprilo, registra una rilevazione, chiudi e riapri.

### 4. Tag

```powershell
git tag -a v1.0.0 -m "Depositum 1.0.0"
git push origin v1.0.0
```

### 5. Release su GitHub

1. Sul repository: **Releases** → **Draft a new release**.
2. **Choose a tag**: `v1.0.0`. Titolo: `Depositum 1.0.0`.
3. Incolla le note di rilascio.
4. Allega `Depositum_<versione>_x64.msix` e `firma\Depositum.cer`.
5. **Publish release**.
