using DroughtPrediction.Communication;
using DroughtPrediction.DataVisualization;
using DroughtPrediction.Domain;
using DroughtPrediction.Services.DataProcessing;
using DroughtPrediction.Services.Evaluation;
using Microsoft.AspNetCore.Http;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

namespace DroughtPrediction.Services.NeuralNetwork;

public class TrainNeuralNetworkService : ITrainNeuralNetworkService
{
    private readonly IDataProcessService _dataProcessService;
    private readonly IEvaluateModelService _evaluateModelService;
    private readonly IDataVisualizationService _dataVisualizationService;

    public TrainNeuralNetworkService(IDataProcessService dataProcessService, IEvaluateModelService evaluateModelService, IDataVisualizationService dataVisualizationService)
    {
        _dataProcessService = dataProcessService;
        _evaluateModelService = evaluateModelService;
        _dataVisualizationService = dataVisualizationService;
    }

    public async Task<NeuralNetworkEvaluationResult> TrainModel(IFormFile file)
    {
        //tf.compat.v1.disable_eager_execution();

        var model = keras.Sequential();
        model.add(keras.layers.LSTM(units: 6, activation: keras.activations.Relu));
        //model.add(keras.layers.LSTM(units: MLMethods.hiddenUnits, activation: keras.activations.Relu, return_sequences: true));
        //model.add(keras.layers.LSTM(units: MLMethods.PredictionPoints, activation: keras.activations.Linear));
        model.add(keras.layers.Dense(units: LstmNeuralNetwork.PredictionPoints, activation: keras.activations.Linear));
        model.add(keras.layers.Dense(units: LstmNeuralNetwork.PredictionPoints, activation: keras.activations.Linear));
        model.add(keras.layers.Dense(units: LstmNeuralNetwork.PredictionPoints, activation: keras.activations.Linear));

        model.compile(optimizer: keras.optimizers.Adam(),loss: keras.losses.MeanSquaredError());

        var testOne = await _dataProcessService.SplitSIntoTestAndTrainData(file);

        var reshapedDataTrainParcel = _dataProcessService.TimeSeriesRearange(testOne.TrainData, LstmNeuralNetwork.TotalPoints, LstmNeuralNetwork.PredictionPoints);
        var reshapedTestDataParcel = _dataProcessService.TimeSeriesRearange(testOne.TestData, LstmNeuralNetwork.TotalPoints, LstmNeuralNetwork.PredictionPoints);

        var reshapedTestMonthParcel = _dataProcessService.TimeSeriesRearange(testOne.monthTestData, LstmNeuralNetwork.TotalPoints, LstmNeuralNetwork.PredictionPoints);

        var train_dataX = reshapedDataTrainParcel.Item1.astype(Tensorflow.TF_DataType.TF_FLOAT);
        var train_dataY = tf.squeeze(reshapedDataTrainParcel.Item2.astype(Tensorflow.TF_DataType.TF_FLOAT)).numpy();

        var test_dataX = reshapedTestDataParcel.Item1.astype(Tensorflow.TF_DataType.TF_FLOAT);
        var test_dataY = tf.squeeze(reshapedTestDataParcel.Item2.astype(Tensorflow.TF_DataType.TF_FLOAT)).numpy();

        var test_dataYMonth = tf.squeeze(reshapedTestMonthParcel.Item2).numpy();

        var history = model.fit(train_dataX, train_dataY, batch_size: 1, epochs: LstmNeuralNetwork.NumberOfEpochs, verbose: 1);
        var prediction = model.predict(test_dataX);

        var absError = _evaluateModelService.CalculateAbsoluteError(prediction, test_dataY);
        var mseError = _evaluateModelService.CalculateMeanSquaredError(prediction, test_dataY);
        var rmseError = _evaluateModelService.CalculateRooMeanSquaredError(prediction, test_dataY);
        var rSquared = _evaluateModelService.CalculateRSquared(prediction, test_dataY);

        Console.WriteLine(absError + " " + mseError + " " + rmseError + " " + rSquared);

        List<double> lossValues = [.. history.history["loss"]];

        var lossImage = _dataVisualizationService.LossVisualization(lossValues);
        var CompareValues = _dataVisualizationService.PredictedDataVisualization(prediction, test_dataY, test_dataYMonth);

        NeuralNetworkEvaluationResult result = new()
        {
            AbsoluteError = absError,
            MeanSquaredError = mseError,
            RooMeanSquaredError = rmseError,
            RSquared = rSquared,
            imageBytes = CompareValues
        };

        return result;
    }
}

