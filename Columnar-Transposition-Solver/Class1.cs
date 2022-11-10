using System;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;

namespace Columnar_Transposition_Solver
{
    internal class Ngrams
    {
        private Dictionary<string, double> ngrams_;
        private int l_;
        private double sum_;
        private double floor_;

        public Ngrams(string filename)
        {
            ngrams_ = new Dictionary<string, double>();
            string[] lines = File.ReadAllLines(filename);

            foreach (string item in lines)
            {
                string[] keyPair = item.Split(" ");
                double value = double.Parse(keyPair[1]);
                string key = keyPair[0];

                ngrams_[key] = value;
            }

            l_ = lines[0].Split(" ")[0].Length;
            sum_ = ngrams_.Values.ToArray().Sum();

            foreach (string key in ngrams_.Keys.ToArray())
            {
                ngrams_[key] = Math.Log10(ngrams_[key] / sum_);
            }

            floor_ = Math.Log10(0.01 / sum_);
        }

        public double score(string text)
        {
            double score = 0;
            for (int i = 0; i < text.Length - l_ + 1; i++)
            {
                if (ngrams_.ContainsKey(text.Substring(i, l_).ToUpper())) { score += ngrams_[text.Substring(i, l_).ToUpper()]; }
                else { score += floor_; }
            }

            return score;
        }
    }
}

