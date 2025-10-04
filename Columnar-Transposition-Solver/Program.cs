using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;

namespace Columnar_Transposition_Solver
{
    class Program
    {
        private int l_;
        private double[] scoresUsingInt_;

        public static void Main(string[] args)
        {
            Console.Write("Enter cipher text: ");
            string text = Console.ReadLine().ToLower();
            double[] scoresUsingInt = InitiliseNgramsScorer();


            for (int keyLength = 2; 9 >= keyLength; keyLength++)
            {
                object arg = new object[3] { text, keyLength, scoresUsingInt };
                var T = new Thread(solve);
                T.Start(arg);
            }
        }

        static IList<IList<int>> Permute(int[] nums)
        {
            var list = new List<IList<int>>();
            return DoPermute(nums, 0, nums.Length - 1, list);
        }

        static IList<IList<int>> DoPermute(int[] nums, int start, int end, IList<IList<int>> list)
        {
            if (start == end)
            {
                list.Add(new List<int>(nums));
            }
            else
            {
                for (var i = start; i <= end; i++)
                {
                    Swap(ref nums[start], ref nums[i]);
                    DoPermute(nums, start + 1, end, list);
                    Swap(ref nums[start], ref nums[i]);
                }
            }

            return list;
        }

        static void Swap(ref int a, ref int b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        static void solve(Object args)
        {
            Array argArray = (Array)args;
            string text = (string)argArray.GetValue(0);
            int keyLength = (int)argArray.GetValue(1);
            double[] scoresUsingInt = (double[])argArray.GetValue(2);
            List<Tuple<IList<int>, Double>> scores = new List<Tuple<IList<int>, double>>();
            int length = Convert.ToInt32(Math.Ceiling(text.Length / Convert.ToDouble(keyLength)));

            int[] ArrayForPermutations = new int[keyLength];
            for (int i = 0; i < ArrayForPermutations.Length; i++) { ArrayForPermutations[i] = i; }

            IList<IList<int>> permutations = Permute(ArrayForPermutations);

            foreach (IList<int> permutation in permutations)
            {
                char[] buffer = new char[4];
                bool bufferFull = false;
                double score = 0;
                int encoded = 0;
                int mask = (1 << 15) - 1;

                for (int firstD = 0; firstD < text.Length; firstD+=keyLength)
                {
                    for (int secondD = 0; secondD < keyLength; secondD++)
                    {
                        if (firstD + permutation[secondD] >= text.Length){ break; }
                        if (!bufferFull)
                        {
                            int index = firstD + permutation[secondD];
                            char c = text[index];
                            buffer[firstD + secondD] = text[firstD + permutation[secondD]];
                            if (firstD + secondD >= 3)
                            {
                                bufferFull = true;
                                encoded = EncodeLower(buffer);
                                score += scoresUsingInt[encoded];
                            }
                        }
                        else
                        {
                            encoded = ((encoded & mask) << 5) | (text[firstD + permutation[secondD]] - 'a');
                            score += scoresUsingInt[encoded];
                        }
                    }
                }
                scores.Add(new Tuple<IList<int>, Double>(permutation, score));
            }
            Console.WriteLine(scores.MaxBy(t => t.Item2) + " " + keyLength.ToString());
        }

        private static int EncodeLower(ReadOnlySpan<char> gram)
        {
            int value = 0;
            foreach (char c in gram)
            {
                value = (value << 5) | (c - 'a');
            }
            return value;
        }

        private static int EncodeUpper(ReadOnlySpan<char> gram)
        {
            int value = 0;
            foreach (char c in gram)
            {
                value = (value << 5) | (c - 'A');
            }
            return value;
        }

        private static double[] InitiliseNgramsScorer()
        {
            double sum_ = 0;
            string[] lines = File.ReadAllLines("english_quadgrams.txt");

            double[] scoresUsingInt = new double[(int)Math.Pow(32, 4)];
            Array.Fill(scoresUsingInt, 0.01);

            foreach (string item in lines)
            {
                string[] keyPair = item.Split(" ");
                double value = double.Parse(keyPair[1]);
                string key = keyPair[0];

                scoresUsingInt[EncodeUpper(key)] = value;
                sum_ += value;
            }

            for (int i = 0; i < scoresUsingInt.Length; i++)
            {
                scoresUsingInt[i] = Math.Log10(scoresUsingInt[i] / sum_);
            }

            return scoresUsingInt;
        }
    }
}