using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataProcessingCodingChallenge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> inputData;

        public MainWindow()
        {
            InitializeComponent();

            Trace.Listeners.Add(new TextBoxTraceListener(LogTextBox));
        }

        private void btnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                SourceFileTextBox.Text = openFileDialog.FileName;
                readInputDataAndRunTest();
            }
        }

        private void SourceFileTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            e.Handled = true;
            readInputDataAndRunTest();
        }

        private void readInputDataAndRunTest()
        {
            List<string> inputLines = new List<string>();
            FileStream fs = null;
            try
            {
                //read source file
                fs = new FileStream(SourceFileTextBox.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (TextReader tr = new StreamReader(fs))
                {
                    fs = null;
                    string line;
                    while ((line = tr.ReadLine()) != null)
                    {
                        inputLines.Add(line);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Could not read source file: " + e.Message);
                return;
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }

            //run test
            inputData = inputLines;
            btnRunTest_Click(null, null);
        }

        private void btnExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Results";
            dlg.Filter = "CSV File|*.csv";
            dlg.Title = "Save Results as CSV";

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                try
                {
                    File.WriteAllText(dlg.FileName, OutputTextbox.Text);
                    System.Diagnostics.Process.Start(dlg.FileName);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }

            }
        }

        private void btnRunTest_Click(object sender, RoutedEventArgs e)
        {
            if (inputData == null)
                return;

            CSVDataProcessor dataProcessor = new CSVDataProcessor();

            //read config values
            int ActivityId;
            if (int.TryParse(ActivityIdTextbox.Text, out ActivityId))
                dataProcessor.ActivityId = ActivityId;
            
            int NumRollingAverageEpochs;
            if (int.TryParse(NumRollingAverageEpochsTextbox.Text, out NumRollingAverageEpochs))
                dataProcessor.NumRollingAverageEpochs = NumRollingAverageEpochs;
            
            float FractionEpochsMatching;
            if (float.TryParse(FractionEpochsMatchingTextbox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out FractionEpochsMatching))
                dataProcessor.FractionEpochsMatching = FractionEpochsMatching;
            
            int ConsecutiveTriggerEpochs;
            if (int.TryParse(ConsecutiveTriggerEpochsTextbox.Text, out ConsecutiveTriggerEpochs))
                dataProcessor.ConsecutiveTriggerEpochs = ConsecutiveTriggerEpochs;
            
            int NumEndEpochs;
            if (int.TryParse(NumEndEpochsTextbox.Text, out NumEndEpochs))
                dataProcessor.NumEndEpochs = NumEndEpochs;
            
            int EpochDuration;
            if (int.TryParse(EpochDurationTextbox.Text, out EpochDuration))
                dataProcessor.EpochDuration = EpochDuration;


            //run test
            Result resultData = dataProcessor.AnalyzeArray(inputData);

            //display output data on text box
            if (resultData != null && resultData.StartTime != null)
            {
                StartTimeTextbox.Text = resultData.StartTime.Value.ToString("HH:mm");
                EndTimeTextbox.Text = resultData.EndTime.ToString("HH:mm");
                DurationTextbox.Text = string.Format("{0} ({1} seconds)", resultData.Duration.ToString(@"hh\:mm"), resultData.Duration.TotalSeconds);
                OutputTextbox.Text = string.Join("\r\n", resultData.OutputCsvArray);
            }
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }
    }
}
