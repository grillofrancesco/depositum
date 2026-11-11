using System.Collections.Generic;
using System.Linq;

namespace Depositum.Core
{
    /// <summary>
    /// Domande sull'insieme delle rilevazioni. Le rilevazioni possono essere state inserite
    /// in qualunque ordine: qui "ultima" vuol dire sempre "con la data più recente".
    /// </summary>
    public static class Storico
    {
        /// <summary>
        /// Rilevazioni dalla più vecchia alla più recente.
        /// A parità di data resta l'ordine di inserimento.
        /// </summary>
        public static List<Rilevazione> OrdinatePerData(IEnumerable<Rilevazione> rilevazioni) =>
            rilevazioni.OrderBy(r => r.Data).ToList();

        /// <summary>
        /// La rilevazione con la data più recente; a parità di data, quella inserita per ultima.
        /// <c>null</c> se non ce ne sono.
        /// </summary>
        public static Rilevazione? PiuRecente(IEnumerable<Rilevazione> rilevazioni) =>
            rilevazioni.OrderBy(r => r.Data).LastOrDefault();
    }
}
