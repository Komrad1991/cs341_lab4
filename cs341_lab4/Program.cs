using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Edge
{
    public string U;
    public string V;
    public double W;
    public int Index;

    public Edge(string u, string v, double w, int index)
    {
        U = u;
        V = v;
        W = w;
        Index = index;
    }
}

class DSU
{
    private int[] parent;
    private int[] rank;

    public DSU(int n)
    {
        parent = new int[n];
        rank = new int[n];

        for (int i = 0; i < n; i++)
            parent[i] = i;
    }

    public int Find(int x)
    {
        if (parent[x] != x)
            parent[x] = Find(parent[x]);

        return parent[x];
    }

    public bool Union(int a, int b)
    {
        int ra = Find(a);
        int rb = Find(b);

        if (ra == rb)
            return false;

        if (rank[ra] < rank[rb])
        {
            int tmp = ra;
            ra = rb;
            rb = tmp;
        }

        parent[rb] = ra;

        if (rank[ra] == rank[rb])
            rank[ra]++;

        return true;
    }
}

class Program
{
    static void Main(string[] args)
    {
        string inputPath = args.Length > 0 ? args[0] : "input.txt";
        string outputPath = args.Length > 1 ? args[1] : "output.txt";

        var lines = File.ReadAllLines(inputPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        if (lines.Length == 0)
            return;

        // Первая строка — имена вершин
        string[] vertexNames = lines[0].Split(
            new[] { ' ', '\t' },
            StringSplitOptions.RemoveEmptyEntries
        );

        var vertexId = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (string name in vertexNames)
        {
            if (!vertexId.ContainsKey(name))
                vertexId[name] = vertexId.Count;
        }

        var edges = new List<Edge>();
        int edgeIndex = 0;

        // Чтение рёбер
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(
                new[] { ' ', '\t' },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (parts.Length < 3)
                continue;

            string u = parts[0];
            string v = parts[1];
            double w = double.Parse(parts[2], CultureInfo.InvariantCulture);

            if (!vertexId.ContainsKey(u))
                vertexId[u] = vertexId.Count;

            if (!vertexId.ContainsKey(v))
                vertexId[v] = vertexId.Count;

            edges.Add(new Edge(u, v, w, edgeIndex++));
        }

        // Сортировка рёбер по весу.
        // При равенстве весов выбран дополнительный порядок,
        // чтобы результат был детерминированным.
        edges.Sort((e1, e2) =>
        {
            int cmp = e1.W.CompareTo(e2.W);
            if (cmp != 0)
                return cmp;

            cmp = string.CompareOrdinal(e2.U, e1.U);
            if (cmp != 0)
                return cmp;

            cmp = string.CompareOrdinal(e2.V, e1.V);
            if (cmp != 0)
                return cmp;

            return e1.Index.CompareTo(e2.Index);
        });

        var dsu = new DSU(vertexId.Count);
        var mstEdges = new List<Edge>();

        double totalWeight = 0.0;

        foreach (Edge edge in edges)
        {
            int u = vertexId[edge.U];
            int v = vertexId[edge.V];

            if (dsu.Union(u, v))
            {
                mstEdges.Add(edge);
                totalWeight += edge.W;
            }
        }

        // Сортировка рёбер MST для вывода
        mstEdges.Sort((e1, e2) =>
        {
            int cmp = string.CompareOrdinal(e1.U, e2.U);
            if (cmp != 0)
                return cmp;

            return string.CompareOrdinal(e1.V, e2.V);
        });

        using var writer = new StreamWriter(outputPath);

        writer.WriteLine(
            totalWeight.ToString("0.0###############", CultureInfo.InvariantCulture)
        );

        foreach (Edge edge in mstEdges)
        {
            writer.WriteLine($"{edge.U} {edge.V}");
        }
    }
}