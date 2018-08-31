using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessingCodingChallenge
{
    public class CSVDataProcessor
    {
        
        // The activity id we're interested in.
        public int ActivityId = 8;

        // The number of epochs in the rolling average
        public int NumRollingAverageEpochs = 150;

        // The percent of the epochs that have to match the ActivityId to trigger a start event
        public float FractionEpochsMatching = .96f;

        // The number of consecutive epochs required to be above the trigger level to trigger a start event
        public int ConsecutiveTriggerEpochs = 150;

        // The number of epochs required to trigger an end event
        public int NumEndEpochs = 375;

        //Assumed epoch duration in seconds. It is used to calculate period duration
        public int EpochDuration = 10;



        // BUSINESS LOGIC.  This function analyzes challenge data and returns the result.
        // Returns nil if the conditions for an end trigger are not met.
        // See inline comments for specifics about the logic and implementation.
        public Result AnalyzeArray(List<string> csvArray) 
        {
            int maxNumEndEpochs = Math.Max(NumEndEpochs, NumRollingAverageEpochs + ConsecutiveTriggerEpochs);

            // Queue to hold all data relevant at a particular point in the analysis.
            Queue<Epoch> dataQueue = new Queue<Epoch>(maxNumEndEpochs);
            // Queue to hold data for the rolling average.
            Queue<Epoch> triggerQueue = new Queue<Epoch>(NumRollingAverageEpochs);
    
            int matchingEpochsInTriggerQueueCount = 0;
            int matchingEpochsInPeriod = 0;
            int streak = 0;

            Result result = new Result();
            string[] outputCsvArray = new string[csvArray.Count];
            result.OutputCsvArray = outputCsvArray;

            // Analysis
            for (int lineCount = 0; lineCount < csvArray.Count; lineCount++)
			{
                string epochString = csvArray[lineCount];
                outputCsvArray[lineCount] = epochString;
                
                // Parse the epoch from the string.
                Epoch epoch = Epoch.ParseFromString(epochString);
                if (epoch == null) 
                    continue;
        
                // Add the epoch to the queues
                Epoch oldTriggerEpoch = triggerQueue.AddObjectAndGetOverflow(epoch, NumRollingAverageEpochs);
                Epoch discardEpoch = dataQueue.AddObjectAndGetOverflow(epoch, maxNumEndEpochs);
        
                // Check if we have reached the trigger level
                matchingEpochsInTriggerQueueCount += countChange(epoch, oldTriggerEpoch);
                bool isAboveTrigger = matchingEpochsInTriggerQueueCount >= ((float)NumRollingAverageEpochs * FractionEpochsMatching);

                outputCsvArray[lineCount] += string.Format(", {0}", (float)matchingEpochsInTriggerQueueCount / NumRollingAverageEpochs); 

                // If we haven't triggered a start event yet.
                if (result.StartTime == null) {

                    matchingEpochsInPeriod += countChange(epoch, discardEpoch);

                    // Add one to the streak or reset.
                    streak = isAboveTrigger ? streak + 1 : 0;
            
                    // If a start event has been triggered.
                    if (streak >= ConsecutiveTriggerEpochs) {
                        // Record the start time
                        int startEpochQueueIndex = dataQueue.Count - (NumRollingAverageEpochs + ConsecutiveTriggerEpochs);
                        result.StartTime = getDateAtIndex(startEpochQueueIndex, dataQueue);

                        int startEpochAbsIndex = lineCount - NumRollingAverageEpochs - ConsecutiveTriggerEpochs + 1;
                        outputCsvArray[startEpochAbsIndex] += ", START OF PERIOD";
                        outputCsvArray[lineCount - ConsecutiveTriggerEpochs + 1] += ", FIRST EPOCH IN STREAK WITH ROLLING% ABOVE {0}"; 
                        outputCsvArray[lineCount] += string.Format(", ROLLING% ABOVE {0} FOR {1} CONSECUTIVE EPOCHS", FractionEpochsMatching, ConsecutiveTriggerEpochs);
                        Trace.TraceInformation("Start time found at line {0}: {1}", startEpochAbsIndex, result.StartTime);
                
                        // Pop off all data in the data queue before the start event, because we are not interested in analyzing them
                        for (int i = 0; i < startEpochQueueIndex; i++)
                        {
                            Epoch dequeuedEpoch = dataQueue.Dequeue();
                            if (dequeuedEpoch.ActivityId == ActivityId)
                                matchingEpochsInPeriod--;
                        }

                        streak = 0; //reset streak to look for an end event
                    }
                } 
                else // If we have triggered a start event, look for an end event.
                {
                    // Add one to the streak or reset.
                    streak = !isAboveTrigger ? streak + 1 : 0;

                    if (epoch.ActivityId == ActivityId)
                        matchingEpochsInPeriod++;

                    // If an end event has been triggered
                    if (streak >= NumEndEpochs) {
                        // Record the end time
                        int endEpochIndex = dataQueue.Count - NumEndEpochs;
                        result.EndTime = getDateAtIndex(endEpochIndex, dataQueue);

                        outputCsvArray[lineCount] += string.Format(", ROLLING% BELOW {0} FOR {1} CONSECUTIVE EPOCHS", FractionEpochsMatching, NumEndEpochs);
                        int endEpochAbsIndex = lineCount - NumEndEpochs + 1;
                        outputCsvArray[endEpochAbsIndex] += ", FINISH OF PERIOD";
                        Trace.TraceInformation("End time found at line {0}: {1}", endEpochAbsIndex, result.EndTime);

                        while (dataQueue.Count > NumEndEpochs) //we are only interested in the last 375
                        {
                            dataQueue.Dequeue();
                        }
                        while (dataQueue.Count > 0) //substract those within the last 375 with ActivityId=8
                        {
                            Epoch dequeuedEpoch = dataQueue.Dequeue();
                            if (dequeuedEpoch.ActivityId == ActivityId)
                                matchingEpochsInPeriod--;
                        }

                        result.Duration = new TimeSpan(0, 0, matchingEpochsInPeriod * EpochDuration);
                        // Return the result, because we have already added all of the relevant durations.
                        return result;
                    }
                }
            }
    
            // If we didn't find an end event, return null.
            return null;
        }

        // One epoch is added to the rolling average and one is taken away.  This function
        // returns the resulting change in the number of epochs in the rolling average that
        // match the required activity id.
        private int countChange(Epoch newEpoch, Epoch oldEpoch) 
        {
            int change = 0;
            if (newEpoch != null && newEpoch.ActivityId == ActivityId) {
                change++;
            }
            if (oldEpoch != null && oldEpoch.ActivityId == ActivityId) {
                change--;
            }
            return change;
        }

        // Returns the date from the epoch at the index, or from the epoch at index 0 if the
        // requested index is negative.
        private DateTime getDateAtIndex(int index, Queue<Epoch> dataQueue) 
        {
            Epoch epoch;
            if (index > 0) 
                epoch = dataQueue.ElementAt(index);
            else 
                epoch = dataQueue.Peek(); //ElementAt(0);
            
            return epoch.Date;
        }

        
    }
}
