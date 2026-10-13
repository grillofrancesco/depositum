using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace codebase
{
    /// <summary>
    /// Modello per la modifica interattiva di un singolo deposito nella pagina di registrazione.
    /// Tutti i valori dei depositi sono rigorosamente numeri interi (int).
    /// </summary>
    public class DepositoEditItem : INotifyPropertyChanged
    {
        private string _nome = string.Empty;
        private int _valore;

        public string Nome
        {
            get => _nome;
            set
            {
                if (_nome != value)
                {
                    _nome = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Valore
        {
            get => _valore;
            set
            {
                if (_valore != value)
                {
                    _valore = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
