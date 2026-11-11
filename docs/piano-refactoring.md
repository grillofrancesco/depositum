# Piano di refactoring — Depositum

Stato analizzato: branch `f/refactoring`, commit `46d24a7`, working tree pulito.
Build di riferimento (allora): `dotnet build Depositum.csproj -p:Platform=x64` → 0 errori, 0 avvisi.

Dalla fase 0 i comandi sono:

- `dotnet build src/Depositum/Depositum.csproj -p:Platform=x64`
- `dotnet test tests/Depositum.Tests`

Obiettivo: stesso comportamento, codice più leggibile e manutenibile, e una base
su cui costruire in seguito la gestione dei temi.

Regola di lavoro: ogni fase è un commit a sé, compila, e l'app si comporta come prima.
Oggi non ci sono test automatici: la fase 0 li introduce per la logica senza
interfaccia. Per tutto ciò che si vede a schermo resta il giro manuale descritto in fondo.

---

## 1. Cosa c'è oggi

| Area | File | Righe | Nota |
|---|---|---|---|
| Avvio | `App.xaml`, `App/App.xaml.cs`, `App/MainWindow.xaml(.cs)` | ~110 | `MainWindow` carica i dati e forza il tema scuro |
| Stili | `App/Styles.xaml` | 246 | colori, spaziature, stili di pulsanti/testi/input |
| Pagine | `Pages/DashboardPage`, `RegistraPage`, `AnalysisPage` | ~1.200 | tutta la logica è nel code-behind |
| Dati | `Services/DataService.cs`, `Models/*` | ~190 | classe statica, CSV in `%LocalAppData%\Depositum\history.csv` |
| Colori depositi | `Utilities/PaletteDepositi.cs` | 15 | 8 colori come stringhe hex |

Il progetto è piccolo e funziona. Il refactoring conviene, ma va tenuto proporzionato:
niente framework o livelli di astrazione che un'app di tre pagine non ripaga.

---

## 2. Problemi trovati

### 2.1 Colori (il punto che blocca i temi)

1. **Dizionari duplicati.** In `Styles.xaml` i dizionari `Default` e `Dark` sono
   identici riga per riga (34 chiavi × 2). Ogni modifica va fatta due volte.
2. **Tema scuro forzato.** `MainWindow.xaml` imposta `RequestedTheme="Dark"` sulla
   griglia radice e non esiste un dizionario `Light`.
3. **Nomi legati alla tinta, non al ruolo.** `IvoryBrush`, `LaurelBrush`,
   `LaurelLightBrush` descrivono il colore del tema verde. In un tema blu
   "Laurel" non ha senso. `LaurelBrush` inoltre non è usato da nessuna parte.
4. **Colori scritti fuori dal dizionario** (13 punti, nessuno reagisce a un cambio tema):

   | Dove | Colore | Ruolo reale |
   |---|---|---|
   | `DashboardPage.xaml` righe 111, 117, 132, 138 | `White` | testo/icona sui pulsanti grandi |
   | `DashboardPage.xaml.cs` righe 94, 104 | `Colors.White` | testo sulle card dei depositi |
   | `RegistraPage.xaml.cs` riga 162 | `Colors.Salmon` | messaggio d'errore nel dialogo |
   | `AnalysisPage.xaml.cs` righe 293, 296 | `SKColor(90,90,90)` | griglia e linea dello zero |
   | `AnalysisPage.xaml.cs` riga 324 | `SKColors.White` | etichette del grafico radiale |
   | `PaletteDepositi.cs` | 8 hex | colori dei depositi |

   In più le etichette degli assi dei grafici usano i colori predefiniti di
   LiveCharts, che non seguono il tema dell'app.
5. **Due convertitori diversi** per lo stesso lavoro: `HexToColor` in
   `DashboardPage` e `ToSkColor` in `AnalysisPage`.
6. **Chiavi di sistema e chiavi dell'app mescolate.** `SystemAccentColor` e le
   `TextControl*` sovrascrivono risorse di WinUI; le altre sono dell'app. Stanno
   nello stesso elenco senza distinzione.

### 2.2 Tipografia e misure

- `FontFamily="Segoe UI"` ripetuto circa 15 volte tra XAML e C#.
- `Constantia, Georgia, Times New Roman` ripetuto 3 volte, più `"Constantia"` nel grafico.
- Dimensioni del testo sparse come numeri (13, 15, 16, 18, 20, 22, 29, 30, 32).
- Il blocco "testo + icona" dei pulsanti grandi è ricopiato 6 volte con gli stessi attributi.

### 2.3 Logica duplicata

| Cosa | Ripetizioni | Dove |
|---|---|---|
| Formato data `dd/MM/yyyy` | 4 | `DataService`, `DashboardPage`, `RegistraPage` |
| Elenco dei formati data accettati | 2 | `DataService`, `RegistraPage` |
| Formato importo (`N0€` / `N0 €`) | 7 | dashboard senza spazio, grafici con spazio |
| Regole di validità (nome ≤ 30, importo 1…999.999.999) | 3 | tutte in `RegistraPage`, con numeri magici |
| "Torna indietro, altrimenti vai alla dashboard" | 3 | `RegistraPage` ×2, `AnalysisPage` |
| Apertura della `InfoBar` (4 righe ogni volta) | 6 | `RegistraPage` |
| Calcolo dei nomi di tutti i depositi | 2 | `AnalysisPage` |

### 2.4 Pagine troppo cariche

- `RegistraPage.BtnAggiungiDeposito_Click`: 120 righe che costruiscono un dialogo
  a mano, lo validano e ne applicano il risultato.
- `AnalysisPage`: 342 righe con stato, dialoghi delle opzioni, preparazione dei
  dati e costruzione dei tre grafici nello stesso file. I pulsanti passano il tipo
  di grafico come stringa nel `Tag`.
- `DashboardPage`: le card dei depositi sono create in C# e disposte a mano su
  una griglia a 2 colonne. Essendo fuori dallo XAML non possono usare `ThemeResource`.
- `DashboardPage` e `AnalysisPage` aggiornano l'interfaccia due volte a ogni
  apertura (`OnNavigatedTo` e poi `Loaded`).

### 2.5 Dati

- `DataService` mescola lettura del file, interpretazione delle righe e scrittura.
- Nessuna gestione degli errori di I/O: un file bloccato fa cadere l'app all'avvio.
- Il caricamento parte dal costruttore di `MainWindow`.

### 2.6 Struttura e residui

- Tutto è nel namespace `Depositum`, a prescindere dalla cartella.
- `App.xaml` è nella radice, `App.xaml.cs` in `App/`.
- `App.xaml.cs` ha 17 `using` inutilizzati (residuo del template).
- `Depositum.csproj` cita `records_test.csv`, che non esiste.
- `_MAX_AVG_MULTIPLIER` e `_MAX_HEIGHT_MULTIPLIER` hanno nome da costante ma sono campi modificabili.

---

## 3. Fasi

### Fase 0 — Rete di sicurezza: test prima di toccare il codice
Serve una libreria senza interfaccia, perché un progetto di test non può
referenziare comodamente l'app WinUI.

1. **Disposizione delle cartelle.** Oggi `Depositum.csproj` è nella radice e
   include automaticamente ogni `.cs` nelle sottocartelle, quindi nuovi progetti
   lì dentro verrebbero compilati due volte. Proposta:

   ```
   src/Depositum/         app WinUI (tutto il contenuto attuale, spostato con git mv)
   src/Depositum.Core/    libreria net8.0, nessuna dipendenza da WinUI
   tests/Depositum.Tests/ xUnit, referenzia solo Core
   Depositum.slnx         nella radice, con i tre progetti
   ```

   Alternativa meno invasiva: lasciare l'app nella radice e aggiungere al csproj
   l'esclusione delle due nuove cartelle. Funziona, ma è fragile.
2. **Spostare in Core, senza riscriverli**, `Rilevazione` e la parte di
   `DataService` che trasforma una riga di testo in `Rilevazione` e viceversa.
   `DataService` resta nell'app e chiama Core per l'interpretazione.
3. **Test di caratterizzazione**: fissano ciò che il codice fa *oggi*, anomalie
   comprese (ogni test di un'anomalia la cita per numero, vedi sezione 4).
   - lettura: riga normale, spazi, `;` finale, righe vuote, date nei tre formati,
     chiavi senza distinzione maiuscole/minuscole, campi non numerici ignorati;
   - scrittura: formato esatto della riga prodotta;
   - andata e ritorno: scrivi → rileggi → stessi dati.
4. Da qui in poi `dotnet test` si aggiunge al giro di verifica di ogni fase.

Sulla macchina sono presenti l'SDK 10.0.401 e il runtime .NET 8.0.31: i test
net8.0 girano senza installare altro.

### Fase 1 — Pulizia senza rischio
- Togliere `using` e `xmlns` inutilizzati, la voce `records_test.csv`, `LaurelBrush`.
- Trasformare i due moltiplicatori in `const`.
- Nessun cambiamento visibile.

### Fase 2 — Token di design (prerequisito dei temi)
1. Eliminare il doppione `Default`/`Dark` tenendo un solo dizionario.
2. Rinominare per ruolo: `IvoryBrush` → `TextPrimaryBrush`,
   `LaurelLightBrush` → `AccentTextBrush`. I nomi già per ruolo restano
   (`CanvasBrush`, `SurfaceBrush`, `AccentAction*`, `Destructive*`…).
3. Aggiungere i ruoli mancanti e usarli al posto dei 13 colori scritti a mano:
   `OnAccentBrush`, `ErrorTextBrush`, `ChartGridColor`, `ChartLabelColor`,
   `DepositoColor1…8`.
4. Risorse per i font (`BodyFontFamily`, `SerifFontFamily`) e stili di testo con
   nome (`ButtonLabelStyle`, `AmountStyle`, …) al posto degli attributi ripetuti.
5. Dividere `Styles.xaml` in `Theme/Colors.xaml`, `Theme/Typography.xaml`,
   `Theme/Controls.xaml`, separando le chiavi di sistema da quelle dell'app.

Risultato: i colori vivono in un solo file e ogni colore ha un nome che dice a cosa serve.

### Fase 3 — Palette e accesso ai colori dal codice
Il traguardo dichiarato sono **più palette predefinite tra cui l'utente sceglie**
(verde attuale, blu, …). I dizionari `Light`/`Dark` di WinUI non bastano: gestiscono
solo chiaro/scuro. Serve quindi un modello nostro.

- **`Palette`**: un oggetto con un nome e un colore per ogni ruolo della fase 2
  (sfondi, bordi, testi, accento, stati dei pulsanti, errore, griglia dei grafici,
  8 colori dei depositi). Il verde attuale diventa la prima e per ora unica
  palette, `Palette.Verde`, con gli stessi valori di oggi.
- **`Tema`** (in `Services/`), unico punto da cui XAML e C# ottengono i colori:
  - `Tema.Applica(palette)` scrive i colori della palette nei pennelli condivisi
    dell'app. Lo XAML continua a usare i pennelli per nome e si aggiorna da solo;
  - restituisce ogni ruolo sia come `Color` (WinUI) sia come `SKColor` (grafici);
  - espone `ColorePerDeposito(...)` e sostituisce `HexToColor`, `ToSkColor`,
    `PaletteDepositi`;
  - fornisce a LiveCharts i colori di assi, griglia ed etichette;
  - solleva `TemaCambiato`, a cui `AnalysisPage` si aggancia per ridisegnare i grafici.
- All'avvio `App` chiama `Tema.Applica(Palette.Verde)`: il risultato a schermo
  è identico a oggi.

**Da verificare con una prova prima di costruirci sopra:** che cambiare il
colore di un pennello condiviso aggiorni davvero tutti i controlli già a schermo,
compresi quelli che usano le chiavi di sistema sovrascritte (`TextControl*`) e
`SystemAccentColor`, che è un colore e non un pennello. Se quest'ultimo non si
aggiorna dal vivo, il cambio palette richiederà di ricaricare la pagina corrente.

Restano fuori dal refactoring, come funzionalità successive: la schermata di
scelta, il salvataggio della preferenza, le altre palette. Con questa base
aggiungere una palette vorrà dire scrivere un nuovo oggetto `Palette`, senza
toccare pagine né stili.

### Fase 4 — Logica condivisa fuori dalle pagine
In `Depositum.Core`, con i relativi test:
- `Formati`: formato data, formati accettati in lettura, formato importo.
  I due formati dell'importo oggi diversi restano diversi (anomalia 6).
- `RegoleDeposito`: lunghezza massima del nome, intervallo dell'importo, un solo
  metodo di validazione che restituisce il messaggio d'errore.

Nell'app:
- Un metodo comune per "torna indietro" e uno per mostrare la `InfoBar`.

### Fase 5 — Pagine più snelle
Resta il code-behind: niente MVVM né nuove dipendenze.
- **RegistraPage**: il dialogo "Aggiungi deposito" diventa un `ContentDialog`
  con il suo XAML (`Dialogs/AggiungiDepositoDialog`).
- **AnalysisPage**: tre parti distinte — dialoghi delle opzioni, preparazione
  dei dati (funzioni pure in Core, con test: finestra temporale, punti per
  deposito, limiti dell'asse), costruzione dei grafici.
  `Tag` stringa sostituito dall'enum.
- **DashboardPage**: card dei depositi in un `DataTemplate` con `ItemsRepeater`
  a 2 colonne, così i colori passano da `ThemeResource`.
  È il passo con più rischio di differenze visive: va confrontato con uno screenshot di prima.
- Togliere il doppio aggiornamento all'apertura delle pagine, dopo averlo verificato.

### Fase 6 — Dati
L'interpretazione delle righe è già in Core dalla fase 0. Resta:
- Spostare `CaricaDati()` da `MainWindow` ad `App.OnLaunched`.
- Gestire gli errori di I/O mostrando un messaggio invece di far cadere l'app
  (è l'unico punto del piano che aggiunge un comportamento: oggi l'app si chiude).

### Fase 7 — Struttura
- Namespace per cartella (`Depositum.Models`, `Depositum.Services`, …).
- `App.xaml` e `App.xaml.cs` nella stessa cartella.

Ordine: 0 → 1 → 2 → 3 → 4 → 5 → 6 → 7. La fase 0 viene prima di tutto; le fasi
2 e 3 sono quelle che servono ai temi; 4, 5 e 6 sono indipendenti tra loro.

### Dopo il refactoring
1. Correzione delle anomalie della sezione 4, una per commit, aggiornando il
   test di caratterizzazione corrispondente.
2. Attesa di una conferma dell'utente, poi funzionalità dei temi: schermata di scelta, preferenza salvata, nuove palette.

---

## 4. Comportamenti anomali trovati (non sono refactoring)

Correggerli cambia il comportamento, quindi restano fuori dal refactoring e
vanno trattati dopo (decisione del 10/11/2026). Alcuni toccano i dati.

1. **"Ultima rilevazione" ha due significati.** La dashboard mostra l'ultima
   riga *inserita*; istogramma e radiale mostrano quella con la *data più recente*.
   Registrando una rilevazione con data passata, le due schermate mostrano dati diversi.
2. **Nomi con `,` o `:` rovinano il CSV.** Il nome non viene controllato né
   protetto: un deposito chiamato `Conto: BNL` viene salvato ma alla rilettura
   si perde.
3. **Colore dello stesso deposito non stabile.** Dashboard, istogramma e radiale
   assegnano il colore in base alla posizione nell'ultima rilevazione; il grafico
   a linee in base alla posizione tra tutti i nomi dello storico. Se un deposito
   è stato tolto o aggiunto nel tempo, lo stesso deposito ha colori diversi.
4. **Righe illeggibili caricate comunque.** Una riga senza data valida diventa
   una rilevazione datata 01/01/0001 e finisce nei grafici.
5. **Totale oltre il limite.** Il totale è un `int`: tre depositi vicini al
   massimo consentito superano 2.147.483.647. La somma solleva un'eccezione non
   gestita e l'app si chiude al momento della conferma, senza salvare.
   (Corretto rispetto alla prima stesura, che parlava di totale salvato sbagliato.)
6. **Formato importo non uniforme.** `1.000€` nella dashboard, `1.000 €` nei grafici.
7. **Data scritta secondo la cultura di Windows.** La riga del CSV usa il
   separatore di data della lingua corrente: con l'italiano è `/`, con altre
   lingue può essere `.` o `-`. La rilettura funziona comunque, ma il file non
   ha un formato unico. (Emersa scrivendo i test della fase 0.)

---

## 5. Giro di verifica manuale (dopo ogni fase)

1. Avvio con `history.csv` esistente: data, card dei depositi e totale corretti.
2. Avvio senza file: stato vuoto, data nascosta, totale `0€`.
3. Registra: lista precompilata, data di oggi, modifica di un importo, aggiunta
   (anche con nome già esistente), rimozione con conferma, errori di validazione,
   Conferma e Annulla.
4. Analisi: i tre grafici, pulsante del grafico attivo disabilitato, opzioni di
   linee (intervallo, depositi visibili) e radiale (etichette), stato "nessun dato".
5. Confronto visivo con gli screenshot presi prima della fase.

---

## 6. Decisioni prese (10/11/2026)

| Tema | Decisione |
|---|---|
| Temi futuri | Più palette predefinite a scelta dell'utente. Non chiaro/scuro di sistema, non colore d'accento libero. |
| Architettura delle pagine | Riordino leggero: resta il code-behind, niente MVVM. |
| Anomalie (sezione 4) | Fuori dal refactoring, da correggere dopo. Restano elencate qui per non perderle. |
| Test automatici | Sì, subito: fase 0, prima di toccare dati e validazione. |

| Cartelle (fase 0) | App spostata in `src/Depositum/`, come da proposta del piano. |

## 7. Note di esecuzione

- **Fase 0.** Le cartelle `bin/` e `obj/` rimaste nella radice sono della
  vecchia posizione del progetto e non vengono più aggiornate. Il pacchetto di
  sviluppo registrato in Windows punta ancora lì: dopo il primo avvio da Visual
  Studio (che lo registra dalla nuova posizione) si possono cancellare.
- **Fase 2.** I ruoli nuovi sono pennelli, quindi hanno il suffisso `Brush`
  invece dei nomi ipotizzati nel piano: `ChartGridBrush`, `ChartLabelBrush`,
  `Deposito1Brush … Deposito8Brush`. Serve che siano pennelli perché la fase 3
  ne cambia il colore dal vivo, cosa impossibile con una risorsa `Color`.
  Stili di testo aggiunti: `AmountStyle`, `ButtonLabelStyle`,
  `BigButtonLabelStyle`, `SmallButtonLabelStyle`; per le icone
  `ButtonIconStyle` e `BigButtonIconStyle`.
- **Fase 3.** I valori dei colori ora stanno solo in `Theme/Palette.cs`
  (`Palette.Verde`); `Theme/Colors.xaml` dichiara i pennelli senza colore e
  `Tema.Applica` li riempie. `Tema.Applica` va chiamato in `App.OnLaunched`:
  nel costruttore di `App` le risorse non sono ancora accessibili e l'app si
  chiude. In debug `Tema` segnala i pennelli di `Colors.xaml` dimenticati dalla palette.

  **Esito della prova** (cambio palette ad app aperta, fatto su una copia di
  verifica con una palette di prova):
  - si aggiornano dal vivo, senza ricaricare la pagina: sfondi, bordi, testi,
    pulsanti, card dei depositi, campi di testo (`TextControl*`), messaggio
    d'errore e, tramite `TemaCambiato`, i grafici;
  - tornando alla palette verde le schermate sono identiche pixel per pixel
    a quelle di partenza;
  - `SystemAccentColor` non ha alcun effetto visibile, né prima né dopo: caselle
    di spunta e pulsante predefinito dei dialoghi usano le sfumature d'accento di
    Windows, non questa chiave.

  Da fare quando si costruirà la funzionalità dei temi, perché oggi cambierebbe
  l'aspetto dell'app:
  - dare un colore di palette a caselle di spunta, caselle a discesa e pulsante
    predefinito dei dialoghi (chiavi `AccentFillColor*` di WinUI);
  - sfondo dell'area dei grafici (oggi nero fisso) ed etichette degli assi, che
    usano ancora i colori predefiniti di LiveCharts. Per questo `Tema` fornisce
    ai grafici solo i colori che già erano impostati dall'app: griglia,
    etichette del radiale, colori dei depositi.
- **Fase 5.** Fatta: `Dialogs/` (AggiungiDeposito, OpzioniLinee, OpzioniRadiale),
  `Charts/CostruttoreGrafici`, `DatiGrafici` in Core con test, card della
  dashboard in `ItemsRepeater`, un solo aggiornamento all'apertura delle pagine.
  Le 14 schermate di confronto sono identiche a quelle di partenza.
  Provate in automatico anche le interazioni dentro i dialoghi (spunte dei
  depositi, cambio di intervallo, etichette del radiale, aggiunta di un
  deposito, aggiornamento di uno esistente, rimozione, salvataggio): 13
  schermate e la riga salvata nel CSV coincidono con quelle dell'app di partenza.
- **Fase 6.** `DataService.CaricaDati()` è chiamato da `App.OnLaunched`.
  Se il file non si riesce a leggere, l'app parte senza dati e mostra un avviso
  (prima si chiudeva). Se non si riesce a scrivere, la pagina di registrazione
  mostra l'errore e resta aperta con i valori inseriti; la rilevazione viene
  aggiunta in memoria solo dopo che la scrittura è riuscita. Provato tenendo
  il file bloccato da un altro processo, all'avvio e al salvataggio.
- **Fase 7.** Namespace per cartella: `Depositum.Pages`, `Dialogs`, `Charts`,
  `Models`, `Services`, `Theme`, `Utilities` (più `Depositum.Core` e
  `Depositum.Tests`). `App` e `MainWindow` restano nel namespace `Depositum`:
  un namespace `Depositum.App` entrerebbe in conflitto con la classe `App`.
  `App.xaml` è ora in `App/` accanto ad `App.xaml.cs`; il csproj lo dichiara
  come `ApplicationDefinition` perché in automatico viene cercato solo nella radice.

## 8. Stato delle anomalie (11/11/2026)

Refactoring concluso (fasi 0-7). Anomalie corrette, una per commit:

| N. | Stato | Scelta fatta |
|---|---|---|
| 1 | Corretta | "Ultima rilevazione" è ovunque quella con la data più recente; a parità di data, l'ultima inserita. |
| 2 | Corretta | I nomi con `,` o `:` e i nomi `Data` e `Totale` vengono rifiutati con un messaggio. Il formato del file non cambia. |
| 3 | Corretta | Il colore dipende dalla posizione del nome in tutto lo storico ordinato per data, in ogni schermata. Nella dashboard i colori di alcuni depositi possono essere cambiati una volta rispetto a prima. |
| 4 | Corretta | Le righe senza data valida vengono ignorate in lettura; all'avvio un avviso dice quante sono. Il file non viene modificato. |
| 5 | Corretta | Il totale è un numero a 64 bit: tre depositi al massimo danno 2.999.999.999 e vengono salvati e mostrati. |
| 6 | Corretta | Un solo formato in tutta l'app, senza spazio: `1.000€` (scelta dell'utente). Cambiano le etichette degli assi, i suggerimenti dei grafici e il messaggio d'errore sull'importo massimo. |
| 7 | **Da fare** | Data scritta nel file con il separatore della lingua di Windows. |

Osservazione nuova, emersa provando l'anomalia 5: nelle card della dashboard un
importo di nove cifre (`999.999.999€`) non entra nella card e viene tagliato a
sinistra. Non dipende dalle modifiche: la card ha la stessa misura di prima.

Dal 11/11/2026, su richiesta dell'utente, prima di correggere ogni anomalia si chiede
come la vuole risolvere.

Dopo l'anomalia 7 resta, in attesa di conferma, la funzionalità dei temi.
