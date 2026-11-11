using Depositum.Core;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace Depositum.Dialogs
{
    /// <summary>
    /// Opzioni del grafico a linee: intervallo di tempo e depositi da mostrare.
    /// Ogni scelta ha effetto subito: il dialogo aggiorna <see cref="Finestra"/> e l'insieme
    /// dei depositi visibili ricevuto dal chiamante, poi solleva <see cref="OpzioniCambiate"/>.
    /// </summary>
    public sealed partial class OpzioniLineeDialog : ContentDialog
    {
        /// <param name="finestra">Intervallo attualmente scelto.</param>
        /// <param name="nomiDepositi">Tutti i depositi tra cui scegliere.</param>
        /// <param name="depositiVisibili">Depositi attualmente mostrati; il dialogo lo modifica direttamente.</param>
        public OpzioniLineeDialog(FinestraTemporale finestra, IEnumerable<string> nomiDepositi, ISet<string> depositiVisibili)
        {
            InitializeComponent();

            Finestra = finestra;
            CmbIntervallo.SelectedIndex = (int)finestra;
            CmbIntervallo.SelectionChanged += (_, _) =>
            {
                if (CmbIntervallo.SelectedIndex < 0) return;

                Finestra = (FinestraTemporale)CmbIntervallo.SelectedIndex;
                OpzioniCambiate?.Invoke(this, EventArgs.Empty);
            };

            foreach (string nome in nomiDepositi)
            {
                var casella = new CheckBox { Content = nome, IsChecked = depositiVisibili.Contains(nome) };
                casella.Checked += (_, _) => { depositiVisibili.Add(nome); OpzioniCambiate?.Invoke(this, EventArgs.Empty); };
                casella.Unchecked += (_, _) => { depositiVisibili.Remove(nome); OpzioniCambiate?.Invoke(this, EventArgs.Empty); };
                Pannello.Children.Add(casella);
            }
        }

        /// <summary>Intervallo scelto.</summary>
        public FinestraTemporale Finestra { get; private set; }

        /// <summary>Sollevato a ogni modifica dell'intervallo o dei depositi visibili.</summary>
        public event EventHandler? OpzioniCambiate;
    }
}
