﻿using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.Collections;
using System.Text;

namespace Ginger.Run
{
    public class GingerRunnerLogger
    {
        string fileName; 

        public GingerRunnerLogger(string fileName)
        {
            string loggerFile = App.UserProfile.Solution.Folder + @"ExecutionResults\" + FileSystem.AppendTimeStamp(fileName);
            this.fileName = loggerFile;
        }

        public void LogAction(Act act)
        {
            if (!act.EnableActionLogConfig) return;

            StringBuilder strBuilder = new StringBuilder();
            ActionLogConfig actionLogConfig = act.ActionLogConfig;
            FormatTextTable formatTextTable = new FormatTextTable();

            // log timestamp
            strBuilder.AppendLine(GetCurrentTimeStampHeader());

            // create a new log file if not exists and append the contents
            strBuilder.AppendLine("[Action] " + act.ActionDescription);
            strBuilder.AppendLine("[Text] " + actionLogConfig.ActionLogText);

            // log all the input values
            if (actionLogConfig.LogInputVariables)
            {
                strBuilder.AppendLine("[Input Values]");
                formatTextTable = new FormatTextTable();

                ArrayList colHeaders = new ArrayList() { "Parameter", "Value" };
                formatTextTable.AddRowHeader(colHeaders);

                foreach (ActInputValue actInputValue in act.InputValues)
                {
                    ArrayList colValues = new ArrayList() { actInputValue.ItemName, actInputValue.Value };
                    formatTextTable.AddRowValues(colValues);
                }
                strBuilder.AppendLine(formatTextTable.FormatLogTable());
            }

            // log all the output variables
            if (actionLogConfig.LogOutputVariables)
            {
                strBuilder.AppendLine("[Return Values]");
                formatTextTable = new FormatTextTable();

                ArrayList colHeaders = new ArrayList() { "Parameter", "Expected", "Actual" };
                formatTextTable.AddRowHeader(colHeaders);

                foreach (ActReturnValue actReturnValue in act.ReturnValues)
                {
                    ArrayList colValues = new ArrayList() { actReturnValue.ItemName, actReturnValue.Expected, actReturnValue.Actual };
                    formatTextTable.AddRowValues(colValues);
                }
                strBuilder.AppendLine(formatTextTable.FormatLogTable());
            }

            // action status
            if (actionLogConfig.LogRunStatus)
            {
                strBuilder.AppendLine("[Run Status] " + act.Status);
            }

            // action elapsed time
            if (actionLogConfig.LogElapsedTime)
            {
                strBuilder.AppendLine("[Elapsed Time (In Secs)] " + act.ElapsedSecs);
            }

            // action error
            if (actionLogConfig.LogError)
            {
                strBuilder.AppendLine("[Error] " + act.Error);
            }

            // flush value expression
            // flush flow control

            // flush to log file
            FlushToLogFile(strBuilder.ToString());
        }

        private string GetCurrentTimeStampHeader()
        {
            StringBuilder sbr = new StringBuilder();
            sbr.AppendLine("-------------------------------");
            sbr.AppendLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            sbr.AppendLine("-------------------------------");
            return sbr.ToString();
        }

        private void FlushToLogFile(string strContents)
        {
            System.IO.File.AppendAllText(fileName, strContents);
        }

    }
}
