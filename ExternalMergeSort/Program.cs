using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ExternalMergeSort
{
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

                    // If the file is big, then make a new split,
                    // however if this was the last line then don't bother
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
                // Read all lines into an array
                string[] contents = File.ReadAllLines(path);
                // Sort the in-memory array
                var sorted = AlgCompl1.Sort.TournamentSort(contents.Select(s => int.Parse(s)));
                // Create the 'sorted' filename
                string newpath = path.Replace("split", "sorted");
                // Write it
                File.WriteAllLines(newpath, sorted.Select(i => i.ToString()));
                // Delete the unsorted chunk
                File.Delete(path);
                // Free the in-memory sorted array
                contents = null;
                GC.Collect();
            }
        }

        public static void MergeTheChunks(string path1)
        {
            string[] paths = Directory.GetFiles($"{path1}\\", "sorted*.txt");
            int chunks = paths.Length; // Number of chunks

            // Open the files
            StreamReader[] readers = new StreamReader[chunks];
            for (int i = 0; i < chunks; i++)
                readers[i] = new StreamReader(paths[i]);

            // Make the queues
            Queue<int>[] queues = new Queue<int>[chunks];
            for (int i = 0; i < chunks; i++)
                queues[i] = new Queue<int>();

            // Load the queues
            for (int i = 0; i < chunks; i++)
                LoadQueue(queues[i], readers[i]);

            // Merge!
            StreamWriter sw = new StreamWriter($"{path1}\\BigFileSorted.txt");
            bool done = false;
            int lowest_index, j;
            int lowest_value;
            while (!done)
            {
                // Find the chunk with the lowest value
                lowest_index = -1;
                lowest_value = -1;
                for (j = 0; j < chunks; j++)
                {
                    if (queues[j] != null && queues[j].Count() > 0)
                    {
                        if (lowest_index < 0 || queues[j].Peek() < lowest_value)
                        {
                            lowest_index = j;
                            lowest_value = queues[j].Peek();
                        }
                    }
                }

                // Was nothing found in any queue? We must be done then.
                if (lowest_index == -1) { done = true; break; }

                // Output it
                sw.WriteLine(lowest_value);

                // Remove from queue
                queues[lowest_index].Dequeue();
                // Have we emptied the queue? Top it up
                if (queues[lowest_index].Count == 0)
                {
                    LoadQueue(queues[lowest_index],
                      readers[lowest_index]);
                    // Was there nothing left to read?
                    if (queues[lowest_index].Count == 0)
                    {
                        queues[lowest_index] = null;
                    }
                }
            }
            sw.Close();

            // Close and delete the files
            for (int i = 0; i < chunks; i++)
            {
                readers[i].Close();
                File.Delete(paths[i]);
            }
        }

        public static void LoadQueue(Queue<int> queue,
          StreamReader file)
        {
            while (file.Peek() >= 0)
            {
                queue.Enqueue(int.Parse(file.ReadLine()));
            }
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
