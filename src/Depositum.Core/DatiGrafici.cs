using System;
using System.Collections.Generic;
using System.Linq;

namespace Depositum.Core
{
    /// <summary>Intervallo di tempo mostrato dal grafico a linee, contato all'indietro dall'ultima rilevazione.</summary>
    public enum FinestraTemporale { Tutto, UnMese, SeiMesi, UnAnno, CinqueAnni }

    /// <summary>
    /// Una linea del grafico: i valori di un deposito, uno per ogni data di <see cref="DatiLinee.Date"/>
    /// (<c>null</c> dove il deposito non compare). <paramref name="IndiceColore"/> è la posizione del
    /// deposito tra tutti i nomi dello storico e decide il suo colore.
    /// </summary>
    public sealed record SerieDeposito(string Nome, int IndiceColore, IReadOnlyList<int?> Valori);

    /// <summary>Tutto ciò che serve per disegnare il grafico a linee.</summary>
    public sealed record DatiLinee(
        IReadOnlyList<DateOnly> Date,
        IReadOnlyList<SerieDeposito> Serie,
        double MinimoAsse,
        double MassimoAsse);

    /// <summary>
    /// Preparazione dei dati per i grafici: nessuna dipendenza dall'interfaccia.
    /// </summary>
    public static class DatiGrafici
    {
        // L'asse verticale arriva a 1,6 volte la media dei valori; se così taglierebbe
        // il valore più alto, arriva a 1,4 volte quel valore.
        private const float MoltiplicatoreMedia = 1.6f;
        private const float MoltiplicatoreMassimo = 1.4f;

        /// <summary>Prima data inclusa nella finestra; <c>null</c> se la finestra è tutto lo storico.</summary>
        public static DateOnly? InizioFinestra(FinestraTemporale finestra, DateOnly ultimaData) => finestra switch
        {
            FinestraTemporale.UnMese => ultimaData.AddMonths(-1),
            FinestraTemporale.SeiMesi => ultimaData.AddMonths(-6),
            FinestraTemporale.UnAnno => ultimaData.AddYears(-1),
            FinestraTemporale.CinqueAnni => ultimaData.AddYears(-5),
            _ => null
        };

        /// <summary>Nomi di tutti i depositi comparsi nelle rilevazioni, nell'ordine in cui compaiono, senza distinzione tra maiuscole e minuscole.</summary>
        public static List<string> NomiDepositi(IEnumerable<Rilevazione> rilevazioni) =>
            rilevazioni.SelectMany(r => r.Depositi.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        /// <summary>Estremi dell'asse verticale dell'istogramma per i valori indicati (almeno uno).</summary>
        public static (double Minimo, double Massimo) LimitiIstogramma(IReadOnlyCollection<int> valori) =>
            (Math.Min(0, valori.Min()), Math.Max(1, valori.Max()) * MoltiplicatoreMassimo);

        /// <summary>
        /// Prepara il grafico a linee.
        /// </summary>
        /// <param name="rilevazioni">Rilevazioni ordinate per data, almeno una.</param>
        /// <param name="finestra">Intervallo da mostrare.</param>
        /// <param name="depositiVisibili">Nomi dei depositi scelti dall'utente.</param>
        /// <returns><c>null</c> se nell'intervallo non c'è alcun valore dei depositi scelti.</returns>
        public static DatiLinee? PreparaLinee(
            IReadOnlyList<Rilevazione> rilevazioni,
            FinestraTemporale finestra,
            IReadOnlySet<string> depositiVisibili)
        {
            DateOnly? inizio = InizioFinestra(finestra, rilevazioni[^1].Data);
            var visibili = rilevazioni.Where(r => inizio is null || r.Data >= inizio.Value).ToList();

            // I nomi si contano su tutto lo storico, non solo sulla finestra:
            // così il colore di un deposito non cambia al cambiare dell'intervallo.
            var nomi = NomiDepositi(rilevazioni);

            var valoriPerDeposito = new Dictionary<string, int?[]>(StringComparer.OrdinalIgnoreCase);
            foreach (string nome in nomi)
            {
                if (depositiVisibili.Contains(nome))
                    valoriPerDeposito[nome] = new int?[visibili.Count];
            }

            long sommaValori = 0;
            int massimoValore = int.MinValue;
            int minimoValore = int.MaxValue;
            int numeroValori = 0;
            for (int indiceRilevazione = 0; indiceRilevazione < visibili.Count; indiceRilevazione++)
            {
                foreach (var deposito in visibili[indiceRilevazione].Depositi)
                {
                    if (!valoriPerDeposito.TryGetValue(deposito.Key, out var valori))
                        continue;

                    valori[indiceRilevazione] = deposito.Value;
                    sommaValori += deposito.Value;
                    numeroValori++;
                    if (deposito.Value > massimoValore)
                        massimoValore = deposito.Value;
                    if (deposito.Value < minimoValore)
                        minimoValore = deposito.Value;
                }
            }

            var serie = new List<SerieDeposito>();
            for (int indiceColore = 0; indiceColore < nomi.Count; indiceColore++)
            {
                string nome = nomi[indiceColore];
                if (!valoriPerDeposito.TryGetValue(nome, out var valori) || valori.All(v => v is null))
                    continue;

                serie.Add(new SerieDeposito(nome, indiceColore, valori));
            }

            if (serie.Count == 0 || numeroValori == 0)
                return null;

            double mediaValori = (double)sommaValori / numeroValori;
            double valoreMassimoAsse = mediaValori * MoltiplicatoreMedia;
            if (valoreMassimoAsse < massimoValore)
                valoreMassimoAsse = massimoValore * MoltiplicatoreMassimo;

            return new DatiLinee(
                visibili.Select(r => r.Data).ToList(),
                serie,
                Math.Min(0, minimoValore),
                Math.Max(1, valoreMassimoAsse));
        }
    }
}
