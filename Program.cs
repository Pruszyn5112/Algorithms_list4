class Program
{
    static void Main()
    {
        string[] lines = File.ReadAllLines("In0402.txt");
        int n = int.Parse(lines[0]); // liczba wierzchołków

        // Inicjalizacja grafu z listą incydencji i wagami
        List<int>[] adjacencyList = new List<int>[n];
        List<int>[] weights = new List<int>[n];
        for (int i = 0; i < n; i++)
        {
            adjacencyList[i] = new List<int>();
            weights[i] = new List<int>();
        }
        // Wczytanie listi incydencji
        for (int i = 1; i < lines.Length; i++)
        {
            // Czy linia jest pusta
            /* if (string.IsNullOrWhiteSpace(lines[i]))
             {
                 // W przeciwnym razie, przejdź do następnej linii
                 continue;
             }*/

            string[] tokens = lines[i].Split(' ');

            // Czy tablica ma odpowiednią długość
            if (tokens.Length % 2 != 0)
            {
                Console.WriteLine("Błędny format danych w linii " + i);
                continue;
            }

            int vertex = int.Parse(tokens[0]);

            // Czy numer wierzchołka nie przekracza liczby wierzchołków grafu
            if (vertex < 1 || vertex > n)
            {
                Console.WriteLine("Numer wierzchołka przekracza zakres w linii " + i);
                continue;
            }

            for (int j = 1; j < tokens.Length; j += 2)
            {
                // Czy indeks tokens[j + 1] mieści się w zakresie tablicy
                if (j + 1 < tokens.Length)
                {
                    int neighbor = int.Parse(tokens[j]);
                    int weight = int.Parse(tokens[j + 1]);

                    // Dodaj krawędź do listy incydencji
                    adjacencyList[vertex].Add(neighbor);
                    weights[vertex].Add(weight);
                }
                else
                {
                    break;
                }
            }



        }
        // Wywołanie algorytmu Johnsona
        JohnsonAlgorithm johnson = new JohnsonAlgorithm(adjacencyList, weights, n);
        johnson.Run();

        using (StreamWriter sw = new StreamWriter("Out0402.txt"))
        {
            // W pierwszej linii pliku umieść wartości tablicy δ[]
            // sw.WriteLine("Wartości tablicy δ[]:");
            // sw.WriteLine(string.Join(" ", johnson.DistancesFromSource));


            sw.WriteLine("Tablica list incydencji dla G':");
            for (int i = 0; i < n; i++)
            {
                sw.WriteLine($"{i}: {string.Join(" ", johnson.AdjacencyList[i])}");
            }


            // Uzupelnienie macierzy zerami
            for (int i = 0; i < n; i++)
            {
                if (johnson.ShortestPaths[i + 1].Count < n)
                {
                    int zerosToAdd = n - johnson.ShortestPaths[i + 1].Count;
                    johnson.ShortestPaths[i + 1].AddRange(new int[zerosToAdd]);
                }

                if (johnson.OriginalPaths[i + 1].Count < n)
                {
                    int zerosToAdd = n - johnson.OriginalPaths[i + 1].Count;
                    johnson.OriginalPaths[i + 1].AddRange(new int[zerosToAdd]);
                }
            }

            // Wypisanie wektorów δ^[s] oraz D[s] w postaci macierzy poprzedników
            sw.WriteLine("Macierz poprzedników:");
            for (int s = 1; s <= n; s++)
            {
                sw.WriteLine($"Delta^[{s}]:\n{MatrixToString(johnson.ShortestPaths[s])}");
                sw.WriteLine($"D[{s}]:\n{MatrixToString(johnson.OriginalPaths[s])}");
            }
        }
        static string MatrixToString(List<int> vector)
        {
            int n = vector.Count;
            int[,] matrix = new int[n, n];
            for (int i = 0; i < n; i++)
            {
                matrix[i, i] = vector[i];
            }

            string result = "";
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result += matrix[i, j] + " ";
                }
                result = result.TrimEnd() + Environment.NewLine;
            }

            return result.Trim();
        }
    }
}

class JohnsonAlgorithm
{
    public List<int>[] AdjacencyList { get; private set; }
    public List<int>[] Weights { get; private set; }
    public int[] DistancesFromSource { get; private set; }
    public List<int>[] ShortestPaths { get; private set; }
    public List<int>[] OriginalPaths { get; private set; }

    private int n;

    public JohnsonAlgorithm(List<int>[] adjacencyList, List<int>[] weights, int n)
    {
        this.AdjacencyList = adjacencyList;
        this.Weights = weights;
        this.n = n;

        // Inicjalizacja pozostałych struktur danych
        DistancesFromSource = new int[n + 1];
        ShortestPaths = new List<int>[n + 1];
        OriginalPaths = new List<int>[n + 1];

        for (int i = 0; i < n + 1; i++)
        {
            ShortestPaths[i] = new List<int>();
            OriginalPaths[i] = new List<int>();
        }
    }

    public void Run()
    {
        // Nowy wierzchołek
        int sourceVertex = n;

        for (int i = 0; i < n; i++)
        {
            AdjacencyList[i].Add(sourceVertex - 1);
            Weights[i].Add(0);
        }

        // Wykonanie algorytmu Bellmana-Forda
        if (BellmanFord(sourceVertex))
        {
            // Korekta wag krawędzi
            CorrectEdgeWeights();

            // Wykonanie algorytmu Dijkstry dla każdego wierzchołka
            for (int s = 0; s < n; s++)
            {
                Dijkstra(s);
            }

            // Odwrócenie korekty wag krawędzi
            UndoCorrectEdgeWeights();
        }
    }

    private bool BellmanFord(int source)
    {
        // Inicjalizacja tablicy odległości
        DistancesFromSource = new int[n + 1];

        for (int i = 0; i < n + 1; i++)
        {
            DistancesFromSource[i] = int.MaxValue;
        }
        DistancesFromSource[source] = 0;

        // relaksacja krawędzi
        for (int i = 0; i < n; i++)
        {
            for (int u = 0; u < n; u++)
            {
                for (int vIndex = 0; vIndex < AdjacencyList[u].Count; vIndex++)
                {
                    int v = AdjacencyList[u][vIndex];
                    int weight = Weights[u][vIndex];

                    if (DistancesFromSource[u] != int.MaxValue && DistancesFromSource[u] + weight < DistancesFromSource[v])
                    {
                        DistancesFromSource[v] = DistancesFromSource[u] + weight;
                    }
                }
            }
        }

        // Obecność cyklu ujemnego
        for (int u = 0; u < n; u++)
        {
            for (int vIndex = 0; vIndex < AdjacencyList[u].Count; vIndex++)
            {
                int v = AdjacencyList[u][vIndex];
                int weight = Weights[u][vIndex];

                if (DistancesFromSource[u] != int.MaxValue && DistancesFromSource[u] + weight < DistancesFromSource[v])
                {
                    Console.WriteLine("Graf zawiera cykl ujemny!");
                    return false;
                }
            }
        }

        return true;
    }

    private void CorrectEdgeWeights()
    {
        for (int u = 0; u < n; u++)
        {
            for (int vIndex = 0; vIndex < AdjacencyList[u].Count; vIndex++)
            {
                int neighbor = AdjacencyList[u][vIndex];

                // Sprawdź, czy sąsiad mieści się w zakresie tablicy
                if (neighbor >= 0 && neighbor < n)
                {
                    Weights[u][vIndex] = Weights[u][vIndex] + DistancesFromSource[u] - DistancesFromSource[neighbor];
                }
            }
        }
    }

    private void UndoCorrectEdgeWeights()
    {
        for (int u = 0; u < n; u++)
        {
            for (int vIndex = 0; vIndex < AdjacencyList[u].Count; vIndex++)
            {
                int neighbor = AdjacencyList[u][vIndex];

                // Sprawdź, czy sąsiad mieści się w zakresie tablicy
                if (neighbor >= 0 && neighbor < n)
                {
                    Weights[u][vIndex] = Weights[u][vIndex] - (DistancesFromSource[u] - DistancesFromSource[neighbor]);
                }
            }
        }
    }


    private void Dijkstra(int source)
    {
        // Inicjalizacja tablicy odległości
        int[] distances = new int[n];
        for (int i = 0; i < n; i++)
        {
            distances[i] = int.MaxValue;
        }
        distances[source] = 0;

        // Inicjalizacja kolejki priorytetowej dla wierzchołków
        PriorityQueue<int> queue = new PriorityQueue<int>();
        for (int i = 0; i < n; i++)
        {
            queue.Enqueue(i, distances[i]);
        }

        // Wykonanie algorytmu Dijkstry
        while (queue.Count > 0)
        {
            int u;
            try
            {
                u = queue.Dequeue();
            }
            catch (InvalidOperationException)
            {
                // Kolejka jest pusta
                break;
            }

            for (int vIndex = 0; vIndex < AdjacencyList[u].Count; vIndex++)
            {
                int v = AdjacencyList[u][vIndex];

                // Czy sąsiad mieści się w zakresie tablicy
                if (v >= 0 && v < n)
                {
                    int weight = Weights[u][vIndex];

                    // Warunek relaksacji
                    if (distances[u] != int.MaxValue && distances[u] + weight < distances[v])
                    {
                        // Relaksacja krawędzi
                        distances[v] = distances[u] + weight;
                        queue.Enqueue(v, distances[v]);
                        // Zapamiętanei poprzednika dla rekonstrukcji ścieżki
                        ShortestPaths[source].Add(v);
                        OriginalPaths[source].Add(u);
                    }
                }
            }
        }
    }


}

// Klasa implementująca prostą kolejkę priorytetową
class PriorityQueue<T>
{
    private SortedDictionary<int, Queue<T>> _dictionary = new SortedDictionary<int, Queue<T>>();

    public int Count
    {
        get
        {
            return _dictionary.Values.Sum(queue => queue.Count);
        }
    }

    public void Enqueue(T item, int priority)
    {
        if (!_dictionary.ContainsKey(priority))
        {
            _dictionary[priority] = new Queue<T>();
        }
        _dictionary[priority].Enqueue(item);
    }

    public T Dequeue()
    {
        if (_dictionary.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        var queue = _dictionary.First();
        var item = queue.Value.Dequeue();

        if (queue.Value.Count == 0)
        {
            _dictionary.Remove(queue.Key);
        }

        return item;
    }
}
