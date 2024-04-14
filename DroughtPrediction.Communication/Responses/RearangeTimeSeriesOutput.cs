using Numpy;

namespace DroughtPrediction.Communication.Responses;

public class RearangeTimeSeriesOutput
{
    public required NDarray InputData { get; set; }
    public required NDarray OutputData { get; set;}
}
