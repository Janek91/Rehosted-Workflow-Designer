using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Core.Presentation.Factories;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Reflection;
using System.IO;
using System.Activities.XamlIntegration;
using Microsoft.Win32;
using RehostedWorkflowDesigner.Helpers;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RehostedWorkflowDesigner.Views
{

    public partial class MainWindow : INotifyPropertyChanged
    {
        private WorkflowApplication _wfApp;
        private ToolboxControl _wfToolbox;
        private CustomTrackingParticipant _executionLog;

        private string _currentWorkflowFile = string.Empty;
        private Timer _timer;


        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _timer = new Timer(1000);
            _timer.Enabled = false;
            _timer.Elapsed += TrackingDataRefresh;

            //load all available workflow activities from loaded assemblies 
            InitializeActivitiesToolbox();

            //initialize designer
            WfDesignerBorder.Child = CustomWfDesigner.Instance.View;
            WfPropertyBorder.Child = CustomWfDesigner.Instance.PropertyInspectorView;

        }


        /// <summary>
        /// Gets or sets the execution log.
        /// </summary>
        /// <value>
        /// The execution log.
        /// </value>
        public string ExecutionLog
        {
            get { return _executionLog != null ? _executionLog.TrackData : string.Empty; }
            set { _executionLog.TrackData = value; NotifyPropertyChanged(nameof(ExecutionLog)); }
        }


        /// <summary>
        /// Tracks the data refresh.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void TrackingDataRefresh(object source, ElapsedEventArgs e)
        {
            NotifyPropertyChanged(nameof(ExecutionLog));
        }


        /// <summary>
        /// Handles the TextChanged event of the consoleExecutionLog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void consoleExecutionLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            consoleExecutionLog.ScrollToEnd();
        }


        /// <summary>
        /// show execution log in ui
        /// </summary>
        private void UpdateTrackingData()
        {
            //retrieve & display execution log
            //consoleExecutionLog.Dispatcher.Invoke(
            //    System.Windows.Threading.DispatcherPriority.Normal,
            //    new Action(
            //        delegate ()
            //        {
            //            //consoleExecutionLog.Text = _executionLog.TrackData;
            NotifyPropertyChanged(nameof(ExecutionLog));
            //        }
            //));
        }


        /// <summary>
        /// Retrieves all Workflow Activities from the loaded assemblies and inserts them into a ToolboxControl
        /// </summary>
        private void InitializeActivitiesToolbox()
        {
            try
            {
                _wfToolbox = new ToolboxControl();

                //load dependency
                AppDomain.CurrentDomain.Load("Twilio");
                // load Custom Activity Libraries into current domain
                AppDomain.CurrentDomain.Load("MeetupActivityLibrary");
                // load System Activity Libraries into current domain; uncomment more if libraries below available on your system
                AppDomain.CurrentDomain.Load("System.Activities");
                AppDomain.CurrentDomain.Load("System.ServiceModel.Activities");
                AppDomain.CurrentDomain.Load("System.Activities.Core.Presentation");
                //AppDomain.CurrentDomain.Load("Microsoft.Workflow.Management");
                //AppDomain.CurrentDomain.Load("Microsoft.Activities.Extensions");
                //AppDomain.CurrentDomain.Load("Microsoft.Activities");
                //AppDomain.CurrentDomain.Load("Microsoft.Activities.Hosting");
                //AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Utility.Activities");
                //AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Security.Activities");
                //AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Management.Activities");
                //AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Diagnostics.Activities");
                //AppDomain.CurrentDomain.Load("Microsoft.Powershell.Core.Activities");
                //AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Activities");

                // get all loaded assemblies
                IEnumerable<Assembly> appAssemblies = AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.GetName().Name);

                // check if assemblies contain activities
                int activitiesCount = 0;
                foreach (Assembly activityLibrary in appAssemblies)
                {
                    try
                    {
                        ToolboxCategory wfToolboxCategory = new ToolboxCategory(activityLibrary.GetName().Name);
                        List<Type> activityTypes = activityLibrary.GetExportedTypes().
                                Where(activityType => (
                                activityType.IsSubclassOf(typeof(Activity)) ||
                                activityType.IsSubclassOf(typeof(NativeActivity)) ||
                                activityType.IsSubclassOf(typeof(DynamicActivity)) ||
                                activityType.IsSubclassOf(typeof(ActivityWithResult)) ||
                                activityType.IsSubclassOf(typeof(AsyncCodeActivity)) ||
                                activityType.IsSubclassOf(typeof(CodeActivity)) ||
                                activityType == typeof(ForEachWithBodyFactory<Type>) ||
                                activityType == typeof(FlowNode) ||
                                activityType == typeof(State) ||
                                activityType == typeof(FinalState) ||
                                activityType == typeof(FlowDecision) ||
                                activityType == typeof(FlowNode) ||
                                activityType == typeof(FlowStep) ||
                                activityType == typeof(FlowSwitch<Type>) ||
                                activityType == typeof(ForEach<Type>) ||
                                activityType == typeof(Switch<Type>) ||
                                activityType == typeof(TryCatch) ||
                                activityType == typeof(While)) &&
                                activityType.IsVisible &&
                                activityType.IsPublic &&
                                !activityType.IsNested &&
                                !activityType.IsAbstract &&
                                activityType.GetConstructor(Type.EmptyTypes) != null).
                                OrderBy(activityType => activityType.Name).
                                ToList();

                        IEnumerable<ToolboxItemWrapper> actvities = activityTypes.Select(activityType => new ToolboxItemWrapper(activityType));

                        actvities.ToList().ForEach(wfToolboxCategory.Add);

                        if (wfToolboxCategory.Tools.Count > 0)
                        {
                            _wfToolbox.Categories.Add(wfToolboxCategory);
                            activitiesCount += wfToolboxCategory.Tools.Count;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                //fixed ForEach
                _wfToolbox.Categories.Add(
                       new ToolboxCategory
                       {
                           CategoryName = "CustomForEach",
                           Tools = {
                                new ToolboxItemWrapper(typeof(ForEachWithBodyFactory<>)),
                                new ToolboxItemWrapper(typeof(ParallelForEachWithBodyFactory<>))
                           }
                       }
                );

                LabelStatusBar.Content = $"Loaded Activities: {activitiesCount}";
                WfToolboxBorder.Child = _wfToolbox;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// Retrieve Workflow Execution Logs and Workflow Execution Outputs
        /// </summary>
        /// <param name="ev">The <see cref="WorkflowApplicationCompletedEventArgs"/> instance containing the event data.</param>
        private void WfExecutionCompleted(WorkflowApplicationCompletedEventArgs ev)
        {
            try
            {
                //retrieve & display execution log
                _timer.Stop();
                UpdateTrackingData();

                //retrieve & display execution output
                foreach (KeyValuePair<string, object> item in ev.Outputs)
                {
                    consoleOutput.Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        new Action(
                            () => consoleOutput.Text += $"[{item.Key}] {item.Value}{Environment.NewLine}"
                        ));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        #region Commands Handlers - Executed - New, Open, Save, Run

        /// <summary>
        /// Creates a new Workflow Application instance and executes the Current Workflow
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CmdWorkflowRun(object sender, ExecutedRoutedEventArgs e)
        {
            //get workflow source from designer
            CustomWfDesigner.Instance.Flush();
            MemoryStream workflowStream = new MemoryStream(Encoding.Default.GetBytes(CustomWfDesigner.Instance.Text));
            DynamicActivity activityExecute = ActivityXamlServices.Load(workflowStream) as DynamicActivity;

            //configure workflow application
            consoleExecutionLog.Text = string.Empty;
            consoleOutput.Text = string.Empty;
            _executionLog = new CustomTrackingParticipant();
            _wfApp = new WorkflowApplication(activityExecute);
            _wfApp.Extensions.Add(_executionLog);
            _wfApp.Completed = WfExecutionCompleted;

            //execute 
            _wfApp.Run();

            //enable timer for real-time logging
            _timer.Start();
        }

        /// <summary>
        /// Stops the Current Workflow
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CmdWorkflowStop(object sender, ExecutedRoutedEventArgs e)
        {
            //manual stop
            if (_wfApp != null)
            {
                _wfApp.Abort("Stopped by User");
                _timer.Stop();
                UpdateTrackingData();
            }

        }


        /// <summary>
        /// Save the current state of a Workflow
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CmdWorkflowSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (_currentWorkflowFile == string.Empty)
            {
                SaveFileDialog dialogSave = new SaveFileDialog();
                dialogSave.Title = "Save Workflow";
                dialogSave.Filter = "Workflows (.xaml)|*.xaml";

                if (dialogSave.ShowDialog() == true)
                {
                    CustomWfDesigner.Instance.Save(dialogSave.FileName);
                    _currentWorkflowFile = dialogSave.FileName;
                }
            }
            else
            {
                CustomWfDesigner.Instance.Save(_currentWorkflowFile);
            }
        }


        /// <summary>
        /// Creates a new Workflow Designer instance and loads the Default Workflow
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CmdWorkflowNew(object sender, ExecutedRoutedEventArgs e)
        {
            _currentWorkflowFile = string.Empty;
            CustomWfDesigner.NewInstance();
            WfDesignerBorder.Child = CustomWfDesigner.Instance.View;
            WfPropertyBorder.Child = CustomWfDesigner.Instance.PropertyInspectorView;
        }


        /// <summary>
        /// Creates a new Workflow Designer instance and loads the Default Workflow with C# Expression Editor
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CmdWorkflowNewCSharp(object sender, ExecutedRoutedEventArgs e)
        {
            _currentWorkflowFile = string.Empty;
            CustomWfDesigner.NewInstanceCSharp();
            WfDesignerBorder.Child = CustomWfDesigner.Instance.View;
            WfPropertyBorder.Child = CustomWfDesigner.Instance.PropertyInspectorView;
        }


        /// <summary>
        /// Loads a Workflow into a new Workflow Designer instance
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CmdWorkflowOpen(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialogOpen = new OpenFileDialog();
            dialogOpen.Title = "Open Workflow";
            dialogOpen.Filter = "Workflows (.xaml, .xamlx)|*.xaml;*.xamlx";

            if (dialogOpen.ShowDialog() == true)
            {
                using (StreamReader file = new StreamReader(dialogOpen.FileName, true))
                {
                    string content = file.ReadToEnd();
                    Regex regex = new Regex(";assembly=[^\"]*");
                    List<string> assemblies = regex.Matches(content).Cast<Match>().Select(x => x.Value.Replace(";assembly=", string.Empty)).Distinct().ToList();
                    Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                    foreach (string name in assemblies)
                    {
                        Assembly assembly = loadedAssemblies.FirstOrDefault(x => x.FullName.Split(',')[0] == name);
                        if (assembly == null)
                        {
                            OpenFileDialog dllOpen = new OpenFileDialog();
                            dllOpen.Title = $"Choose {name}.dll to use";
                            dllOpen.Filter = "Dynamic-Link Libraries (.dll)|*.dll";
                            if (dllOpen.ShowDialog() == true)
                            {
                                AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(dllOpen.FileName));
                            }
                        }
                    }
                    CustomWfDesigner.NewInstance(dialogOpen.FileName);
                    WfDesignerBorder.Child = CustomWfDesigner.Instance.View;
                    WfPropertyBorder.Child = CustomWfDesigner.Instance.PropertyInspectorView;

                    _currentWorkflowFile = dialogOpen.FileName;
                }
            }
        }

        #endregion


        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
