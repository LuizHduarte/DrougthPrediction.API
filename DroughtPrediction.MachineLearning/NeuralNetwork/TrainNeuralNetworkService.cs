using DroughtPrediction.Communication.Responses;
using DroughtPrediction.DataVisualization;
using DroughtPrediction.MachineLearning.Evaluation;
using DroughtPrediction.Services.DataProcessing;
using System.Data;
using Tensorflow.Keras.Engine;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

namespace DroughtPrediction.MachineLearning.NeuralNetwork;

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

    private Sequential DefineLstmModel()
    {
        var model = keras.Sequential();
        model.add(keras.layers.LSTM(units: 6, activation: keras.activations.Relu));
        //model.add(keras.layers.LSTM(units: MLMethods.hiddenUnits, activation: keras.activations.Relu, return_sequences: true));
        //model.add(keras.layers.LSTM(units: MLMethods.PredictionPoints, activation: keras.activations.Linear));
        model.add(keras.layers.Dense(units: LstmNeuralNetwork.PredictionPoints, activation: keras.activations.Linear));
        model.add(keras.layers.Dense(units: LstmNeuralNetwork.PredictionPoints, activation: keras.activations.Linear));
        model.add(keras.layers.Dense(units: LstmNeuralNetwork.PredictionPoints, activation: keras.activations.Linear));

        return model;
    }

    public async Task<NeuralNetworkEvaluationResult> TrainModel(DataTable dataTable)
    {
        var model = DefineLstmModel();

        model.compile(optimizer: keras.optimizers.Adam(),loss: keras.losses.MeanSquaredError());

        var SplitedData = await _dataProcessService.SplitSIntoTestAndTrainData(dataTable);

        var reshapedDataTrainParcel = _dataProcessService.TimeSeriesRearange(SplitedData.TrainData, LstmNeuralNetwork.TotalPoints, LstmNeuralNetwork.PredictionPoints);
        var reshapedTestDataParcel = _dataProcessService.TimeSeriesRearange(SplitedData.TestData, LstmNeuralNetwork.TotalPoints, LstmNeuralNetwork.PredictionPoints);

        var reshapedTestMonthParcel = _dataProcessService.TimeSeriesRearange(SplitedData.monthTestData, LstmNeuralNetwork.TotalPoints, LstmNeuralNetwork.PredictionPoints);

        var trainDataX = reshapedDataTrainParcel.InputData.astype(Tensorflow.TF_DataType.TF_FLOAT);
        var trainDataY = tf.squeeze(reshapedDataTrainParcel.OutputData.astype(Tensorflow.TF_DataType.TF_FLOAT)).numpy();

        var testDataX = reshapedTestDataParcel.InputData.astype(Tensorflow.TF_DataType.TF_FLOAT);
        var testDataY = tf.squeeze(reshapedTestDataParcel.OutputData.astype(Tensorflow.TF_DataType.TF_FLOAT)).numpy();

        var testDataYMonth = tf.squeeze(reshapedTestMonthParcel.OutputData).numpy();

        var history = model.fit(trainDataX, trainDataY, batch_size: 1, epochs: LstmNeuralNetwork.NumberOfEpochs, verbose: 1);
        var predictionValues = model.predict(testDataX);

        var absError = _evaluateModelService.CalculateAbsoluteError(predictionValues, testDataY);
        var mseError = _evaluateModelService.CalculateMeanSquaredError(predictionValues, testDataY);
        var rmseError = _evaluateModelService.CalculateRooMeanSquaredError(predictionValues, testDataY);
        var rSquared = _evaluateModelService.CalculateRSquared(predictionValues, testDataY);

        List<double> lossValues = [.. history.history["loss"]];

        var lossImage = _dataVisualizationService.LossVisualization(lossValues);
        var CompareValues = _dataVisualizationService.PredictedDataVisualization(predictionValues, testDataY, testDataYMonth);

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

