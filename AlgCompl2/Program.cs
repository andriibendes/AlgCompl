using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


namespace AlgCompl2
{
    public class Node
    {
        public bool IsController = false;
        public Dictionary<char, Node> Edges = new Dictionary<char, Node>();
    }

    [MemoryDiagnoser]
    public class Trie
    {
        public Node Root = new Node();
        public static Trie Trie1 { get; set; }
        public static Trie Trie2 { get; set; }


        public static string Path = "C:\\Users\\andriib\\source\\repos\\AlgCompl\\AlgCompl2\\dictionary.txt";

        [Params(100, 500, 1000, 2000, 5000)]
        public static int WordsCount { get; set; }

        [Params(0.1, 0.2, 0.5, 1)]
        public static double ControllersRel { get; set; }
        public static string[] Controllers { get; set; }
        public static string[] Words { get; set; }

        [IterationSetup]
        public void Setup()
        {
            var lines = System.IO.File.ReadAllLines(Path).ToList();
            Words = lines.OrderBy(s => Guid.NewGuid()).Take(WordsCount).ToArray();
            Controllers = Words.OrderBy(s => Guid.NewGuid()).Take((int)(WordsCount * ControllersRel)).ToArray();
            Trie2 = new Trie();
            Trie2.Build(Controllers);
        }

        [Benchmark]
        public void BuildTrie()
        {
            Trie1 = new Trie();
            Trie1.Build(Controllers);
        }

        [Benchmark]
        public void Req()
        {
            Trie2.Request(Words);
        }

        public void Request(string[] words)
        {
            foreach (var w in words)
            {
                TryGetController(Trie2, w, out _);
            }
        }

        public void Build(string[] controllers)
        {
            foreach (var controller in controllers)
            {
                var node = Root;
                for (int i = 0; i < controller.Length; i++)
                {
                    var letter = controller[i];

                    if (!node.Edges.TryGetValue(letter, out var next))
                    {
                        next = new Node();

                        if (i == controller.Length - 1)
                        {
                            next.IsController = true;
                        }

                        node.Edges.Add(letter, next);
                    }
                    node = next;
                }
            }
        }

        public bool TryGetController(Trie trie,string word, out string controller)
        {
            var node = trie.Root;
            var result = new StringBuilder();
            controller = "";

            for (int i = 0; i < word.Length; i++)
            {
                var letter = word[i];

                if (node.Edges.TryGetValue(letter, out var next))
                {
                    result.Append(letter);

                    if (next.IsController)
                    {
                        controller = result.ToString();
                    }

                    node = next;
                }
                else
                {
                    break; 
                }
            }

            return !string.IsNullOrEmpty(controller);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var res = BenchmarkRunner.Run<Trie>();

            /*var benchmark = new List<KeyValuePair<int, int>>();

            benchmark.Add(new KeyValuePair<int, int>(100, 100));
            benchmark.Add(new KeyValuePair<int, int>(100, 500));
            benchmark.Add(new KeyValuePair<int, int>(100, 1000));
            benchmark.Add(new KeyValuePair<int, int>(100, 200000));
            benchmark.Add(new KeyValuePair<int, int>(100, 349000));

            benchmark.Add(new KeyValuePair<int, int>(500, 100));
            benchmark.Add(new KeyValuePair<int, int>(500, 500));
            benchmark.Add(new KeyValuePair<int, int>(500, 1000));
            benchmark.Add(new KeyValuePair<int, int>(500, 200000));
            benchmark.Add(new KeyValuePair<int, int>(500, 349000));

            benchmark.Add(new KeyValuePair<int, int>(1000, 100));
            benchmark.Add(new KeyValuePair<int, int>(1000, 500));
            benchmark.Add(new KeyValuePair<int, int>(1000, 1000));
            benchmark.Add(new KeyValuePair<int, int>(1000, 200000));
            benchmark.Add(new KeyValuePair<int, int>(1000, 349000));

            benchmark.Add(new KeyValuePair<int, int>(10000, 100));
            benchmark.Add(new KeyValuePair<int, int>(10000, 500));
            benchmark.Add(new KeyValuePair<int, int>(10000, 1000));
            benchmark.Add(new KeyValuePair<int, int>(10000, 200000));
            benchmark.Add(new KeyValuePair<int, int>(10000, 349000));

            benchmark.Add(new KeyValuePair<int, int>(200000, 100));
            benchmark.Add(new KeyValuePair<int, int>(200000, 500));
            benchmark.Add(new KeyValuePair<int, int>(200000, 1000));
            benchmark.Add(new KeyValuePair<int, int>(200000, 200000));
            benchmark.Add(new KeyValuePair<int, int>(200000, 349000));

            benchmark.Add(new KeyValuePair<int, int>(349000, 100));
            benchmark.Add(new KeyValuePair<int, int>(349000, 500));
            benchmark.Add(new KeyValuePair<int, int>(349000, 1000));
            benchmark.Add(new KeyValuePair<int, int>(349000, 200000));
            benchmark.Add(new KeyValuePair<int, int>(349000, 349000));


            var lines = System.IO.File.ReadAllLines(@"C:\Users\andriib\source\repos\AlgCompl\AlgCompl2\dictionary.txt").ToList();

            foreach (var b in benchmark)
            {
                var randStrings = lines.OrderBy(s => Guid.NewGuid());

                var randConts = randStrings.Take(b.Key);
                var randWords = randStrings.Skip(b.Key).Take(b.Value);

                var time1 = new Stopwatch();
                time1.Start();

                var trie = new Trie(randConts);

                time1.Stop();

                var init = time1.ElapsedMilliseconds;

                var time2 = new Stopwatch();
                time2.Start();

                foreach (var w in randWords)
                {
                    trie.TryGetController(w, out _);
                }

                time2.Stop();

                var search = time2.ElapsedMilliseconds;

                Console.WriteLine($"FOR {b.Key} CONTROLLERS AND {b.Value} WORDS ELAPSED {init} INIT MS AND {search} SEARCH MS");
            */
        }
    }
}
