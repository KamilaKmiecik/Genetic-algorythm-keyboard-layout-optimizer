using System.Text.RegularExpressions;

namespace KeyboardERX
{
    internal class Program
    {
        static Random random = new Random();
        static int NumberOfCharacters = 30;
        static List<char> Letters = new List<char>() { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P',
                                                       'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', ';',
                                                       'Z', 'X', 'C', 'V', 'B', 'N', 'M', ',', '.', '/' };

        static List<double> PlacementCost = new List<double>() { 4, 2, 2, 3, 4, 5, 3, 2, 2, 4,
                                                                 1.5, 1, 1, 1, 3, 3, 1, 1, 1, 1.5,
                                                                 4, 4, 3, 2, 5, 3, 2, 3, 4, 4 };

        static Dictionary<char, double> LetterFrequency = new Dictionary<char, double>
        {
            {'A', 8.2},     {'B', 1.5},     {'C', 2.8},     {'D', 4.3},     {'E', 13},      {'F', 2.2},     {'G', 2},       {'H', 6.1},     {'I', 7},   {'J', 0.15},
            {'K', 0.77},    {'L', 4},       {';', 0.33 },   {'M', 2.4},     {'N', 6.7},     {'O', 7.5},     {'P', 1.9},     {'Q', 0.095},   {'R', 6},   {'S', 6.3},
            {'T', 9.1},     {'U', 2.8},     {'V', 0.98},    {'W', 2.4},     {'X', 0.15},    {'Y', 2},       {'Z', 0.074},   {',', 1},       {'.', 1},   {'/', 0.3}
        };

        static Dictionary<string, int> LetterPairs = GetPairs();

        static void Main(string[] args)
        {
            int N = 0;
            Console.Write("Input number of generations: "); 
            N = Convert.ToInt32(Console.ReadLine());

            List<List<char>> population = new List<List<char>>();
            population = GeneratePopulation(N);

            //Console.WriteLine("Population");
            //for (int i = 0; i < N; i++)
            //{
            //    for (int j = 0; j < NumberOfCharacters; j++)
            //        Console.Write(population[i][j]);

            //    Console.WriteLine();
            //}

            List<double> fitnessForPopulation = new List<double>();

            Console.WriteLine();
            for (int i = 0; i < N; i++)
            {
                fitnessForPopulation = PopulationFitness(population);
                Console.WriteLine($"Best fitness in generation {i+1}:\t\t{fitnessForPopulation.Min()}");
                Console.Write($"Best chromosome in generation {i+1}:\n"); Display(population, fitnessForPopulation.IndexOf(fitnessForPopulation.Min()));


                List<int> parents = new List<int>();
                parents = CreateParents(population, fitnessForPopulation, N);

                List<List<char>> children = new List<List<char>>();
                children = EdgeRecombinationCrossover(population, parents, N);

                MutationChangeRandomLetter(children, N);
                MutationChangeReverse(children, N);

                population = children;
            }
        }

        static List<List<char>> GeneratePopulation(int size)
        {
            List<List<char>> Population = new List<List<char>>();
            for (int i = 0; i < size; i++)
            {
                Population.Add(Letters.OrderBy(p => random.Next()).ToList());
            }
            return Population;
        }

        static double Fitness(List<char> chromosome)
        {
            var chromosomeToString = new string(chromosome.ToArray());
            var singleList = KeyPlacementSingle(chromosomeToString);
            var doubleList = KeyPlacementDouble(chromosomeToString);
            int singleCount = 0, doubleCount = 0;
            double fitness = 0.0;

            foreach (var pair in LetterPairs)
            {
                if (singleList.Contains(pair.Key)) singleCount += pair.Value;
                if (doubleList.Contains(pair.Key)) doubleCount += pair.Value;
            }

            chromosome.ForEach(x =>
            {
                fitness += 0.01 * LetterFrequency[x] * PlacementCost[chromosome.IndexOf(x)] + 0.01 * singleCount + 0.02 * doubleCount;
            });

            return fitness;
        }

        static List<double> PopulationFitness(List<List<char>> population)
        {
            List<double> fitnessForPopulation = new List<double>();
            double fitness = 0.0;

            population.ForEach(x =>
            {
                fitness = Fitness(x);
                fitnessForPopulation.Add(fitness);
            });

            return fitnessForPopulation;
        }

        static List<int> CreateParents(List<List<char>> population, List<double> fitnessForPopulation, int populationSize)
        {
            List<int> parents = new List<int>();

            for (int i = 0; i < populationSize; i++)
            {
                double MaxFitness = 10e10;
                int PerfectParent = 0;
                int NumberOfSearchesPerUnit = 4;

                for (int j = 0; j < NumberOfSearchesPerUnit; j++)
                {
                    int Candidate = random.Next(populationSize);

                    if (fitnessForPopulation[Candidate] < MaxFitness)
                    {
                        //zamiana
                        MaxFitness = fitnessForPopulation[Candidate];
                        MaxFitness = Candidate;
                    }

                }
                parents.Add(PerfectParent);
            }

            return parents;
        }

        static List<List<char>> EdgeRecombinationCrossover(List<List<char>> population, List<int> parents, int populationSize)
        {
            List<List<char>> children = new List<List<char>>();

            for (int i = 0; i < populationSize; i++)
            {
                List<char> parent1 = population[i];
                List<char> parent2 = population[parents[i]];

                // Create a dictionary to store the edges of each node in parent1 and parent2
                Dictionary<char, List<char>> edges = new Dictionary<char, List<char>>();
                foreach (char c in parent1)
                {
                    edges[c] = new List<char>();
                }
                for (int j = 0; j < parent1.Count; j++)
                {
                    edges[parent1[j]].Add(parent1[(j + 1) % parent1.Count]);
                    edges[parent1[j]].Add(parent1[(j - 1 + parent1.Count) % parent1.Count]);
                    edges[parent2[j]].Add(parent2[(j + 1) % parent2.Count]);
                    edges[parent2[j]].Add(parent2[(j - 1 + parent2.Count) % parent2.Count]);
                }

                // Choose a random starting node
                char currentNode = parent1[random.Next(parent1.Count)];

                // Create the child by following the edges from the starting node
                List<char> child = new List<char>();
                child.Add(currentNode);
                while (child.Count < parent1.Count)
                {
                    List<char> neighbors = edges[currentNode];
                    neighbors.RemoveAll(c => child.Contains(c));
                    if (neighbors.Count > 0)
                    {
                        currentNode = neighbors[random.Next(neighbors.Count)];
                        child.Add(currentNode);
                    }
                    else
                    {
                        // If all neighbors have already been added to the child, choose a random node that hasn't been added yet
                        currentNode = parent1.First(c => !child.Contains(c));
                        child.Add(currentNode);
                    }
                }

                children.Add(child);
            }

            return children;
        }

        static void MutationChangeRandomLetter(List<List<char>> children, int populationSize)
        {
            if (random.NextDouble() < 0.01)
            {
                for (int i = 0; i < populationSize; i++)
                {
                    char tmp;
                    int RandomLetterIndex1 = random.Next(NumberOfCharacters);
                    int RandomLetterIndex2 = random.Next(NumberOfCharacters);
                    tmp = children[i][RandomLetterIndex1];
                    children[i][RandomLetterIndex1] = children[i][RandomLetterIndex2];
                    children[i][RandomLetterIndex2] = tmp;
                }
            }
        }

        static void MutationChangeReverse(List<List<char>> children, int populationSize)
        {
            if (random.NextDouble() < 0.1)
            {
                for (int i = 0; i < populationSize; i++)
                {
                    children[i].Reverse();
                }
            }

        }

        static void Display(List<List<char>> population, int index)
        {
            int i = 0;
            foreach (var letter in population[index])
            {
                if (i % 10 == 0 && i != 0) Console.WriteLine();

                Console.Write(letter + " ");
                i++;
            }
            Console.WriteLine("\n");
        }

        static List<string> KeyPlacementSingle(string chromosomeToString)
        {
            List<string> listSingleJumps = new List<string>();

            for (int i = 0; i < chromosomeToString.Length - 1; i++)
            {
                listSingleJumps.Add(chromosomeToString[i].ToString() + chromosomeToString[i + 1].ToString());
            }
            return listSingleJumps;

        }

        //  0    1   2   3   4   5   6   7   8   9      +10
        // 10   11  12  13  14  15  16  17  18  19      +10
        // 20   21  22  23  24  25  26  27  28  29      +10

        static List<string> KeyPlacementDouble(string chromosomeToString)
        {
            List<string> listDoubleJumps = new List<string>();

            for (int i = 0; i + 20 < NumberOfCharacters; i++)
            {
                listDoubleJumps.Add(chromosomeToString[i].ToString() + chromosomeToString[i + 20].ToString());
            }

            for (int i = NumberOfCharacters - 1; i - 20 >= 0; i--)
            {
                listDoubleJumps.Add(chromosomeToString[i].ToString() + chromosomeToString[i - 20].ToString());
            }

            return listDoubleJumps;
        }

        static Dictionary<string, int> GetPairs()
        {
            string text = File.ReadAllText(@"TheProjectGutenberg.txt");
            text = Regex.Replace(text, @"[^a-zA-Z;,./]+", "");                   
            text = text.ToUpper();
            Dictionary<string, int> pairsFrequency = new Dictionary<string, int>();

            for (int i = 0; i < text.Length - 1; i++)
            {
                string pair = text.Substring(i, 2);
                if (pairsFrequency.ContainsKey(pair))
                {
                    pairsFrequency[pair]++;
                }
                else
                {
                    pairsFrequency.Add(pair, 1);
                }
            }

            foreach (var pair in pairsFrequency.ToList())
            {
                if (pair.Value < 5)
                {
                    pairsFrequency.Remove(pair.Key);
                }
            }

            using (StreamWriter sw = new StreamWriter("TheProjectGutenbergPAIRS.txt"))
            {
                foreach (var pair in pairsFrequency)
                {
                    sw.WriteLine(pair.Key + " " + pair.Value);
                }
            }

            return pairsFrequency;
        }

    }
}
