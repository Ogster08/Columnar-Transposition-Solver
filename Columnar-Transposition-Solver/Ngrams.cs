using System;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;

namespace Columnar_Transposition_Solver
{
    internal class Ngrams
    {
        private double[] scoresUsingInt_;
        private int l_;
        private double sum_ = 0;
        private double floor_;

        public Ngrams(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            l_ = lines[0].Split(" ")[0].Length;

            scoresUsingInt_ = new double[(int)Math.Pow(32, l_)];
            Array.Fill(scoresUsingInt_, 0.01);

            foreach (string item in lines)
            {
                string[] keyPair = item.Split(" ");
                double value = double.Parse(keyPair[1]);
                string key = keyPair[0];

                scoresUsingInt_[EncodeUpper(key)] = value;
                sum_ += value;
            }

            for (int i = 0; i < scoresUsingInt_.Length; i++)
            {
                scoresUsingInt_[i] = Math.Log10(scoresUsingInt_[i] / sum_);
            }

            floor_ = Math.Log10(0.01 / sum_);
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

        public double score(ReadOnlySpan<char> text)
        {
            double score = 0;

            int encoded = EncodeLower(text.Slice(0, l_));
            score += scoresUsingInt_[encoded];

            int mask = (1 << (5 * (l_ - 1))) - 1;

            for (int i = l_; i < text.Length - l_ + 1; i++)
            {
                encoded = ((encoded & mask) << 5) | (text[i] - 'a');
                score += scoresUsingInt_[encoded];
            }
            return score;
        }

    }
}

