using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AlgCompl1
{
    public class Sort
    {
        public static int MinHeapSize { get; set; }

        [Params(100, 300, 500, 1000, 3000, 5000, 7000, 10000)]
        public int Size { get; set; }

        [Params(0.333, 0.5, 0.7, 1)]
        public double SizeRelation { get; set; }

        static int[] Unsorted;
 
        [IterationSetup]
        public void Setup()
        {
            MinHeapSize = (int)(Size * SizeRelation);
            Unsorted = Shuffle(Size);
        }

        [Benchmark]
        public List<int> TournamentSort()
        {
            return Tour(Unsorted.ToList());
        }

        public static List<int> TournamentSort(IEnumerable<int> data)
        {
            if (MinHeapSize == 0)
            {
                MinHeapSize = data.Count() / 2;
            }
            if (data.Count() <= MinHeapSize)
            {
                return OptimalTourneySort(data.ToArray());
            }

            return Tour(data.ToList());
        }

        private static List<int> Tour(List<int> list)
        {
            var size = list.Count;
            var minHeap = new MinHeap(MinHeapSize);

            while (minHeap.GetSize() < MinHeapSize)
            {
                minHeap.Add(list.First());
                list.RemoveAt(0);
            }

            var winners = new List<int>();
            var losers = new List<int>();

            while (list.Any())
            {
                if (!winners.Any())
                {
                    winners.Add(minHeap.Pop());
                }

                if (list.First() > winners.Last())
                {
                    minHeap.Add(list.First());
                    list.RemoveAt(0);
                }
                else
                {
                    losers.Add(list.First());
                    list.RemoveAt(0);
                }

                if (!minHeap.IsEmpty())
                {
                    winners.Add(minHeap.Pop());
                }
            }

            while (!minHeap.IsEmpty())
            {
                winners.Add(minHeap.Pop());
            }

            if (!losers.Any())
            {
                return winners;
            }

            losers.AddRange(winners);

            while (losers.Count > size)
            {
                losers.RemoveAt(losers.Count - 1);
            }

            return Tour(losers);
        }

        public static int[] Shuffle(int n)
        {
            var random = new Random();
            var result = new int[n];
            for (var i = 0; i < n; i++)
            {
                var j = random.Next(0, i + 1);
                if (i != j)
                {
                    result[i] = result[j];
                }
                result[j] = i;
            }
            return result;
        }

        private static List<int> OptimalTourneySort(int[] array)
        {
            var sorted = new List<int>();
            var minHeap = new MinHeap(array.Length);

            foreach (var el in array)
            {
                minHeap.Add(el);
            }

            while (!minHeap.IsEmpty())
            {
                sorted.Add(minHeap.Pop());
            }

            return sorted;
        }
    }

    class Program
    {
        static void Main(string[] args)
        { /*
            var sort = new Sort();
            sort.Size = 1000;
            sort.Setup();
            var sorted = sort.TournamentSort();
            Console.WriteLine(string.Join(",", sorted));*/
            var res = BenchmarkRunner.Run<Sort>();
        }
    }
}
