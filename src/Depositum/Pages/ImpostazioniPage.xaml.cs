using Depositum.Core.Temi;
using Depositum.Models;
using Depositum.Services;
using Depositum.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using System.Linq;

namespace Depositum.Pages
{
    /// <summary>
    /// Impostazioni: lingua e tema di colore.
    /// Il tema toccato si applica subito a tutta l'app; la lingua al riavvio, perché Windows
    /// carica i testi all'avvio. Entrambe le scelte vengono ricordate.
    /// </summary>
    public sealed partial class ImpostazioniPage : Page
    {
        // Evita che la selezione iniziale della lingua venga presa per una scelta dell'utente.
        private bool _inizializzazione;

        public ImpostazioniPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _inizializzazione = true;
            string scelta = Testi.LinguaScelta;
            CmbLingua.SelectedItem = CmbLingua.Items.OfType<ComboBoxItem>().First(i => LinguaDi(i) == scelta);
            _inizializzazione = false;

            MostraTemi();
        }

        // ---------- Lingua ----------

        private void CmbLingua_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_inizializzazione || CmbLingua.SelectedItem is not ComboBoxItem voce)
                return;

            string lingua = LinguaDi(voce);

            if (!Testi.SalvaLinguaScelta(lingua))
            {
                MostraAvviso(InfoBarSeverity.Warning, "Lingua_NonSalvata_Titolo", "Lingua_NonSalvata_Testo");
                return;
            }

            var riavvia = new Button { Content = Testi.Get("Lingua_Riavvio_Pulsante") };
            riavvia.Click += (_, _) => Riavvia();
            MostraAvviso(InfoBarSeverity.Informational, "Lingua_Riavvio_Titolo", "Lingua_Riavvio_Testo", riavvia);
        }

        /// <summary>Lingua di una voce del menu. Nello XAML un Tag vuoto diventa null: è la lingua di Windows.</summary>
        private static string LinguaDi(ComboBoxItem voce) => voce.Tag as string ?? Testi.Automatica;

        private void Riavvia()
        {
            // Se il riavvio riesce, l'app si chiude qui e non si arriva alla riga dopo.
            AppInstance.Restart(string.Empty);
            StatusInfoBar.Message = Testi.Get("Lingua_RiavvioFallito");
            StatusInfoBar.ActionButton = null;
        }

        // ---------- Tema ----------

        private void MostraTemi()
        {
            TemiRepeater.ItemsSource = Palette.Tutte
                .Select(p => new TemaCard(p, p.Id == Tema.Corrente.Id))
                .ToList();
        }

        private void Tema_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: TemaCard voce } || voce.Selezionato)
                return;

            Tema.Applica(voce.Palette);
            MostraTemi();

            if (!PreferenzaTema.Salva(voce.Palette))
                MostraAvviso(InfoBarSeverity.Warning, "Tema_NonSalvato_Titolo", "Tema_NonSalvato_Testo");
        }

        // ---------- Comuni ----------

        private void MostraAvviso(InfoBarSeverity gravita, string chiaveTitolo, string chiaveTesto, ButtonBase? azione = null)
        {
            StatusInfoBar.Severity = gravita;
            StatusInfoBar.Title = Testi.Get(chiaveTitolo);
            StatusInfoBar.Message = Testi.Get(chiaveTesto);
            StatusInfoBar.ActionButton = azione;
            StatusInfoBar.IsOpen = true;
        }

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            Frame.TornaIndietro();
        }
    }
}
