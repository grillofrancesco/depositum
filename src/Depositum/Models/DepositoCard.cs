using Microsoft.UI.Xaml.Media;

namespace Depositum.Models
{
    /// <summary>
    /// Una card della dashboard: nome del deposito, importo già formattato e colore di sfondo.
    /// </summary>
    public sealed record DepositoCard(string Nome, string Importo, Brush Sfondo);
}
