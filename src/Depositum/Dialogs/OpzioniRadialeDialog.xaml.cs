using Microsoft.UI.Xaml.Controls;

namespace Depositum
{
    /// <summary>
    /// Opzioni del grafico radiale: mostrare o no i nomi dei depositi sulle fette.
    /// </summary>
    public sealed partial class OpzioniRadialeDialog : ContentDialog
    {
        public OpzioniRadialeDialog()
        {
            InitializeComponent();
        }

        public bool MostraEtichette
        {
            get => ChkEtichette.IsChecked == true;
            set => ChkEtichette.IsChecked = value;
        }
    }
}
