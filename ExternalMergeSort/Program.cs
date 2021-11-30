using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ExternalMergeSort
{
    [MemoryDiagnoser]
    public class SortLarge
    {
        [Params(10000, 20000, 30000)]
        public static int LengthFile { get; set; }

        [Params(0.1, 0.2, 0.5, 1)]
        public static double SplitFileRel { get; set; }
        public static string path = "C:\\Users\\andriib\\Desktop\\New folder";

        [IterationSetup]
        public void Setup()
        {
            GenerateFile(path, LengthFile);
        }


        public static void Split(int maxLength, string path)
        {
            int split_num = 1;
            StreamWriter sw = new StreamWriter(
              string.Format($"{path}\\split{split_num}.txt"));
            var file = $"{path}\\unsorted.txt";

            using (StreamReader sr = new StreamReader(file))
            {
                int counter = 0;
                while (!sr.EndOfStream)
                {
                    var l = sr.ReadLine();
                    sw.WriteLine(l);
                    counter++;

                    if (counter >= maxLength)
                    {
                        sw.Close();
                        split_num++;
                        counter = 0;
                        sw = new StreamWriter(
                          string.Format($"{path}\\split{split_num}.txt"));
                    }
                }
            }
            sw.Close();
        }

        

        public static void SortTheChunks(string path1)
        {
            foreach (string path in Directory.GetFiles($"{path1}\\", "split*.txt"))
            {
                string[] contents = File.ReadAllLines(path);
                var sorted = AlgCompl1.Sort.TournamentSort(contents.Select(s => int.Parse(s)));
                string newpath = path.Replace("split", "sorted");
                File.WriteAllLines(newpath, sorted.Select(i => i.ToString()));
                File.Delete(path);
                contents = null;
                GC.Collect();
            }
        }

        public static void MergeTheChunks(string path1)
        {
            string[] paths = Directory.GetFiles($"{path1}\\", "sorted*.txt");
            int chunks = paths.Length;

            StreamReader[] readers = new StreamReader[chunks];
            for (int i = 0; i < chunks; i++)
                readers[i] = new StreamReader(paths[i]);

            var map = new Dictionary<int, int>();
            var minHeap = new AlgCompl1.MinHeap(chunks);
            StreamWriter sw = new StreamWriter($"{path1}\\BigFileSorted.txt");

            for (int i = 0; i < chunks; i++)
            {
                if (!readers[i].EndOfStream)
                {
                    int value = int.Parse(readers[i].ReadLine());
                    minHeap.Add(value);
                    map.Add(value, i);
                }
            }

            while (AllFilesEmpty(readers))
            {
                if (!minHeap.IsEmpty())
                {
                    int minValue = minHeap.Pop();
                    sw.WriteLine(minValue);
                    int minIndex = map[minValue];
                    if (!readers[minIndex].EndOfStream)
                    {
                        int value = int.Parse(readers[minIndex].ReadLine());
                        map.Add(value, minIndex);
                        minHeap.Add(value);
                    }
                }
            }

            while (!minHeap.IsEmpty())
            {
                sw.WriteLine(minHeap.Pop());
            }

            sw.Close();

            for (int i = 0; i < chunks; i++)
            {
                readers[i].Close();
                File.Delete(paths[i]);
            }
        }

        public static bool AllFilesEmpty(StreamReader[] files)
        {
            foreach (var f in files)
            {
                if (!f.EndOfStream)
                {
                    return true;
                }
            }
            return false;
        }

        public static void GenerateFile(string path, int n)
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

            StreamWriter sw = new StreamWriter($"{path}\\unsorted.txt");
            foreach (var r in result)
            { 
                sw.WriteLine(r);
            }
            sw.Close(); 
        }

        [Benchmark]
        public void Sort()
        {
            Split((int)(LengthFile * SplitFileRel), path);
            SortTheChunks(path);
            MergeTheChunks(path);
        }

        static void Main(string[] args)
        {
            /*var path1 = "C:\\Users\\andriib\\Desktop\\New folder";
            int n = 100000;
            GenerateFile(path1, n);
            Split(2000, path1);
            SortTheChunks(path1);
            MergeTheChunks(path1);*/
            var res = BenchmarkRunner.Run<SortLarge>();
        }
    }
}
