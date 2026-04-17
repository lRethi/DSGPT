using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSGPT
{
    public class KNN
    {
        private List<DataPoint> data;
        private int k;

        public KNN(List<DataPoint> data, int k)
        {
            this.data = data;
            this.k = k;
        }

        public string Predict(double[] input)
        {
            var distances = new List<(double distance, string label)>();

            // Calcula distância para todos os pontos
            foreach (var point in data)
            {
                double dist = EuclideanDistance(input, point.Features);
                distances.Add((dist, point.Label));
            }

            // Ordena pelos mais próximos
            var nearest = distances
                .OrderBy(d => d.distance)
                .Take(k);

            // Votação
            var result = nearest
                .GroupBy(n => n.label)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            return result;
        }

        private double EuclideanDistance(double[] a, double[] b)
        {
            double sum = 0;

            for (int i = 0; i < a.Length; i++)
            {
                sum += Math.Pow(a[i] - b[i], 2);
            }

            return Math.Sqrt(sum);
        }
    }
}
