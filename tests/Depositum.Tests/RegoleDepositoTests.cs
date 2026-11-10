using Depositum.Core;
using Xunit;

namespace Depositum.Tests
{
    public class RegoleDepositoTests
    {
        private static readonly string NomeDi30 = new('a', 30);
        private static readonly string NomeDi31 = new('a', 31);

        [Theory]
        [InlineData("Conto")]
        [InlineData("  Conto  ")]
        [InlineData("a")]
        public void NomeValido_Accettato(string nome)
        {
            Assert.True(RegoleDeposito.NomeValido(nome));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void NomeValido_VuotoRifiutato(string nome)
        {
            Assert.False(RegoleDeposito.NomeValido(nome));
        }

        [Fact]
        public void NomeValido_LimiteDiLunghezza()
        {
            Assert.True(RegoleDeposito.NomeValido(NomeDi30));
            Assert.False(RegoleDeposito.NomeValido(NomeDi31));
            // gli spazi ai lati non contano
            Assert.True(RegoleDeposito.NomeValido("  " + NomeDi30 + "  "));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(500)]
        [InlineData(999_999_999)]
        public void ImportoValido_Accettato(double valore)
        {
            Assert.True(RegoleDeposito.ImportoValido(valore));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1_000_000_000)]
        [InlineData(0.5)]
        [InlineData(12.5)]
        [InlineData(999_999_999.5)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ImportoValido_Rifiutato(double valore)
        {
            Assert.False(RegoleDeposito.ImportoValido(valore));
        }

        [Fact]
        public void Valida_TuttoCorretto_NessunMessaggio()
        {
            Assert.Null(RegoleDeposito.Valida("Conto", 100));
        }

        [Fact]
        public void Valida_NomeVuoto()
        {
            Assert.Equal("Il nome del deposito non può essere vuoto.", RegoleDeposito.Valida("   ", 100));
        }

        [Fact]
        public void Valida_NomeTroppoLungo()
        {
            Assert.Equal("La lunghezza del nome non deve superare 30 caratteri.", RegoleDeposito.Valida(NomeDi31, 100));
        }

        [Fact]
        public void Valida_ImportoNonValido()
        {
            Assert.Equal("L'importo deve essere un numero intero compreso tra 1 e 1mlrd", RegoleDeposito.Valida("Conto", 0));
        }

        [Fact]
        public void Valida_IlNomeVieneControllatoPrimaDellImporto()
        {
            Assert.Equal("Il nome del deposito non può essere vuoto.", RegoleDeposito.Valida("", 0));
        }

        // Anomalia 2 (docs/piano-refactoring.md, sezione 4): virgola e due punti nel nome
        // sono accettati, ma rovinano la riga del CSV.
        [Theory]
        [InlineData("Conto: BNL")]
        [InlineData("Casa, mare")]
        public void Anomalia2_NomiCheRovinanoIlCsvSonoAccettati(string nome)
        {
            Assert.True(RegoleDeposito.NomeValido(nome));
            Assert.Null(RegoleDeposito.Valida(nome, 100));
        }
    }
}
