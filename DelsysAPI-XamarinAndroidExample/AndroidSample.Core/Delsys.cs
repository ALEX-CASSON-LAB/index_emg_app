using System;
using DelsysAPI.Pipelines;
using DelsysAPI.Contracts;
using System.Collections.Generic;
using DelsysAPI.DelsysDevices;

using DelsysAPI.Events;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using DelsysAPI.Utils.TrignoBt;
using DelsysAPI.Utils;
using DelsysAPI.Components.TrignoBT;
using DelsysAPI.Configurations.DataSource;
using DelsysAPI.Configurations;
using DelsysAPI.Configurations.Component;
using DelsysAPI.Transforms;
using DelsysAPI.Channels.Transform;

using System.Reflection;

namespace AndroidSample.Core
{
    public class Delsys
    {
        Pipeline BTPipeline;
        ITransformManager TransformManager;
        public List<List<double>> Data = new List<List<double>>();
        public TrignoEmgSignal[] processedData;


        IDelsysDevice DeviceSource = null;
        private string[] DeviceFilters = new string[] {};
        int TotalLostPackets = 0; //todo check if these are useful
        int TotalDataPoints = 0;
        /********TODO UNDERSTAND WAHT THEESE ARE DOING*******/
        private Dictionary<string, SensorTrignoBt> _sensors = new Dictionary<string, SensorTrignoBt>();
        private Dictionary<Guid, string> _guidToSensor = new Dictionary<Guid, string>();
        private Dictionary<string, int> _sensorToChannel = new Dictionary<string, int>();
        /****************/

        #region EMG signal processing related variables
        private int _samplingRate = 1000;

        private BandPassFilter[] _bandPassFilters;
        private int _nChannels = 2;

        private double _movingWindowLength = 1;
        private List<double>[] _movingWindowData;

        private List<double>[] _baselineData;
        private int[] _baselineDataCounters;

        private int _baselineDataLength = 100;
        private int _baselineThrowOut = 20;

        private double[] _baselineMean;
        private double[] _baselineStdev;

        public double[] mvcs; // addition to openfeasyo

        public bool mvcCollection = false;
        #endregion

        public List<string> sensors
        {
            get { return _sensors.Keys.ToList(); }
        }

        public Delsys(){
            if (BTPipeline != null)
                Console.WriteLine(BTPipeline.CurrentState);
            if (BTPipeline == null || BTPipeline.CurrentState != Pipeline.ProcessState.Off)
            {
                InitializeSignalProcessing(); //todo mark that this is emg processing
                InitializeDataSource();
            }
        }


        #region Scan,Arm,Stream,Stop
        public async Task<bool> SensorScan()
        {
            return await BTPipeline.Scan();

        }
        public void SensorArm()
        {
            // Select every component we found and didn't filter out.
            int i = 0;
            foreach (var component in BTPipeline.TrignoBtManager.Components) //TODO change this so it displays this on the screen and user can select the sensors
            {
                BTPipeline.TrignoBtManager.SelectComponentAsync(component);
                _sensorToChannel.Add(component.Properties.SerialNumber.ToString(), i);
                i++;
            }

            ConfigurePipeline();
        }
        public Task<bool> SensorStream()
        {
            return BTPipeline.Start();
        }
        public Task<bool> SensorStop()
        {
            return BTPipeline.Stop();
        }
        #endregion

        public Task<bool> PipelineDisarm()
        {
            return BTPipeline.DisarmPipeline();
        }

        #region Initialisation
        private void InitializeDataSource()
        {
            

            // Load your license and key files
            // This tutorial assumes you have them contained in embedded resources named PublicKey.lic and License.lic, as part of
            // a solution with an output executable called AndroidSample.
            var assembly = Assembly.GetExecutingAssembly();
            string key;
            using (Stream stream = assembly.GetManifestResourceStream("AndroidSample.Core.PublicKey.lic")) // Change the name of the .lic file accordingly
            {
                StreamReader sr = new StreamReader(stream);
                key = sr.ReadLine();
            }
            string lic;
            using (Stream stream = assembly.GetManifestResourceStream("AndroidSample.Core.Carolina.lic")) // Change the name of the .lic file accordingly
            {
                StreamReader sr = new StreamReader(stream);
                lic = sr.ReadToEnd();
            }

            // The API uses a factory method to create the data source of your application.
            // This creates the factory method, which will then give the data source for your platform.
            // In this case the platform is BT.
            var deviceSourceCreator = new DelsysAPI.Android.DeviceSourcePortable(key, lic);
            // Sets the output stream for debugging information from the API. This could be a file stream,
            // but in this example we simply use the Console.WriteLine output stream.
            deviceSourceCreator.SetDebugOutputStream(Console.WriteLine);
            // Here is where we tell the factory method what type of data source we want to receive,
            // which we then set a reference to for future use.
            DeviceSource = deviceSourceCreator.GetDataSource(SourceType.TRIGNO_BT);
            // Here we use the key and license we previously loaded.
            DeviceSource.Key = key;
            DeviceSource.License = lic;
            //Load source
            LoadDataSource(DeviceSource);
            // Create a reference to the first Pipeline (which was generated by the factory method above)
            // for easier access to various objects within the API.
            //BTPipeline = PipelineController.Instance.PipelineIds[0];
            //TransformManager = PipelineController.Instance.PipelineIds[0].TransformManager;

            // Just setting up some of the necessary callbacks from the API.
            BTPipeline.CollectionStarted += CollectionStarted;
            BTPipeline.CollectionDataReady += CollectionDataReady;
            BTPipeline.CollectionComplete += CollectionComplete;
            BTPipeline.TrignoBtManager.ComponentAdded += ComponentAdded;
            BTPipeline.TrignoBtManager.ComponentLost += ComponentLost;
            BTPipeline.TrignoBtManager.ComponentRemoved += ComponentRemoved;
            BTPipeline.TrignoBtManager.ComponentScanComplete += ComponentScanComplete;

            // The component manager is how you reference specific / individual sensors so creating 
            // a reference to it will shorten a lot of calls.
            var ComponentManager = PipelineController.Instance.PipelineIds[0].TrignoBtManager;
        }
        
        
        private void LoadDataSource(IDelsysDevice ds)
        {
            // if the pipeline has not been initialized yet
            if (PipelineController.Instance.PipelineIds.Count == 0)
            {
                PipelineController.Instance.AddPipeline(ds);
            }

            BTPipeline = PipelineController.Instance.PipelineIds[0];
            TransformManager = PipelineController.Instance.PipelineIds[0].TransformManager;

            // Device Filters allow you to specify which sensors to connect to
            foreach (var filter in DeviceFilters)
            {
                BTPipeline.TrignoBtManager.AddDeviceIDFilter(filter);
            }
        }
        #endregion

        #region Componenet Callbacks -- Component Added, Scan Complete

        // This event is only sometimes called if no sensors were found
        // If no sensors are found, the scanning stops and the API does not call anything sometimes
        public void ComponentScanComplete(object sender, DelsysAPI.Events.ComponentScanCompletedEventArgs e)
        {
            for (int i = 0; i < BTPipeline.TrignoBtManager.Components.Count; i++)
                _sensors.Add(BTPipeline.TrignoBtManager.Components[i].Properties.SerialNumber.ToString(), BTPipeline.TrignoBtManager.Components[i]);

            OnScanFinished(_sensors.Keys);
        }

        public event EventHandler<ScanResultsEventArgs> ScanFinished;
        private void OnScanFinished(ICollection<string> sensors)
        {
            if (ScanFinished != null)
            {
                ScanFinished(this, new ScanResultsEventArgs(sensors));
            }
        }

        public class ScanResultsEventArgs : EventArgs
        {
            private ICollection<string> _devices;
            public ICollection<string> Devices { get { return _devices; } }

            public ScanResultsEventArgs(ICollection<string> devices)
            {
                _devices = devices;
            }
        }
        #endregion

        #region Component Callbacks -- Found, Lost, Removed

        public void ComponentAdded(object sender, ComponentAddedEventArgs e)
        {
        }

        public void ComponentLost(object sender, ComponentLostEventArgs e)
        {

        }

        public void ComponentRemoved(object sender, ComponentRemovedEventArgs e)
        {

        }

        #endregion

        #region Extra Event handlers
        //Realtime data collection
        public event EventHandler<MuscleActiveEventArgs> MuscleActive;
        private void OnMuscleActive(double[][] muscleData)
        {
            if (MuscleActive != null)
            {
                MuscleActive(this, new MuscleActiveEventArgs(muscleData));
            }
        }

        public class MuscleActiveEventArgs : EventArgs
        {
            private double[][] _muscleData;
            public double[][] MuscleData { get { return _muscleData; } }

            public MuscleActiveEventArgs(double[][] muscleData)
            {
                _muscleData = muscleData;
            }
        }
        //Realtime data collection
        public event EventHandler<CollectionStoppedEventArgs> CollectionStopped;
        private void OnCollectionStopped(int DataCount)
        {
            if (CollectionStopped != null)
            {
                CollectionStopped(this, new CollectionStoppedEventArgs(DataCount));
            }
        }

        public class CollectionStoppedEventArgs : EventArgs
        {
            private int _dataCount;
            public int DataCount { get { return _dataCount; } }

            public CollectionStoppedEventArgs(int dataCount)
            {
                _dataCount = dataCount;
            }
        }
        #endregion


        #region Collection Callbacks -- Data Ready, Colleciton Started, and Collection Complete
        public void CollectionDataReady(object sender, ComponentDataReadyEventArgs e)
        {
            TransformData[] tData = new TransformData[e.Data.Length];
            foreach (TransformData td in e.Data)
            {
                tData[_sensorToChannel[_guidToSensor[td.Id]]] = td; //for each transform data in api data, add it to tData
            }
            double[][] data = new double[e.Data.Length][];

            ////if (tData.Length >= 2 && tData[0].Data.Count == tData[1].Data.Count) // just a check to make sure that both the sensors collected the same amount of data
            ////if (tData.Length >= 2 ) //if its using two sensors and has to use two sensors
            ////{
            for (int channel = 0; channel < tData.Length; channel++)
            {
                data[channel] = new double[tData[channel].Data.Count]; //make an array inside the data [channel] that is of size of data count
                for (int sample = 0; sample < tData[channel].Data.Count; sample++)
                {
                    data[channel][sample] = tData[channel].Data[sample];
                }
            }

            processedData = Process(data);

            double[][] pData = new double[processedData.Length][];
            for(int i = 0;i<processedData.Length; i++)
            foreach(var sig in processedData)
            {
                    pData[i] = sig.AveragedSample;
            }

            if (mvcCollection == false)
            {
                double[][] normData = mvcNormalise(pData);
                OnMuscleActive(normData);
            }
            else
            {
                OnMuscleActive(pData);
            }
                
            //OnMuscleActive(pData);

            //switch (_calibrationState)
            //{
            //    case CalibrationState.Calibrating:
            //        Train(data);
            //        OnMuscleActivationChanged(new MuscleActivationChangedEventArgs(null));
            //        break;
            //    case CalibrationState.Calibrated:
            //        OnMuscleActivationChanged(new MuscleActivationChangedEventArgs(Process(data)));
            //        break;
            //    default: //CalibrationState.Uncalibrated
            //for (int c = 0; c < data.Length; c++)
            //{
            //    _signals[c] = new TrignoEmgSignal(data[c]);
            //}
            //        OnMuscleActivationChanged(new MuscleActivationChangedEventArgs(_signals));
            //        break;
            //}
            //}
            //else
            //{
            //    Console.Write("ERROR");
            //}
            /*******************************/

            // DONT GET RID OF THIS, IT ADDS TO THE COUNT //
            int lostPackets = 0;
            int dataPoints = 0;

            // Check each data point for if it was lost or not, and add it to the sum totals.
            for (int j = 0; j < e.Data.Count(); j++)
            {
                var channelData = e.Data[j];
                Data[j].AddRange(channelData.Data);
                dataPoints += channelData.Data.Count;
                for (int i = 0; i < channelData.Data.Count; i++)
                {
                    if (e.Data[0].IsLostData[i])
                    {
                        lostPackets++;
                    }
                }
            }
            TotalLostPackets += lostPackets;
            TotalDataPoints += dataPoints;

        }

        //myown
        public double[][] mvcNormalise(double[][] data)
        {

            double[][] normData = new double[data.Length][];

            for (int i = 0; i < data.Length; i++) // for each channel
            {
                double mvc = mvcs[i];
                double[] normSig = new double[data[i].Length];
                for (int j = 0; j < data[i].Length; j++) // for each datapoint
                {
                    double dp = data[i][j];
                    
                    normSig[j] = ((dp / mvc) * 100);
                    Console.WriteLine(dp.ToString() + "/ " + mvc.ToString() + "* 100 = " + normSig[j].ToString());
                }
                normData[i] = normSig;
            }
            return normData;
        }


        //from openfeasyo
        private void FullWaveRectification(double[] values, double[] destination)
        {
            //TODO use vector approach
            for (int i = 0; i < values.Length; i++)
            {
                destination[i] = Math.Abs(values[i]);
            }
        }

        private void InitializeSignalProcessing()
        {
            this._movingWindowLength = Math.Floor((float)_samplingRate / 10);
            this._baselineDataLength = (int)_samplingRate * 2;
            this._baselineThrowOut = (int)Math.Floor((float)_baselineDataLength / 5);

            this._movingWindowData = new List<double>[_nChannels];
            this._baselineData = new List<double>[_nChannels];
            this._bandPassFilters = new BandPassFilter[_nChannels];

            this._baselineDataCounters = new int[_nChannels];
            this._baselineMean = new double[_nChannels];
            this._baselineStdev = new double[_nChannels];

            for (int i = 0; i < _nChannels; i++)
            {
                this._movingWindowData[i] = new List<double>();
                this._baselineData[i] = new List<double>();
                this._bandPassFilters[i] = new BandPassFilter(BandPassFilter.BAND_PASS, _samplingRate, new double[] { 10, 500 }, 6);

                this._baselineDataCounters[i] = 0;
                this._baselineMean[i] = -1;
                this._baselineStdev[i] = -1;
            }
        }
        private TrignoEmgSignal MovingWindowAverageFilter(TrignoEmgSignal signal, int index)
        {
            bool activated = false;
            double onOff = 0;
            for (int i = 0; i < signal.FullWaveSample.Length; i++)
            {
                if (_movingWindowData[index].Count == _movingWindowLength)
                {
                    _movingWindowData[index].RemoveAt(0);
                    _movingWindowData[index].Add(signal.FullWaveSample[i]);
                }
                else
                {
                    _movingWindowData[index].Add(signal.FullWaveSample[i]);
                }

                double currentMean = Mean(_movingWindowData[index]);

                signal.AveragedSample[i] = currentMean;

                // not needed in this context
                if (currentMean > (_baselineMean[index] + (3 * _baselineStdev[index])))
                {
                    onOff = currentMean;
                }
                else
                {
                    onOff = 0;
                }

                signal.OnOff[i] = onOff;
                activated = activated || !(onOff == 0);
                signal.RestingMean[i] = _baselineMean[index];
                signal.RestingStdev[i] = _baselineStdev[index];

            }
            signal.MuscleActivated = activated;

            return signal;
        }

        /// <summary>
        /// Calculates mvc based on the processData
        /// Called post collection mvc data
        /// </summary>
        /// <returns></returns>
        public List<double> calculate_MVC()
        {
            double mvc = 1; // default 
            double sum = 0;

            List<double> mvcs = new List<double>();


            for (int i = 0; i < processedData.Length; i++) // For each channel/sensor
            {
                double[] mvcData = processedData[i].AveragedSample;
                foreach (var pt in mvcData) // for each data point
                {
                    sum = sum + (pt * pt); // Add the squares of all values
                }
                mvc = sum /mvcData.Length;
                mvc = Math.Sqrt(mvc); // square root

                mvcs.Add(mvc);
            }

            return mvcs;
        }
        private double Mean(List<double> data)
        {
            Double sum = 0;
            foreach (Double val in data)
            {
                sum += val;
            }

            return sum / data.Count;
        }

        private double StandardDeviation(List<double> data, double mean)
        {
            Double res = 0;
            foreach (Double value in data)
            {
                res += (value - mean) * (value - mean);
            }
            return Math.Sqrt(res / data.Count);
        }
        //original
    
        private TrignoEmgSignal[] Process(double[][] rawData)
        {
            int count = Math.Min(_nChannels, rawData.Length);
            TrignoEmgSignal[] signals = new TrignoEmgSignal[count];


            for (int index = 0; index < count; index++)
            {
                TrignoEmgSignal signal = new TrignoEmgSignal(rawData[index]);

                signal.BpfSample = _bandPassFilters[index].filterData(signal.RawSample);
                FullWaveRectification(signal.BpfSample, signal.FullWaveSample);
                signals[index] = MovingWindowAverageFilter(signal, index);

            }

            return signals;
        }

    private void CollectionStarted(object sender, DelsysAPI.Events.CollectionStartedEvent e)
        {
            var comps = PipelineController.Instance.PipelineIds[0].TrignoBtManager.Components;

            // Refresh the counters for display.
            TotalDataPoints = 0;
            TotalLostPackets = 0;

            // Recreate the list of data channels for recording
            int totalChannels = 0;
            for (int i = 0; i < comps.Count; i++)
            {
                for (int j = 0; j < comps[i].BtChannels.Count; j++)
                {
                    if (Data.Count <= totalChannels)
                    {
                        Data.Add(new List<double>());
                    }
                    else
                    {
                        Data[totalChannels] = new List<double>();
                    }
                    totalChannels++;
                }
            }
            // CC: Commented out to avoid useless computing power - originally from the delsys sample
            //Task.Factory.StartNew(() => {
            //    Stopwatch batteryUpdateTimer = new Stopwatch();
            //    batteryUpdateTimer.Start();
            //    while (BTPipeline.CurrentState == Pipeline.ProcessState.Running)
            //    {
            //        if (batteryUpdateTimer.ElapsedMilliseconds >= 500)
            //        {
            //            foreach (var comp in BTPipeline.TrignoBtManager.Components)
            //            {
            //                if (comp == null)
            //                    continue;
            //                Console.WriteLine("Sensor {0}: {1}% Charge", comp.Properties.SerialNumber, BTPipeline.TrignoBtManager.QueryBatteryComponentAsync(comp).Result);
            //            }
            //            batteryUpdateTimer.Restart();
            //        }
            //    }
            //});
        }

        /// <summary>
        /// Data collection complete, stores each data point into a csv file. 
        /// Increments for every run.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CollectionComplete(object sender, DelsysAPI.Events.CollectionCompleteEvent e)
        {
            OnCollectionStopped(Data[0].Count);//TODO add for both sensors
            //string path = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            //for (int i = 0; i < Data.Count; i++)
            //{
            //    string filename = Path.Combine(path, "sensor" + i + "_data.csv");
            //    using (StreamWriter channelOutputFile = new StreamWriter(filename, true))
            //    {
            //        foreach (var pt in Data[i])
            //        {
            //            channelOutputFile.WriteLine(pt.ToString());
            //        }
            //    }
            //}
            // If you do not disarm the pipeline, then upon stopping you may begin streaming again.
            //BTPipeline.DisarmPipeline().Wait();
        }

        #endregion

        #region Data Collection Configuration

        /// <summary>
        /// Configures the input and output of the pipeline.
        /// </summary>
        /// <returns></returns>
        private bool CallbacksAdded = false;

        private bool ConfigurePipeline()
        {
            if (PipelineController.Instance.PipelineIds[0].CurrentState == Pipeline.ProcessState.OutputsConfigured || PipelineController.Instance.PipelineIds[0].CurrentState == Pipeline.ProcessState.Armed)
            {
                PipelineController.Instance.PipelineIds[0].DisarmPipeline();
            }
            if (PipelineController.Instance.PipelineIds[0].CurrentState == Pipeline.ProcessState.Running)
            {
                // If it is running; stop the task and then disarm the pipeline
                PipelineController.Instance.PipelineIds[0].Stop();
            }

            // Arm everything (should be in "Connected" State)
            var state = PipelineController.Instance.PipelineIds[0].CurrentState;

            if (CallbacksAdded)
            {
                BTPipeline.TrignoBtManager.ComponentAdded -= ComponentAdded;
                BTPipeline.TrignoBtManager.ComponentLost -= ComponentLost;
                BTPipeline.TrignoBtManager.ComponentRemoved -= ComponentRemoved;
            }

            BTPipeline.TrignoBtManager.ComponentAdded += ComponentAdded;
            BTPipeline.TrignoBtManager.ComponentLost += ComponentLost;
            BTPipeline.TrignoBtManager.ComponentRemoved += ComponentRemoved;
            CallbacksAdded = true;

            PipelineController.Instance.PipelineIds[0].TrignoBtManager.Configuration = new TrignoBTConfig() { EOS = EmgOrSimulate.EMG };

            var inputConfiguration = new BTDsConfig();
            inputConfiguration.NumberOfSensors = BTPipeline.TrignoBtManager.Components.Count;

            foreach (var somecomp in BTPipeline.TrignoBtManager.Components.Where(x => x.State == SelectionState.Allocated))
            {
                string selectedMode = "EMG RMS";
                //Synchronize to the UI thread and check if the mode textbox value exists in the
                // available sample modes for the sensor.

                somecomp.SensorConfiguration.SelectSampleMode(selectedMode);

                if (somecomp.SensorConfiguration == null)
                {
                    return false;
                }
            }

            PipelineController.Instance.PipelineIds[0].ApplyInputConfigurations(inputConfiguration);
            var transformTopology = GenerateTransforms();//For multi Sensors
            PipelineController.Instance.PipelineIds[0].ApplyOutputConfigurations(transformTopology);
            PipelineController.Instance.PipelineIds[0].RunTime = double.MaxValue;


            return true;
        }

        /// <summary>
        /// Generates the Raw Data transform that produces our program's output.
        /// </summary>
        /// <returns>A transform configuration to be given to the API pipeline.</returns>
        public OutputConfig GenerateTransforms()
        {
            // Clear the previous transforms should they exist.
            TransformManager.TransformList.Clear();

            int channelNumber = 0;
            // Obtain the number of channels based on our sensors and their mode.
            for (int i = 0; i < BTPipeline.TrignoBtManager.Components.Count; i++)
            {
                if (BTPipeline.TrignoBtManager.Components[i].State == SelectionState.Allocated)
                {
                    var tmp = BTPipeline.TrignoBtManager.Components[i];

                    BTCompConfig someconfig = tmp.SensorConfiguration as BTCompConfig;
                    if (someconfig.IsComponentAvailable())
                    {
                        channelNumber += BTPipeline.TrignoBtManager.Components[i].BtChannels.Count;
                    }

                }
            }

            // Create the raw data transform, with an input and output channel for every
            // channel that exists in our setup. This transform applies the scaling to the raw
            // data from the sensor.
            var rawDataTransform = new TransformRawData(channelNumber, channelNumber);
            PipelineController.Instance.PipelineIds[0].TransformManager.AddTransform(rawDataTransform);

            // The output configuration for the API to use.
            _guidToSensor.Clear(); //NEW
            var outconfig = new OutputConfig();
            outconfig.NumChannels = channelNumber;

            int channelIndex = 0;
            for (int i = 0; i < BTPipeline.TrignoBtManager.Components.Count; i++)
            {
                if (BTPipeline.TrignoBtManager.Components[i].State == SelectionState.Allocated)
                {
                    BTCompConfig someconfig = BTPipeline.TrignoBtManager.Components[i].SensorConfiguration as BTCompConfig;
                    if (someconfig.IsComponentAvailable())
                    {
                        // For every channel in every sensor, we gather its sampling information (rate, interval, units) and create a
                        // channel transform (an abstract channel used by transforms) from it. We then add the actual component's channel
                        // as an input channel, and the channel transform as an output. 
                        // Finally, we map the channel counter and the output channel. This mapping is what determines the channel order in
                        // the CollectionDataReady callback function.
                        for (int k = 0; k < BTPipeline.TrignoBtManager.Components[i].BtChannels.Count; k++)
                        {
                            var chin = BTPipeline.TrignoBtManager.Components[i].BtChannels[k];
                            var chout = new ChannelTransform(chin.FrameInterval, chin.SamplesPerFrame, BTPipeline.TrignoBtManager.Components[i].BtChannels[k].Unit);
                            TransformManager.AddInputChannel(rawDataTransform, chin);
                            TransformManager.AddOutputChannel(rawDataTransform, chout);
                            Guid tmpKey = outconfig.MapOutputChannel(channelIndex, chout);
                            _guidToSensor.Add(chout.Id, BTPipeline.TrignoBtManager.Components[i].Properties.SerialNumber); //NEW
                            channelIndex++;
                        }
                    }
                }
            }
            return outconfig;
        }

        #endregion

        #region Helper classes

        public List<List<double>> Normalise(double mvc)
        {
            List<List<double>> normData = new List<List<double>>();

            foreach (List<double> channel in Data)
            {
                IEnumerable<double> normP = channel.Select(i => (i / mvc) * 100);
                normData.Add(normP.ToList()) ;
            }

            Console.WriteLine(Data[0][400]);
            Console.WriteLine(normData[0][400]);
            return normData;
        }

        // Clear the variable holding the data. This can be called at the end of an exercise for example.
        public void ClearData()
        {
            Data = new List<List<double>>();
        }

        # endregion
    }


    public class TrignoEmgSignal 
    {
        public TrignoEmgSignal(double[] rawSample)
        {
            RawSample = rawSample;
            FullWaveSample = new double[rawSample.Length];
            AveragedSample = new double[rawSample.Length];
            OnOff = new double[rawSample.Length];
            RestingMean = new double[rawSample.Length];
            RestingStdev = new double[rawSample.Length];
        }


        public bool MuscleActivated { get; set; }

        public double[] RawSample { get; set; }

        public double[] BpfSample { get; set; }

        public double[] AveragedSample { get; set; }

        public double[] FullWaveSample { get; set; }

        public double[] OnOff { get; set; }

        public double[] RestingMean { get; set; }

        public double[] RestingStdev { get; set; }
    }
}
