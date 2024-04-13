using Tensorflow.NumPy;

namespace DroughtPrediction.Communication.Responses;

public class RearangeTimeSeriesOutput
{
    public required NDArray InputData { get; set; }
    public required NDArray OutputData { get; set;}

}
