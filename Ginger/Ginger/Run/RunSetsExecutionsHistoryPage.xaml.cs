#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Ginger.Environments;
using Ginger.Reports;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using LiteDB;
using System.Collections.Generic;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for RunSetsExecutionsPage.xaml
    /// </summary>
    public partial class RunSetsExecutionsHistoryPage : Page
    {
        ObservableList<RunSetReport> mExecutionsHistoryList = new ObservableList<RunSetReport>();
        ExecutionLoggerHelper executionLoggerHelper = new ExecutionLoggerHelper();
        public ObservableList<RunSetReport> ExecutionsHistoryList
        {
            get { return mExecutionsHistoryList; }
        }

        string mRunSetExecsRootFolder = string.Empty;
        public RunSetConfig RunsetConfig { get; set; }

        public enum eExecutionHistoryLevel
        {
            Solution,
            SpecificRunSet
        }

        private eExecutionHistoryLevel mExecutionHistoryLevel;
        public RunSetsExecutionsHistoryPage(eExecutionHistoryLevel executionHistoryLevel, RunSetConfig runsetConfig=null)
        {
            InitializeComponent();

            mExecutionHistoryLevel = executionHistoryLevel;
            RunsetConfig = runsetConfig;

            SetGridView();
            LoadExecutionsHistoryData();
        }

        public void ReloadData()
        {
            LoadExecutionsHistoryData();
        }

        private void SetGridView()
        {
            if (mExecutionHistoryLevel == eExecutionHistoryLevel.Solution)
            {
                grdExecutionsHistory.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.History, GingerDicser.GetTermResValue(eTermResKey.RunSets, "All "," Executions History"), saveAllHandler: null, addHandler: null);
            }

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = RunSetReport.Fields.Name, WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RunSetReport.Fields.Description, WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RunSetReport.Fields.StartTimeStamp, Header = "Execution Start Time", WidthWeight = 10, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RunSetReport.Fields.EndTimeStamp, Header = "Execution End Time", WidthWeight = 10, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RunSetReport.Fields.Elapsed, Header = "Execution Duration (Seconds)", WidthWeight = 10, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RunSetReport.Fields.RunSetExecutionStatus, Header = "Execution Status", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = RunSetReport.Fields.DataRepMethod, Header = "Type", Visible = true, ReadOnly = true, WidthWeight = 5, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "Generate Report", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ReportButton"] });

            grdExecutionsHistory.SetAllColumnsDefaultView(view);
            grdExecutionsHistory.InitViewItems();

            grdExecutionsHistory.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            grdExecutionsHistory.AddToolbarTool("@Open_16x16.png", "Open Execution Results Main Folder", new RoutedEventHandler(GetExecutionResultsFolder));            
            grdExecutionsHistory.AddToolbarTool("@Delete_16x16.png", "Delete Selected Execution Results", new RoutedEventHandler(DeleteSelectedExecutionResults));
            grdExecutionsHistory.AddToolbarTool("@Trash_16x16.png", "Delete All Execution Results", new RoutedEventHandler(DeleteAllSelectedExecutionResults));
            grdExecutionsHistory.RowDoubleClick += OpenExecutionResultsFolder;
        }
        public string NameInDb<T>()
        {
            var name = typeof(T).Name + "s";
            return name;
        }
        private async void LoadExecutionsHistoryData()
        {
            grdExecutionsHistory.Visibility = Visibility.Collapsed;
            Loading.Visibility = Visibility.Visible;
            mExecutionsHistoryList.Clear();
            await Task.Run(() => {
                if ( WorkSpace.Instance.Solution != null &&  WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList != null)
                {
                    mRunSetExecsRootFolder = executionLoggerHelper.GetLoggerDirectory(WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.ExecutionLoggerConfigurationExecResultsFolder);
                    //pull all RunSets JSON files from it
                    string[] runSetsfiles = Directory.GetFiles(mRunSetExecsRootFolder, "RunSet.txt", SearchOption.AllDirectories);
                    foreach (string runSetFile in runSetsfiles)
                    {
                        try
                        {
                            RunSetReport runSetReport = (RunSetReport)JsonLib.LoadObjFromJSonFile(runSetFile, typeof(RunSetReport));
                            runSetReport.DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile;
                            runSetReport.LogFolder = System.IO.Path.GetDirectoryName(runSetFile);
                            if (mExecutionHistoryLevel == eExecutionHistoryLevel.SpecificRunSet)
                            {
                                //filer the run sets by GUID
                                if (RunsetConfig != null && string.IsNullOrEmpty(runSetReport.GUID) == false)
                                {
                                    Guid runSetReportGuid = Guid.Empty;
                                    Guid.TryParse(runSetReport.GUID, out runSetReportGuid);
                                    if (RunsetConfig.Guid.Equals(runSetReportGuid))
                                        mExecutionsHistoryList.Add(runSetReport);
                                }
                            }
                            else
                                mExecutionsHistoryList.Add(runSetReport);
                        }
                        catch { }
                    }
                    LiteDbConnector dbConnector = new LiteDbConnector(Path.Combine(mRunSetExecsRootFolder, "LiteDbData.db"));
                    var rsLiteColl = dbConnector.GetCollection<LiteDbRunSet>(NameInDb<LiteDbRunSet>());
                    
                    var runSetDataColl = rsLiteColl.FindAll();
                    foreach (var runSet in runSetDataColl)
                    {
                        RunSetReport runSetReport = new RunSetReport();
                        runSetReport.DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB;
                        runSetReport.SetLiteDBData(runSet);
                        mExecutionsHistoryList.Add(runSetReport);
                    }
                }
            });

            ObservableList<RunSetReport> executionsHistoryListSortedByDate = new ObservableList<RunSetReport>();
            foreach (RunSetReport runSetReport in mExecutionsHistoryList.OrderByDescending(item => item.StartTimeStamp))
            {
                runSetReport.StartTimeStamp = runSetReport.StartTimeStamp.ToLocalTime();
                runSetReport.EndTimeStamp = runSetReport.EndTimeStamp.ToLocalTime();
                executionsHistoryListSortedByDate.Add(runSetReport);
            }
            
            grdExecutionsHistory.DataSourceList = executionsHistoryListSortedByDate;
            grdExecutionsHistory.Visibility = Visibility.Visible;
            Loading.Visibility = Visibility.Collapsed;
        }

        private void DeleteSelectedExecutionResults(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((Reporter.ToUser(eUserMsgKey.ExecutionsResultsToDelete)) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                foreach (RunSetReport runSetReport in grdExecutionsHistory.Grid.SelectedItems)
                {
                    //if(runSetReport.DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                    //{
                    //    LiteDbConnector dbConnector = new LiteDbConnector(Path.Combine(mRunSetExecsRootFolder, "LiteDbData.db"));
                    //    var rsLiteColl = dbConnector.GetCollection<LiteDbRunSet>(NameInDb<LiteDbRunSet>());
                    //    //var checkQuery = dbConnector.FilterCollection(rsLiteColl, Query.Where()
                    //    var getAll = rsLiteColl.IncludeAll().Find(rs => rs._id.ToString() == runSetReport.GUID);
                    //    List<LiteDbRunSet> allrsdata = rsLiteColl.IncludeAll().FindAll().ToList();
                    //    var setget = getAll.Select(rs => rs._id.ToString() == runSetReport.GUID);
                    //    var getlist = getAll.SelectMany(rr => rr.RunnersColl).ToList().Select(rid=>rid._id);
                    //    var getRunners = dbConnector.GetCollection<LiteDbRunner>(NameInDb<LiteDbRunner>());
                    //    foreach (var item in getlist)
                    //    {
                    //        dbConnector.DeleteCollectionItems(dbConnector.GetCollection<LiteDbRunner>(NameInDb<LiteDbRunner>()), Query.In("_id",item));
                    //    }
                    //    foreach (LiteDbRunner itemRunner in allrsdata[0].RunnersColl)
                    //    {
                    //        foreach (LiteDbBusinessFlow itemBF in itemRunner.BusinessFlowsColl)
                    //        {
                    //            foreach(LiteDbActivityGroup itemAG in itemBF.ActivitiesGroupsColl)
                    //            {

                    //            }
                    //            foreach (LiteDbActivity itemActivity in itemBF.ActivitiesColl)
                    //            {
                    //                foreach (LiteDbAction itemAction in itemActivity.ActionsColl)
                    //                {

                    //                }
                    //            }
                    //        }
                    //    }
                    //    //var getsperunners = getRunners.Find(Query.In("_id",
                    //    //var getRunners = rsLiteColl.Include(rs => rs.RunnersColl).FindAll();
                    //    //var getRunners = dbConnector.FilterCollection<LiteDbRunner>(rsLiteColl.Include(rs=>rs.RunnersColl).FindAll(),Query.All())
                    //    rsLiteColl.Delete(x => x._id.Equals(runSetReport.GUID));
                    //    //dbConnector.DeleteCollectionItems<LiteDbRunner>()
                    //    break;
                    //}
                    string runSetFolder = executionLoggerHelper.GetLoggerDirectory(runSetReport.LogFolder);

                    var fi = new DirectoryInfo(runSetFolder);
                    CleanDirectory(fi.FullName);
                    fi.Delete();
                }

                if (grdExecutionsHistory.Grid.SelectedItems.Count > 0)
                {
                    LoadExecutionsHistoryData();
                }
            }
        }
        private void DeleteAllSelectedExecutionResults(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((Reporter.ToUser(eUserMsgKey.AllExecutionsResultsToDelete)) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                foreach (RunSetReport runSetReport in grdExecutionsHistory.Grid.Items)
                {
                    string runSetFolder = executionLoggerHelper.GetLoggerDirectory(runSetReport.LogFolder);

                    var fi = new DirectoryInfo(runSetFolder);
                    CleanDirectory(fi.FullName);
                    fi.Delete();
                }

                if (grdExecutionsHistory.Grid.SelectedItems.Count > 0)
                {
                    LoadExecutionsHistoryData();
                }
            }
        }
        private static void CleanDirectory(string folderName)
        {
            try
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderName);

                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch { }
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            ReloadData();
        }

        private void GetExecutionResultsFolder(object sender, RoutedEventArgs e)
        {
            if ( WorkSpace.Instance.Solution != null &&  WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList != null)
            {
                Process.Start(executionLoggerHelper.GetLoggerDirectory( WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.ExecutionLoggerConfigurationExecResultsFolder));
            }
            else
                return;
        }

        private void OpenExecutionResultsFolder()
        {
            if (grdExecutionsHistory.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }

            string runSetFolder = ((RunSetReport)grdExecutionsHistory.CurrentItem).LogFolder;

            if (string.IsNullOrEmpty(runSetFolder))
                return;

            if (!Directory.Exists(runSetFolder))
            {
                Directory.CreateDirectory(runSetFolder);
            }

            Process.Start(runSetFolder);
        }

        private void OpenExecutionResultsFolder(object sender, RoutedEventArgs e)
        {
            OpenExecutionResultsFolder();
        }

        private void OpenExecutionResultsFolder(object sender, EventArgs e)
        {
            OpenExecutionResultsFolder();
        }

        private void ReportBtnClicked(object sender, RoutedEventArgs e)
        {
            HTMLReportsConfiguration currentConf =  WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (grdExecutionsHistory.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }

            string runSetFolder = executionLoggerHelper.GetLoggerDirectory(((RunSetReport)grdExecutionsHistory.CurrentItem).LogFolder);

            string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder), false, null, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);

            if (reportsResultFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }
            else
            {
                Process.Start(reportsResultFolder);
                Process.Start(reportsResultFolder + "\\" + "GingerExecutionReport.html");
            }
        }
    }
}
