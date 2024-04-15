using ScottPlot;
using System.Drawing;
using System.Globalization;

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

    public byte[] PredictedDataVisualization(double[] predictedValues, double[] realValues, double[] month)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;


        List<DateTime> dates = month.Select(unixTimestamp =>
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)unixTimestamp);
            DateTime date = dateTimeOffset.LocalDateTime;
            return date;
        }).ToList();

        double[] xs = dates.Select(x => x.ToOADate()).ToArray();

        Plot plt = new();

        plt.Palette = new ScottPlot.Palettes.Nord();
        plt.AddScatter(xs, realValues, label:"True Values");
        plt.AddScatter(xs, predictedValues, label: "Predicted Values");
        plt.XAxis.DateTimeFormat(true);

        plt.XLabel("Year");
        plt.YLabel("SPEI");
        plt.Title("SPEI");
        plt.Legend();

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
