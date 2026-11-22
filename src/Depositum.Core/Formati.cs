using System;
using System.Globalization;

namespace Depositum.Core
{
    /// <summary>Lingue dell'interfaccia.</summary>
    public enum Lingua { Italiano, Inglese }

    /// <summary>
    /// Come l'app scrive e legge date e importi. Tutte le conversioni passano da qui.
    /// <para>
    /// A schermo il formato segue la lingua dell'interfaccia (<see cref="LinguaSchermo"/>):
    /// in italiano <c>05/01/2025</c> e <c>1.800€</c>, in inglese <c>01/05/2025</c> e <c>€1,800</c>.
    /// Nel file history.csv il formato è sempre lo stesso, qualunque sia la lingua.
    /// </para>
    /// </summary>
    public static class Formati
    {
        private static readonly CultureInfo CulturaItaliana = CultureInfo.GetCultureInfo("it-IT");
        private static readonly CultureInfo CulturaInglese = CultureInfo.GetCultureInfo("en-US");

        // Formati a schermo, per lingua: il primo è quello con cui l'app scrive le date.
        private static readonly string[] FormatiSchermoItaliano = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };
        private static readonly string[] FormatiSchermoInglese = { "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd" };

        // Formati del file: sempre con la cultura invariante, il separatore resta quello scritto qui.
        private const string FormatoDataFile = "dd-MM-yyyy";

        private static readonly string[] FormatiDataFileAccettati =
        {
            "dd/MM/yyyy", "d/M/yyyy",   // righe salvate prima del passaggio al trattino
            FormatoDataFile, "d-M-yyyy",
            "yyyy-MM-dd"
        };

        /// <summary>
        /// Lingua in cui si mostrano date e importi. L'app la imposta all'avvio, uguale a quella
        /// dei testi; vale <see cref="Lingua.Italiano"/> finché nessuno la cambia.
        /// </summary>
        public static Lingua LinguaSchermo { get; set; } = Lingua.Italiano;

        /// <summary>Convenzioni numeriche della lingua a schermo (separatori, percentuali).</summary>
        public static CultureInfo CulturaSchermo => LinguaSchermo == Lingua.Inglese ? CulturaInglese : CulturaItaliana;

        private static string[] FormatiSchermo => LinguaSchermo == Lingua.Inglese ? FormatiSchermoInglese : FormatiSchermoItaliano;

        // ---------- Date a schermo ----------

        /// <summary>Data come viene mostrata nell'app: gg/mm/aaaa in italiano, mm/gg/aaaa in inglese.</summary>
        public static string Data(DateOnly data) => data.ToString(FormatiSchermo[0], CultureInfo.InvariantCulture);

        /// <summary>
        /// Interpreta una data scritta dall'utente, nel formato della lingua a schermo
        /// (anche senza zeri iniziali) o come aaaa-mm-gg; in ultimo, qualunque data valida
        /// per quella lingua.
        /// </summary>
        public static bool ProvaALeggereDataInserita(string testo, out DateOnly data)
        {
            return DateOnly.TryParseExact(testo, FormatiSchermo, CultureInfo.InvariantCulture, DateTimeStyles.None, out data)
                || DateOnly.TryParse(testo, CulturaSchermo, DateTimeStyles.None, out data);
        }

        // ---------- Date nel file ----------

        /// <summary>Data come viene salvata in history.csv: gg-mm-aaaa.</summary>
        public static string DataPerFile(DateOnly data) => data.ToString(FormatoDataFile, CultureInfo.InvariantCulture);

        /// <summary>
        /// Interpreta una data letta da history.csv: prima i formati gg-mm-aaaa, g-m-aaaa,
        /// gg/mm/aaaa, g/m/aaaa e aaaa-mm-gg, poi qualunque data valida per la lingua di Windows.
        /// Le righe salvate prima del passaggio a gg-mm-aaaa (con la barra) continuano a essere lette.
        /// Non dipende dalla lingua dell'interfaccia.
        /// </summary>
        public static bool ProvaALeggereDataDelFile(string testo, out DateOnly data)
        {
            return DateOnly.TryParseExact(testo, FormatiDataFileAccettati, CultureInfo.InvariantCulture, DateTimeStyles.None, out data)
                || DateOnly.TryParse(testo, out data);
        }

        // ---------- Importi ----------

        /// <summary>Importo come appare in tutta l'app: <c>1.800€</c> in italiano, <c>€1,800</c> in inglese.</summary>
        public static string Importo(long valore) => ConSimbolo(valore.ToString("N0", CulturaSchermo));

        /// <summary>
        /// Come <see cref="Importo(long)"/>, per i valori dei grafici (assi e suggerimenti),
        /// che possono non essere interi: vengono arrotondati all'unità.
        /// </summary>
        public static string Importo(double valore) => ConSimbolo(valore.ToString("N0", CulturaSchermo));

        /// <summary>Percentuale con un decimale, nelle convenzioni della lingua a schermo.</summary>
        public static string Percentuale(double frazione) => frazione.ToString("P1", CulturaSchermo);

        private static string ConSimbolo(string numero)
        {
            if (LinguaSchermo != Lingua.Inglese)
                return numero + "€";

            // In inglese il simbolo va prima del numero, e prima del segno meno: -€1,800.
            return numero.StartsWith('-') ? "-€" + numero[1..] : "€" + numero;
        }
    }
}
