using DroughtPrediction.DataVisualization;
using DroughtPrediction.Services.DataLoading;
using DroughtPrediction.Services.DataProcessing;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OneOf.Types;
using System.Text.Json.Nodes;

namespace DroughtPrediction.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DataSetController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GetSpeiData([FromServices] IDataLoadingService dataLoadingService, [FromServices] IDataProcessService dataProcessService, IFormFile file, [FromServices] IDataVisualizationService dataVisualizationService )
    {
        var data = await dataLoadingService.LoadFileData(file);
        var speiValues = dataProcessService.GetSpeiValues(data);
        var monthValues = dataProcessService.GetMonthValues(data);

        var response = dataVisualizationService.SpeiDataVisualization(speiValues.SpeiValues, monthValues);

        return File(response, "application/octet-stream", "SpeiData.png");
    }
}


