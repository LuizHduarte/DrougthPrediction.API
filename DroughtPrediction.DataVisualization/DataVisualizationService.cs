using ScottPlot;
using System.Drawing;
using System.Globalization;
using Tensorflow;
using Tensorflow.NumPy;
using static Tensorflow.Binding;

namespace DroughtPrediction.DataVisualization;

public class DataVisualizationService : IDataVisualizationService
{
    public byte[] LossVisualization(List<double> lossValues)
    {
        Plot plt = new();

        plt.AddSignal(lossValues.ToArray());

        var image = SaveFile(plt);

        return image;
    }

    public byte[] SpeiDataVisualization(List<double> speiData, List<DateTime> monthData)
    {
        Plot plt = new();

        double[] xs = monthData.Select(x => x.ToOADate()).ToArray();

        plt.AddFillAboveAndBelow(xs, speiData.ToArray(), colorAbove: Color.Green, colorBelow: Color.Red);

        plt.XLabel("Year");
        plt.YLabel("SPEI");
        plt.Title("SPEI");

        plt.XAxis.DateTimeFormat(true);

        plt.Grid(false);

        var image = SaveFile(plt);

        return image;
    }

    public byte[] PredictedDataVisualization(Tensor predictedValues, NDArray realValues, NDArray month)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        List<double> realValuesList = [];
        List<double> predictedValuesList = [];
        List<double> monthsValuesList = [];

        foreach (double value in realValues.numpy().reshape(-1))
        {
            realValuesList.add(value);
        }

        foreach (double value in predictedValues.numpy().reshape(-1))
        {
            predictedValuesList.add(value);
        }

        foreach (double value in month.numpy().reshape(-1))
        {
            monthsValuesList.add(value);
        }

        List<string> dates = monthsValuesList.Select(unixTimestamp =>
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)unixTimestamp);
            DateTime date = dateTimeOffset.LocalDateTime;
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }).ToList();

        Plot plt = new();

        plt.Palette = new ScottPlot.Palettes.Nord();
        var realSig = plt.AddSignal(realValuesList.ToArray(), label:"True Values");
        var predictedSig = plt.AddSignal(predictedValuesList.ToArray(), label: "Predicted Values");

        realSig.Label = "True values";
        predictedSig.Label = "Predicted values";

        plt.XLabel("Year");
        plt.YLabel("SPEI");
        plt.Title("SPEI");

        plt.Grid(false);

        var image = SaveFile(plt);

        return image;
    }

    private byte[] SaveFile(Plot plt)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "temp_plot.png");

        plt.SaveFig(tempPath, 800, 600);

        var imageBytes = File.ReadAllBytes(tempPath);

        return imageBytes;
    }
}
