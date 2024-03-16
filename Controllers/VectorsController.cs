using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using x_siemens_task.BLL;

namespace x_siemens_task.Controllers
{



            
    [Route("api/[controller]")]
    [ApiController]
    public class VectorsController : ControllerBase
    {
        private readonly IVectorOperations _vectorOperations;

        public VectorsController(IVectorOperations vectorOperations)
        {
            _vectorOperations = vectorOperations;
        }

        [HttpGet("findTop5MostSimilarFor/{id}")]
        public IActionResult FindTop5MostSimilarFor(int id)
        {
            try
            {
                var similarVectors = _vectorOperations.FindTop5MostSimilarFor(id);
                return Ok(similarVectors);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
