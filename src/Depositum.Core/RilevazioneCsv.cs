using System;
using System.Globalization;
using System.Text;

namespace Depositum.Core
{
    /// <summary>
    /// Conversione tra una riga di 'history.csv' e una <see cref="Rilevazione"/>.
    /// Formato di una riga: <c>Data:gg/mm/aaaa,NomeDeposito:importo,...,Totale:importo</c>
    /// </summary>
    public static class RilevazioneCsv
    {
        /// <summary>
        /// Interpreta una riga del file. Restituisce <c>null</c> se la riga è vuota oppure
        /// non contiene una data valida: senza data una rilevazione non si può collocare nel tempo.
        /// Gli altri campi non riconoscibili vengono ignorati senza segnalazione.
        /// </summary>
        public static Rilevazione? DaRiga(string rigaGrezza)
        {
            var riga = rigaGrezza.Trim();
            if (string.IsNullOrWhiteSpace(riga))
                return null;

            riga = riga.TrimEnd(';');
            var campi = riga.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var rilevazione = new Rilevazione();
            bool dataTrovata = false;

            foreach (var campo in campi)
            {
                int colonIndex = campo.IndexOf(':');
                if (colonIndex <= 0)
                    continue;

                string chiave = campo.Substring(0, colonIndex).Trim();
                string valore = campo.Substring(colonIndex + 1).Trim().TrimEnd(';');

                if (string.Equals(chiave, "Data", StringComparison.OrdinalIgnoreCase))
                {
                    if (Formati.ProvaALeggereData(valore, out var data))
                    {
                        rilevazione.Data = data;
                        dataTrovata = true;
                    }
                }
                else if (string.Equals(chiave, "Totale", StringComparison.OrdinalIgnoreCase))
                {
                    if (long.TryParse(valore, NumberStyles.Integer, CultureInfo.InvariantCulture, out long tot))
                    {
                        rilevazione.Totale = tot;
                    }
                }
                else
                {
                    if (int.TryParse(valore, NumberStyles.Integer, CultureInfo.InvariantCulture, out int importo))
                    {
                        rilevazione.Depositi[chiave] = importo;
                    }
                }
            }

            return dataTrovata ? rilevazione : null;
        }

        /// <summary>
        /// Produce la riga da appendere al file per la rilevazione indicata.
        /// </summary>
        public static string InRiga(Rilevazione rilevazione)
        {
            var sb = new StringBuilder();
            sb.Append($"Data:{Formati.Data(rilevazione.Data)}");
            foreach (var dep in rilevazione.Depositi)
            {
                sb.Append($",{dep.Key}:{dep.Value}");
            }
            sb.Append($",Totale:{rilevazione.Totale}");

            return sb.ToString();
        }
    }
}
