using DroughtPrediction.Services.NeuralNetwork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DroughtPrediction.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NeuralNetworkController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> TrainNetwork(IFormFile file, [FromServices] ITrainNeuralNetworkService service )
    {
        var result = await service.TrainModel(file);

        var response = new
        {
            data = new { 
                result.MeanSquaredError,
                result.RooMeanSquaredError,
                result.AbsoluteError,
                result.RSquared
            },
        };

        byte[] data = Encoding.UTF8.GetBytes(response.data.ToString());
        Response.ContentType = "application/json";

        //return Ok(File(result.imageBytes, "application/octet-stream", "filename.png"));
        return File(result.imageBytes, "application/octet-stream", "filename.png");
    }
}
