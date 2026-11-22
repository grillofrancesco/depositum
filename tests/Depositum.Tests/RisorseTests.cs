using Depositum.Core;
using Depositum.Core.Temi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Depositum.Tests
{
    /// <summary>
    /// Controlli sui testi dell'interfaccia (src/Depositum/Strings): le due lingue devono avere
    /// le stesse chiavi, e ogni testo usato da XAML o codice deve esistere.
    /// I test leggono i file del progetto, non l'app compilata.
    /// </summary>
    public class RisorseTests
    {
        private static readonly string CartellaApp = TrovaCartellaApp();

        private static readonly Dictionary<string, string> Italiano = Leggi("it-IT");
        private static readonly Dictionary<string, string> Inglese = Leggi("en-US");

        private static string TrovaCartellaApp()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                string candidata = Path.Combine(dir.FullName, "src", "Depositum");
                if (Directory.Exists(Path.Combine(candidata, "Strings")))
                    return candidata;
            }
            throw new DirectoryNotFoundException("Cartella src/Depositum/Strings non trovata risalendo da " + AppContext.BaseDirectory);
        }

        private static Dictionary<string, string> Leggi(string lingua)
        {
            var doc = XDocument.Load(Path.Combine(CartellaApp, "Strings", lingua, "Resources.resw"));
            return doc.Root!.Elements("data").ToDictionary(
                d => (string)d.Attribute("name")!,
                d => (string?)d.Element("value") ?? string.Empty);
        }

        private static IEnumerable<string> FileSorgente(string estensione) =>
            Directory.EnumerateFiles(CartellaApp, "*" + estensione, SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
                         && !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar));

        [Fact]
        public void LeDueLingueHannoLeStesseChiavi()
        {
            Assert.Empty(Italiano.Keys.Except(Inglese.Keys));
            Assert.Empty(Inglese.Keys.Except(Italiano.Keys));
        }

        [Fact]
        public void NessunTestoVuoto()
        {
            Assert.Empty(Italiano.Where(t => string.IsNullOrWhiteSpace(t.Value)).Select(t => "it-IT: " + t.Key));
            Assert.Empty(Inglese.Where(t => string.IsNullOrWhiteSpace(t.Value)).Select(t => "en-US: " + t.Key));
        }

        [Fact]
        public void CodiceLingua_UnoPerLingua()
        {
            Assert.Equal("it", Italiano["CodiceLingua"]);
            Assert.Equal("en", Inglese["CodiceLingua"]);
        }

        [Fact]
        public void StessiSegnapostoNelleDueLingue()
        {
            // {0}, {1}… devono comparire in entrambe, altrimenti un valore andrebbe perso.
            var segnaposto = new Regex(@"\{\d+\}");
            foreach (var (chiave, italiano) in Italiano)
            {
                var it = segnaposto.Matches(italiano).Select(m => m.Value).OrderBy(s => s);
                var en = segnaposto.Matches(Inglese[chiave]).Select(m => m.Value).OrderBy(s => s);
                Assert.True(it.SequenceEqual(en), $"Segnaposto diversi in '{chiave}'");
            }
        }

        [Fact]
        public void OgniXUidHaIlSuoTesto()
        {
            var uid = new Regex("x:Uid=\"([^\"]+)\"");
            var mancanti = FileSorgente(".xaml")
                .SelectMany(f => uid.Matches(File.ReadAllText(f)).Select(m => (File: Path.GetFileName(f), Uid: m.Groups[1].Value)))
                .Where(u => !Italiano.Keys.Any(k => k.StartsWith(u.Uid + ".", StringComparison.Ordinal)))
                .Select(u => $"{u.File}: {u.Uid}")
                .ToList();

            Assert.Empty(mancanti);
        }

        [Fact]
        public void OgniTestoChiestoDalCodiceEsiste()
        {
            var chiamata = new Regex("Testi\\.(?:Get|Formato)\\(\"([^\"]+)\"");
            var mancanti = FileSorgente(".cs")
                .SelectMany(f => chiamata.Matches(File.ReadAllText(f)).Select(m => (File: Path.GetFileName(f), Chiave: m.Groups[1].Value)))
                .Where(c => !Italiano.ContainsKey(c.Chiave))
                .Select(c => $"{c.File}: {c.Chiave}")
                .ToList();

            Assert.Empty(mancanti);
        }

        [Fact]
        public void OgniErroreDiValidazioneHaIlSuoMessaggio()
        {
            foreach (var errore in Enum.GetValues<ErroreDeposito>())
                Assert.True(Italiano.ContainsKey("ErroreDeposito_" + errore), "Manca ErroreDeposito_" + errore);
        }

        [Fact]
        public void OgniTemaHaNomeEDescrizione()
        {
            foreach (var palette in Palette.Tutte)
            {
                Assert.True(Italiano.ContainsKey($"Tema_{palette.Id}_Nome"), "Manca il nome del tema " + palette.Id);
                Assert.True(Italiano.ContainsKey($"Tema_{palette.Id}_Descrizione"), "Manca la descrizione del tema " + palette.Id);
            }
        }

        [Fact]
        public void NomiDeiTemiInItalianoUgualiAQuelliDellePalette()
        {
            foreach (var palette in Palette.Tutte)
                Assert.Equal(palette.Nome, Italiano[$"Tema_{palette.Id}_Nome"]);
        }

        [Fact]
        public void FormatoDataSuggerito_CoerenteConLaLingua()
        {
            // Il campo data vuoto suggerisce lo stesso ordine che Formati usa per quella lingua.
            Assert.Equal("gg/mm/aaaa", Italiano["RegistraDataInput.PlaceholderText"]);
            Assert.Equal("mm/dd/yyyy", Inglese["RegistraDataInput.PlaceholderText"]);
        }
    }
}
