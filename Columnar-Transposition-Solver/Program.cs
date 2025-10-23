using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

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

            // scores for each quadgram
            double[] scoresUsingInt = InitiliseNgramsScorer();

            // run each key length on a different thread
            /*for (int keyLength = 2; 9 >= keyLength; keyLength++)
            {
                object arg = new object[3] { text, keyLength, scoresUsingInt };
                var T = new Thread(solve);
                T.Start(arg);
            }*/

            Parallel.For(2, 10, keyLength =>
            {
                solve(new object[] { text, keyLength, scoresUsingInt });
            });

            Console.ReadLine();
        }

        static void solve(Object args)
        {
            // getting passed arguements
            Array argArray = (Array)args;
            string text = (string)argArray.GetValue(0);
            int keyLength = (int)argArray.GetValue(1);
            double[] scoresUsingInt = (double[])argArray.GetValue(2);

            int textLength = text.Length;
            int rowCount = (textLength + keyLength - 1) / keyLength;

            // precomputing row lengths and start indices for flattening the rows and columns later
            int[] rowLengths = new int[rowCount];
            int[] rowStarts = new int[rowCount];
            int[] flattenedPermutedTextIndices = new int[textLength];

            for (int row = 0; row < rowCount; row++)
            {
                rowStarts[row] = row * keyLength;
                rowLengths[row] = Math.Min(keyLength, textLength - rowStarts[row]);
            }

            // setting variables for the scoring
            double bestScore = -999999;
            int[] bestPermutation = new int[keyLength];
            int mask = (1 << 15) - 1;

            // setting initial permutation
            int[] permutation = new int[keyLength];
            for (int i = 0; i < keyLength; i++) { permutation[i] = i; }

            // Tty every permutation of the key
            GetPermutations(permutation, keyLength, (permute) =>
            {
                // Scoring permutation

                // gets the indeces in the cipher text for each position in the decryption
                int index = 0;
                for (int row = 0; row < rowCount; row++)
                {
                    int startIndex = rowStarts[row];
                    int rowSize = rowLengths[row];
                    for (int col = 0; col < rowSize; col++)
                    {
                        int colPerm = permutation[col];
                        if (colPerm >= rowSize) continue;
                        flattenedPermutedTextIndices[index++] = startIndex + colPerm;
                    }
                }

                double score = ScorePermutation(flattenedPermutedTextIndices[..index], text, scoresUsingInt, mask);

                // save the score and key if it is the best so far
                if (score > bestScore)
                {
                    bestScore = score;
                    Array.Copy(permutation, bestPermutation, keyLength);
                }
            });

            // decrypt cipher text with the best key
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < text.Length; row += keyLength)
            {
                for (int col = 0; col < keyLength; col++)
                {
                    if (row + bestPermutation[col] >= text.Length) { continue; }
                    sb.Append(text[row + bestPermutation[col]]);

                }
            }

            // output best possible decryption
            Console.WriteLine(string.Join(",", bestPermutation) + "  keyLength: " + keyLength.ToString() + " score: " + bestScore + " " + sb.ToString());
        }

        private static void GetPermutations(int[] Permute, int n, Action<int[]> action)
        {
            if (n == 1)
            {
                action(Permute); 
                return;
            }

            GetPermutations(Permute, n - 1, action);
            for (int i = 0; i < n - 1; i++)
            {
                if (n % 2 == 0) (Permute[i], Permute[n - 1]) = (Permute[n - 1], Permute[i]);
                else (Permute[0], Permute[n - 1]) = (Permute[n - 1], Permute[0]);
                GetPermutations(Permute, n - 1, action);
            }
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

        // table to convert chars to integers for scoring
        static readonly byte[] CharToIntTable = CreateCharToIntTable();
        private static byte[] CreateCharToIntTable()
        {
            byte[] table = new byte[256];
            for (char c = 'a'; c <= 'z'; c++) { table[c] = (byte)(c - 'a'); }
            return table;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double ScorePermutation(ReadOnlySpan<int> flatIndex, string text, double[] scores, int mask)
        {
            // initial score for the first quadgram
            double score = 0;
            int encoded = 0;
            for (int i = 0; i < 4; i++)
            {
                encoded = (encoded << 5) | CharToIntTable[text[flatIndex[i]]];
            }

            // using rolling encoding to only have to change 1 char each iteration
            for (int i = 4; i < flatIndex.Length; i++)
            {
                encoded = ((encoded & mask) << 5) | (CharToIntTable[text[flatIndex[i]]]);
                score += scores[encoded];
            }

            return score;
        }
    }
}