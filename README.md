wpf-data-processing-coding-challenge
==================
A small WPF application that resolves the coding challenge detailed below, inspired on the Objective-C implementation posted by Greg Lee on https://github.com/Gregliest/Coding-Challenge-1. This challenge centers around data analysis and data structures, but also has a WPF UI.

Some sample data and analyzed data are included in the repo.


Requirements:

We need you to write an app/program which can import CSV files in this file format: Test Data.csv

Loading data: 

The app needs to have the ability to import these test files one at a time. We will leave it up to you how you want the data to be provided to the app. This import can occur from a local source, online source or by opening the the CSV attached to an email.

Output: 

After the data file is loaded and analysed, the following output should be shown on the screen:

	•	Start of Period: time of day (hh:mm)
	
	•	End of Period: time of day (hh:mm)
	
	•	Period duration: hours and minutes (hh:mm)

How these output are presented is up to you. Calculating these output is detailed below.

Determining Start and Finish

	•	The data between the start and finish points is called a ‘period’
	
	•	Conduct an analysis of the last 150 EPOCHs of activity_id. Calculate the % that equal 8 (=8). Call this rolling%. 
	
	•	If rolling% >= 96% (ie. 144+ EPOCHS of the last 150) for 150 consecutive EPOCHS, then the period starts 150 EPOCHs prior to the first EPOCHs to meet 96% (ie. at the start of the analysed period - that is the very first sample to be analysed, or 150 EPOCHs prior to the first EPOCH to get the 96% trigger
	
	•	An alternative way of saying this would be: 
	
	•	Determine rolling% for last 150 EPOCHS
	
	•	Roll this forward, EPOCH by EPOCH. If this 150 consecutive EPOCHs have >=96% equalling 8, then sleep begins 150 EPOCHs prior to the first EPOCH that is >=96%.
	
	•	Once this period has started, it ends when rolling% < 96% for 375 consecutive EPOCHs. The Finish would be at the start of the 1st consecutive sample to meet this criteria.
	
	•	The time of day for start and finish are shown in the ‘date’ column.

Determining Period Duration

	•	The period duration is not just start to end
	
	•	Let us assume each EPOCH = 10 seconds
	
	•	Period duration = accumulated duration of all EPOCH where activity_id = 8 during the period. This should be reported as HH:MM. This means you may have 8 hours between the start and finish of a period, but only have a period duration of 7.5.

An example analysis of the above CSV file with correct outputs is "Example Analysed Data.xlsx"