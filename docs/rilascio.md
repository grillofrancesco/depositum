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

La versione pubblica ha due numeri (`1.0`, `1.1`, `2.0`). Il formato MSIX ne vuole quattro: in
`src/Depositum/Package.appxmanifest` scrivi `Version` come `1.1.0.0`, con gli ultimi due a 0.
Fai il commit e invialo.

Il nome del pacchetto (`Identity Name`, ora `grillofrancesco.Depositum`) non va più cambiato:
Windows lo usa per riconoscere gli aggiornamenti e la cartella dei dati dell'app. Il vecchio nome
`532f8103-…` del modello di Visual Studio era stato abbandonato perché su questo PC Windows ne aveva
conservato uno stato rovinato e l'app non si avviava più.

### 2. Pacchetto firmato

```powershell
$impronta = (Get-ChildItem Cert:\CurrentUser\My | Where-Object Subject -eq "CN=grillofrancesco" | Sort-Object NotAfter | Select-Object -Last 1).Thumbprint
dotnet publish src/Depositum/Depositum.csproj -c Release -p:Platform=x64 -p:RuntimeIdentifier=win-x64 `
    -p:GenerateAppxPackageOnBuild=true -p:AppxBundle=Never `
    -p:AppxPackageSigningEnabled=true -p:PackageCertificateThumbprint=$impronta `
    -p:AppxPackageDir="$PWD\rilascio\"
```

Il pacchetto è in `rilascio\Depositum_1.1.0.0_x64_Test\Depositum_1.1.0.0_x64.msix`
(il suffisso `_Test` è solo il nome della cartella). Copialo con il nome della versione pubblica,
che è quello da allegare alla release (la firma non dipende dal nome del file):

```powershell
Copy-Item rilascio\Depositum_1.1.0.0_x64_Test\Depositum_1.1.0.0_x64.msix rilascio\Depositum_1.1_x64.msix
```

Note sulla build Release:
- .NET è incluso nel pacchetto e ridotto con il trimming parziale (`TrimMode=partial` nel `.csproj`):
  il trimming completo fa andare l'app in crash all'avvio.
- I componenti di intelligenza artificiale di Windows App SDK sono esclusi nel `.csproj`.

### 3. Prova

Installa il pacchetto con un doppio clic, aprilo, registra una rilevazione, chiudi e riapri.
Per reinstallare la stessa versione usa **Reinstalla**: non disinstallare prima, perché
disinstallando si cancellano i dati dell'app.

### 4. Tag

```powershell
git tag -a v1.1 -m "Depositum 1.1"
git push origin v1.1
```

### 5. Release su GitHub

1. Sul repository: **Releases** → **Draft a new release**.
2. **Choose a tag**: `v1.1`. Titolo: `Depositum 1.1`.
3. Incolla le note di rilascio.
4. Allega `rilascio\Depositum_1.1_x64.msix` e `firma\Depositum.cer`.
5. **Publish release**.
