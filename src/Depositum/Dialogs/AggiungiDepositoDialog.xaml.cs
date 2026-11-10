using Depositum.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Depositum
{
    /// <summary>
    /// Chiede nome e importo di un nuovo deposito. Si chiude con "Aggiungi" solo se i valori
    /// rispettano <see cref="RegoleDeposito"/>; altrimenti resta aperto e mostra l'errore.
    /// Esito: <see cref="ContentDialogResult.Secondary"/> quando l'utente ha confermato.
    /// </summary>
    public sealed partial class AggiungiDepositoDialog : ContentDialog
    {
        public AggiungiDepositoDialog()
        {
            InitializeComponent();
            TxtNome.MaxLength = RegoleDeposito.LunghezzaMassimaNome;
        }

        /// <summary>Nome inserito, senza spazi ai lati.</summary>
        public string NomeDeposito => TxtNome.Text.Trim();

        /// <summary>Importo inserito. Significativo solo dopo una conferma andata a buon fine.</summary>
        public int Importo => (int)NumValore.Value;

        private void Aggiungi_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string? errore = RegoleDeposito.Valida(TxtNome.Text, NumValore.Value);
            if (errore != null)
            {
                TxtErrore.Text = errore;
                TxtErrore.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            TxtErrore.Visibility = Visibility.Collapsed;
        }
    }
}
