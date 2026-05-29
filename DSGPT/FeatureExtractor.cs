using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DSGPT
{
    public enum FeatureType
    {
        Comprimento = 0,
        Largura = 1,
        Altura = 2,
        Peso = 3,
        Consumo = 4,
        Conectores = 5,

        PCIe = 6,
        SATA = 7,
        NVMe = 8,
        DIMM = 9,
        Socket = 10
    }

    public class KeywordRule
    {
        public FeatureType Feature { get; set; }
        public string[] Keywords { get; set; }
        public double Value { get; set; } = 1.0;
    }

    public class FeatureExtractor
    {
        // constantes de normalização
        private const double MAX_COMPRIMENTO = 400.0;
        private const double MAX_PESO = 2000.0;
        private const double MAX_CONSUMO = 500.0;
        private const double MAX_CONECTORES = 4.0;

        private Dictionary<string, double> intensidades =
        new Dictionary<string, double>()
        {
            { "muito", 1.0 },
            { "bem", 0.8 },
            { "bastante", 0.7 },
            { "meio", 0.5 },
            { "um pouco", 0.3 }
        };

        private Dictionary<string, (FeatureType feature, bool invertido)> descricoes =
        new Dictionary<string, (FeatureType feature, bool invertido)>()
        {
            { "grande", (FeatureType.Comprimento, false) },
            { "longo", (FeatureType.Comprimento, false) },
            { "alto", (FeatureType.Altura, false) },

            { "pequeno", (FeatureType.Comprimento, true) },
            { "baixo", (FeatureType.Altura, true) },

            { "pesado", (FeatureType.Peso, false) },
            { "leve", (FeatureType.Peso, true) }
        };

        private List<KeywordRule> rules;

        public FeatureExtractor()
        {
            rules = new List<KeywordRule>
            {
                new KeywordRule
                {
                    Feature = FeatureType.NVMe,
                    Keywords = new[] { "nvme", "m.2",
                                        "2280", "2260", "2242",
                                        "pcie 4.0", "pcie 3.0",
                                        "ssd m2", "ssd m.2", "vários conectores" }
                },
                new KeywordRule
                {
                    Feature = FeatureType.SATA,
                    Keywords = new[] { "sata", "sata3", "sata 3",
                                    "2.5", "2.5\"",
                                    "cabo sata",
                                    "hd", "hdd", "vários conectores" }
                },
                new KeywordRule
                {
                    Feature = FeatureType.PCIe,
                    Keywords = new[] { "gpu", "placa de video", "pcie",
                                        "rtx", "gtx", "radeon",
                                        "vga",
                                        "gddr5", "gddr6",
                                        "fan", "cooler gpu", "vários conectores" }
                },
                new KeywordRule
                {
                    Feature = FeatureType.DIMM,
                    Keywords = new[] { "ram", "ddr", "dimm",
                                        "ddr3", "ddr4", "ddr5",
                                        "2666mhz", "3200mhz", "3600mhz",
                                        "mhz",
                                        "memoria", "vários conectores" }
                },
                new KeywordRule
                {
                    Feature = FeatureType.Socket,
                    Keywords = new[] { "cpu", "processador", "ryzen", "intel",
                                        "i3", "i5", "i7", "i9",
                                        "am4", "am5", "lga", "lga1200", "lga1700",
                                        "socket", "soquete", "vários conectores" }
                }
            };
        }

        public double[] Extract(string text)
        {
            double[] features = new double[11];
            text = text.ToLower();

            // palavras-chave (one-hot)
            foreach (var rule in rules)
            {
                if (rule.Keywords.Any(k => text.Contains(k)))
                {
                    features[(int)rule.Feature] = rule.Value;
                }
            }

            // comprimento (cm → mm)
            var matchCm = Regex.Match(text, @"(\d+([.,]\d+)?)\s*(cm)");
            if (matchCm.Success)
            {
                double val;
                if (double.TryParse(matchCm.Groups[1].Value.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out val))
                {
                    features[(int)FeatureType.Comprimento] = (val * 10.0) / MAX_COMPRIMENTO;
                }
            }

            // peso (kg → g)
            var matchKg = Regex.Match(text, @"(\d+([.,]\d+)?)\s*(kg)");
            if (matchKg.Success)
            {
                double val;
                if (double.TryParse(matchKg.Groups[1].Value.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out val))
                {
                    features[(int)FeatureType.Peso] = (val * 1000.0) / MAX_PESO;
                }
            }

            // consumo (W)
            var matchW = Regex.Match(text, @"(\d+([.,]\d+)?)\s*(w|watts)");
            if (matchW.Success)
            {
                double val;
                if (double.TryParse(matchW.Groups[1].Value.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out val))
                {
                    features[(int)FeatureType.Consumo] = val / MAX_CONSUMO;
                }
            }

            // conectores
            if (text.Contains("2 cabos") || text.Contains("2 conectores"))
                features[(int)FeatureType.Conectores] = 2.0 / MAX_CONECTORES;

            if (text.Contains("1 cabo"))
                features[(int)FeatureType.Conectores] = 1.0 / MAX_CONECTORES;

            // qualitativo (pequeno, pesado, etc)
            ApplyQualitative(text, features);

            return features;
        }

        private void ApplyQualitative(string text, double[] features)
        {
            foreach (var desc in descricoes)
            {
                if (text.Contains(desc.Key))
                {
                    double intensidade = 0.5;

                    foreach (var inten in intensidades)
                    {
                        if (text.Contains(inten.Key))
                        {
                            intensidade = inten.Value;
                            break;
                        }
                    }

                    double valor = desc.Value.invertido
                        ? 1.0 - intensidade
                        : intensidade;

                    int index = (int)desc.Value.feature;

                    if (features[index] == 0)
                        features[index] = valor;
                }
            }
        }
    }
}