using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Columnar_Transposition_Solver
{
	class Program
	{
		public static void Main(string[] args)
		{
			Ngrams ngrams = new Ngrams("english_quadgrams.txt");

			Console.Write("Enter cipher text: ");
			string text = Console.ReadLine().ToLower();
			List<string> texts = new();

			for (int keyLength = 2; 9 >= keyLength; keyLength++)
			{
                List<Tuple<string, Double>> scores = new List<Tuple<string, double>>();
				int length = Convert.ToInt32(Math.Ceiling(text.Length / Convert.ToDouble(keyLength)));

                char[,] TextArray = new char[length, keyLength];
				for (int i = 0; i < text.Length; i++)
				{
					TextArray[i/keyLength,i%keyLength] = text[i];
				}

                for (int i = 0; i < TextArray.GetLength(0); i++)
                {
                    char[] thing = new char[TextArray.GetLength(1)];
                    for (int j = 0; j < TextArray.GetLength(1); j++)
                    {
                        thing[j] = TextArray[i, j];
                    }
                }
                List<char[]> slices = TextArray.Slices();


                int[] ArrayForPermutations = new int[keyLength];
                for (int i = 0; i < ArrayForPermutations.Length; i++)
                {
                    ArrayForPermutations[i] = i;
                }

                IList < IList<int> > permutations = Permute(ArrayForPermutations);

                foreach (IList<int> item in permutations)
                {
                    StringBuilder possibleString = new StringBuilder();
                    foreach (int num in item)
                    {
                        possibleString.Append(slices[num]);
                    }
                    scores.Add(new Tuple<string, Double>(possibleString.ToString(), ngrams.score(possibleString.ToString())));
                }

                if (keyLength == 5)
                {

                }

                Console.WriteLine(scores.MaxBy(t => t.Item2));
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
                // We have one of our possible n! solutions,
                // add it to the list.
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
    }

    static class Ext
    {
        public static List<T[]> Slices<T>(this T[,] array)
        {
            List<T[]> list = new List<T[]>();
            for (int j = 0; j < array.GetLength(1); j++)
            {
                T[] newArray = new T[array.GetLength(0)];
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    newArray[i] = array[i, j];
                }

                list.Add(newArray);
            }
            return list;
        }



    }
}