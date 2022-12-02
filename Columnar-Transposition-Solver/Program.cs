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
        public static void Main(string[] args)
        {

            Console.Write("Enter cipher text: ");
            string text = Console.ReadLine().ToLower();
            List<string> texts = new();

            for (int keyLength = 2; 9 >= keyLength; keyLength++)
            {
                object arg = new object[2] { text, keyLength };
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
            Ngrams ngrams = new Ngrams("english_quadgrams.txt");
            List<Tuple<string, Double>> scores = new List<Tuple<string, double>>();
            int length = Convert.ToInt32(Math.Ceiling(text.Length / Convert.ToDouble(keyLength)));

            int[] ArrayForPermutations = new int[keyLength];
            for (int i = 0; i < ArrayForPermutations.Length; i++) { ArrayForPermutations[i] = i; }

            IList<IList<int>> permutations = Permute(ArrayForPermutations);

            foreach (IList<int> permutation in permutations)
            {
                StringBuilder possibleString = new StringBuilder();
                for (int firstD = 0; firstD < text.Length; firstD+=keyLength)
                {
                    for (int secondD = 0; secondD < keyLength; secondD++)
                    {
                        if (firstD + permutation[secondD] >= text.Length){ break; }
                        possibleString.Append(text[firstD+permutation[secondD]]);
                    }
                }
                scores.Add(new Tuple<string, Double>(possibleString.ToString(), ngrams.score(possibleString.ToString())));
            }
            Console.WriteLine(scores.MaxBy(t => t.Item2));
        }
    }
}