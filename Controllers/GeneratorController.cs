using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace x_siemens_task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneratorController : ControllerBase
    {
        private readonly string _filePath = "vectors.json";
        private readonly List<VectorData> _existingVectors;

        public GeneratorController()
        {
            // Load existing vectors from the file
            _existingVectors = JsonConvert.DeserializeObject<List<VectorData>>(System.IO.File.ReadAllText(_filePath)) ?? new List<VectorData>();
        }

        [HttpPost("generate/{count}")]
        public IActionResult GenerateRandomVectors(int count)
        {
            try
            {
                var vectors = GenerateRandomVectorData(count);
                AppendToFile(vectors);
                return Ok("Vectors added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private List<VectorData> GenerateRandomVectorData(int count)
        {
            var random = new Random();
            var vectorList = new List<VectorData>();

            // Find the maximum ID already present
            int maxId = _existingVectors.Count > 0 ? _existingVectors.Max(v => v.Id) : 0;

            for (int i = 0; i < count; i++)
            {
                var id = ++maxId; // Increment the maximum ID
                var vector = GenerateRandomVector(); // Generate random vector
                vectorList.Add(new VectorData { Id = id, Vector = vector });
            }

            return vectorList;
        }

        private string GenerateRandomVector()
        {
            // Generate a random vector with 250 dimensions
            Random random = new Random();
            List<double> randomValues = new List<double>();

            // Generate 250 random numbers between -1 and 1
            for (int i = 0; i < 250; i++)
            {
                randomValues.Add(random.NextDouble() * 2 - 1); // Random number between -1 and 1
            }

            // Convert the list of random numbers to a comma-separated string
            string vector = string.Join(",", randomValues);

            return vector;
        }

        private void AppendToFile(List<VectorData> vectors)
        {
            _existingVectors.AddRange(vectors);
            System.IO.File.WriteAllText(_filePath, JsonConvert.SerializeObject(_existingVectors));
        }
    }

    public class VectorData
    {
        public int Id { get; set; }
        public string Vector { get; set; }
    }
}
