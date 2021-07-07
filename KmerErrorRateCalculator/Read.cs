using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmerErrorRateCalculator
{
    class Read
    {
        public string ReadName { get; init; }
        public int Length { get; init; }
        public char[] Gene { get; init; }
        public char[] QualityChars { get; init; }
        public int[] Qualities { get; init; }
        public double QualityAverage { get; init; }
        public Read(string readName,int length,char[] gene,char[] qualityChars)
        {
            ReadName = readName;
            Length = length;
            Gene = gene;
            QualityChars = qualityChars;
            Qualities = new int[QualityChars.Length];
            int qualitySum = 0;
            for(int i = 0; i < QualityChars.Length; i++)
            {
                var quality = ((int)QualityChars[i]) - 33;
                Qualities[i] = quality;
                qualitySum += quality;
            }
            QualityAverage = (double)(qualitySum / Qualities.Length); 
        }
    }
}
