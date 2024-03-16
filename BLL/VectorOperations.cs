using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using x_siemens_task.Models;

namespace x_siemens_task.BLL
{
    public class VectorData
    {
        public int Id { get; set; }
        public string Vector { get; set; }
    }

    public class VectorOperations : IVectorOperations
    {
        private readonly List<Vector> _vectors;

        public VectorOperations()
        {
            // Load vectors from file or database
            _vectors = LoadVectorsFromFile("vectors.json");
        }

        public List<KeyValuePair<int, double>> FindTop5MostSimilarFor(int id)
        {
            Vector? targetVector = _vectors.FirstOrDefault(v => v.Id == id);

            if (targetVector == null)
            {
                throw new ArgumentException("Vector with the specified id not found.", nameof(id));
            }

            Dictionary<int, double> similarityScores = new Dictionary<int, double>();

            int optimalChunkSize = _vectors.Count / Environment.ProcessorCount;

            // Ensure minimum chunk size
            optimalChunkSize = Math.Max(optimalChunkSize, 1);

            var chunks = Enumerable.Range(0, Environment.ProcessorCount)
                                   .Select(i => _vectors.Skip(i * optimalChunkSize).Take(optimalChunkSize).ToList())
                                   .ToList();

            Parallel.ForEach(chunks, chunk =>
            {
                Dictionary<int, double> chunkSimilarityScores = new Dictionary<int, double>();

                foreach (var vector in chunk)
                {
                    if (vector.Id != id)
                    {
                        double similarity = CosineSimilarity(targetVector.Values, vector.Values);
                        chunkSimilarityScores.Add(vector.Id, similarity);
                    }
                }

                lock (similarityScores)
                {
                    foreach (var kvp in chunkSimilarityScores)
                    {
                        similarityScores.Add(kvp.Key, kvp.Value);
                    }
                }
            });

            var topSimilarVectors = similarityScores.OrderByDescending(kv => kv.Value)
                                                    .Take(5)
                                                    .ToList();

            return topSimilarVectors;
        }

        private List<Vector> LoadVectorsFromFile(string filePath)
        {
            string jsonData = File.ReadAllText(filePath);
            List<VectorData>? vectorDataList = JsonConvert.DeserializeObject<List<VectorData>>(jsonData);

            if (vectorDataList == null)
                return new List<Vector>() ;

            // Convert VectorData to Vector objects
            List<Vector> vectors = vectorDataList.Select(vectorData => new Vector
            {
                Id = vectorData.Id,
                Values = vectorData.Vector.Split(',').Select(double.Parse).ToArray()
            }).ToList();

            return vectors;
        }

        private double CosineSimilarity(double[] vector1, double[] vector2)
        {
            double dotProduct = vector1.Zip(vector2, (a, b) => a * b).Sum();
            double magnitude1 = Math.Sqrt(vector1.Sum(x => x * x));
            double magnitude2 = Math.Sqrt(vector2.Sum(x => x * x));

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0; // Avoid division by zero

            return dotProduct / (magnitude1 * magnitude2);
        }
    }
}
