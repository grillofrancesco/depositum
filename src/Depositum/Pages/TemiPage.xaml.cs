using Depositum.Core.Temi;
using Depositum.Models;
using Depositum.Services;
using Depositum.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;

namespace Depositum.Pages
{
    /// <summary>
    /// Scelta del tema di colore. Il tema toccato si applica subito a tutta l'app e viene
    /// ricordato per i prossimi avvii.
    /// </summary>
    public sealed partial class TemiPage : Page
    {
        public TemiPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MostraTemi();
        }

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

            if (PreferenzaTema.Salva(voce.Palette))
            {
                StatusInfoBar.IsOpen = false;
            }
            else
            {
                StatusInfoBar.Title = "Tema non salvato";
                StatusInfoBar.Message = "Il tema è applicato, ma non è stato possibile ricordarlo: al prossimo avvio tornerà quello precedente.";
                StatusInfoBar.IsOpen = true;
            }
        }

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            Frame.TornaIndietro();
        }
    }
}
