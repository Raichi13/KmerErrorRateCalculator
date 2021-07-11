using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace KmerErrorRateCalculator
{
    class Program
    {
        static char[] DnaBase = new char[4] { 'A', 'T', 'G', 'C' };
        static char[] RnaBase = new char[4] { 'A', 'G', 'C', 'U' };
        static string outputPath = "./output";
        static void Main(string[] args)
        {
            var k = Int32.Parse(args[0]);
            var max = Int32.Parse(args[1]);
            for (k = k; k <= max; k++)
            {
                Console.WriteLine($"begin {k} of {max}");
                var filterCount = 10000;
                string fastqPath = args[2];
                var Reads = GetReadsFromFastq(fastqPath);
                var b = new List<output>();
                Reads.Where(x => x.Length >= filterCount).ToList().ForEach(x => b.Add(new output() { KmerCounts = GetKmerCount(k, x.Gene, DnaBase), Name = x.ReadName, Quality = x.QualityAverage, Length = x.Length }));

                //RPK of reads
                var o = new Dictionary<string, double>();
                foreach (var x in GetKmerList(k, DnaBase))
                {
                    var count = 0;
                    var length = 0;
                    foreach (var y in b)
                    {
                        count += y.KmerCounts[x];
                        length += y.Length;
                    }
                    o.Add(x, (double)((count / (double)length) ) * 1000);
                }
                Console.WriteLine($"finished calculate read's RPK ({k}/{max})");

                //参照配列のRPK
                string referenceFilePath = args[3];
                var sq = File.ReadAllLines(referenceFilePath);
                var referenceString = "";
                for (int j = 1; j < sq.Length; j++)
                {
                    referenceString += sq[j];
                }
                var referenceChars = referenceString.ToCharArray();
                var kmerCount = GetKmerCount(k, referenceChars, DnaBase);
                var csvTitle = "flowcellVer,basecallerNameVer,k,kmer,refRPK,readRPK\n";
                var csv = "";
                foreach (var x in kmerCount)
                {
                    csv += "," + "," + k.ToString() + "," + x.Key + "," + ((double)((x.Value/ (double)referenceChars.Length) ) * 1000).ToString() + "," + o[x.Key] + "\n";
                }
                if (!File.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                File.WriteAllText($"{outputPath}/RPK-{k}.csv", csvTitle + csv);
                if (!File.Exists($"{outputPath}/RPK.csv")) File.WriteAllText($"{outputPath}/RPK.csv", csvTitle);
                File.AppendAllText($"{outputPath}/RPK.csv",csv);
            }   
            Console.WriteLine("Complete!");
        }

        static List<Read> GetReadsFromFastq(string fastqPath)
        {
            var fastq = File.ReadAllLines(fastqPath);
            var reads = new List<Read>();
            for (int i = 0; i < fastq.Length; i++)
            {
                var length = Int32.Parse(Regex.Match(fastq[i], @"(?<=length=)\d+").Value.ToString());
                var name = Regex.Match(fastq[i], @"(?<=@).+(?=\s[0-9])").Value.ToString();
                i++;
                var gene = fastq[i].ToCharArray();
                i++;
                i++;
                var qChars = fastq[i].ToCharArray();
                reads.Add(new Read(name, length, gene, qChars));
            }
            return reads;
        }
        static Dictionary<string, int> GetKmerCount(int k, char[] gene, char[] bases)
        {
            var a = GetKmerList(k, bases);
            var counter = new Dictionary<string, int>();
            a.ForEach(x => counter.Add(x, 0));
            for (int i = 0; i <= gene.Length - k; i++)
            {
                var b = "";
                for (int j = 0; j < k; j++)
                {
                    b += gene[i + j];
                }
                counter[b]++;
            }
            return counter;
        }
        static List<string> GeneList(List<string> addingBases, char[] bases)
        {

            var b = new List<string>();
            foreach (var y in addingBases)
            {
                foreach (var x in bases)
                {
                    b.Add(y + x.ToString());
                }
            }
            return b;
        }
        static List<string> GetKmerList(int k, char[] bases)
        {
            var a = new List<string>();
            for (int i = 0; i < k; i++)
            {
                if (i == 0)
                {
                    foreach (var x in bases)
                    {
                        a.Add(x.ToString());
                    }
                    continue;
                }
                var b = GeneList(a, bases);
                a = b;
            }
            return a;
        }
    }
    class output
    {
        public Dictionary<string, int> KmerCounts { get; set; }
        public string Name { get; set; }
        public double Quality { get; set; }
        public int Length { get; set; }
        public output()
        {
            KmerCounts = new Dictionary<string, int>();
        }
    }

}
