using Depositum.Core.Temi;
using Depositum.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;

namespace Depositum.Models
{
    /// <summary>
    /// Una voce della scelta del tema: nome, descrizione e un'anteprima dei colori della palette.
    /// I campioni usano i colori fissi della palette, non quelli del tema attuale, così ogni
    /// voce mostra il proprio aspetto qualunque sia il tema applicato.
    /// </summary>
    public sealed class TemaCard
    {
        /// <summary>Quanti colori dei depositi mostrare nell'anteprima.</summary>
        private const int CampioniDeposito = 6;

        public TemaCard(Palette palette, bool selezionato)
        {
            Palette = palette;
            Selezionato = selezionato;
            Accento = Pennello(palette.AccentActionBackground);
            SfondoAnteprima = Pennello(palette.Canvas);
            BordoAnteprima = Pennello(palette.BorderSubtle);
            CampioniDepositi = palette.Depositi.Take(CampioniDeposito).Select(Pennello).ToList();
        }

        public Palette Palette { get; }

        /// <summary>Nome nella lingua dell'interfaccia (dai file dei testi, non dalla palette).</summary>
        public string Nome => Testi.Get($"Tema_{Palette.Id}_Nome");

        public string Descrizione => Testi.Get($"Tema_{Palette.Id}_Descrizione");

        /// <summary>Identificativo per l'automazione dell'interfaccia (per esempio "Tema_acqua").</summary>
        public string IdAutomazione => "Tema_" + Palette.Id;

        public bool Selezionato { get; }

        public Visibility VisibilitaSpunta => Selezionato ? Visibility.Visible : Visibility.Collapsed;

        public Brush Accento { get; }

        public Brush SfondoAnteprima { get; }

        public Brush BordoAnteprima { get; }

        public IReadOnlyList<Brush> CampioniDepositi { get; }

        private static Brush Pennello(ColoreRgb colore) => new SolidColorBrush(Tema.InColore(colore));
    }
}
