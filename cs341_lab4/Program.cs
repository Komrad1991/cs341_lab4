using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Использование: program <input-file> <output-file>");
            return 1;
        }

        string inputPath = args[0];
        string outputPath = args[1];

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Файл не найден: {inputPath}");
            return 1;
        }

        var inputLines = new List<string>(File.ReadAllLines(inputPath));

        if (inputLines.Count == 0)
        {
            File.WriteAllText(outputPath, string.Empty);
            return 0;
        }

        var vertexNames = inputLines[0].Split(
            new[] { ' ', '\t' },
            StringSplitOptions.RemoveEmptyEntries
        );

        int n = vertexNames.Length;

        var id = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < n; i++)
            id[vertexNames[i]] = i;

        var graph = new List<(int to, double weight)>[n];
        for (int i = 0; i < n; i++)
            graph[i] = new List<(int, double)>();

        bool hasNegativeEdge = false;

        for (int i = 1; i < inputLines.Count; i++)
        {
            var parts = inputLines[i].Split(
                new[] { ' ', '\t' },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (parts.Length < 3)
                continue;

            string from = parts[0];
            string to = parts[1];
            double weight = double.Parse(parts[2], CultureInfo.InvariantCulture);

            if (weight < 0)
                hasNegativeEdge = true;

            int u = id[from];
            int v = id[to];

            graph[u].Add((v, weight));
        }

        if (hasNegativeEdge)
        {
            File.WriteAllText(outputPath, "Есть дуга отрицательного веса.\n");
            return 0;
        }

        var dist = new double[n];
        for (int i = 0; i < n; i++)
            dist[i] = double.PositiveInfinity;

        dist[0] = 0;

        // Встроенная очередь с приоритетами (.NET 6+):
        // элемент — номер вершины, приоритет — известное расстояние.
        var pq = new PriorityQueue<int, double>();
        pq.Enqueue(0, 0);

        while (pq.Count > 0)
        {
            if (!pq.TryDequeue(out int u, out double currentDist))
                break;

            // Устаревшая запись — расстояние уже улучшено раньше.
            if (currentDist > dist[u])
                continue;

            foreach (var edge in graph[u])
            {
                int v = edge.to;
                double newDist = currentDist + edge.weight;

                if (newDist < dist[v])
                {
                    dist[v] = newDist;
                    // decrease-key нет — кладём дубликат (lazy deletion).
                    pq.Enqueue(v, newDist);
                }
            }
        }

        var sb = new StringBuilder();

        for (int i = 1; i < n; i++)
        {
            sb.Append(vertexNames[i]);
            sb.Append(' ');
            sb.Append(FormatDistance(dist[i]));
            sb.Append('\n');
        }

        File.WriteAllText(outputPath, sb.ToString());
        return 0;
    }

    static string FormatDistance(double value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }
}