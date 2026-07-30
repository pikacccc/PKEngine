using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using EnvDTE80;
using PKEngineEditor.Utilities;
using Project = PKEngineEditor.GameProject.Project;

namespace PKEngineEditor.GameDev
{
    public static class VisualStudio
    {
        private static DTE2? _vsInstance;

        private static readonly string ProgId = "VisualStudio.DTE.17.0";
        public static bool BuildSucceeded { get; private set; }
        public static bool BuildDone { get; private set; }

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pprot);

        public static void OpenVisualStudio(string solutionPath)
        {
            IRunningObjectTable? rot = null;
            IEnumMoniker? monikerTable = null;
            IBindCtx? bindCtx = null;

            try
            {
                if (_vsInstance == null)
                {
                    var hRes = GetRunningObjectTable(0, out rot);
                    if (hRes < 0 || rot == null)
                        throw new COMException($"GetRunningObjectTable() returned HRESULT: {hRes:x8}");

                    rot.EnumRunning(out monikerTable);
                    monikerTable.Reset();

                    hRes = CreateBindCtx(0, out bindCtx);
                    if (hRes < 0 || bindCtx == null)
                        throw new COMException($"CreateBindCtx() returned HRESULT: {hRes:x8}");

                    IMoniker[] curMoniker = new IMoniker[1];
                    while (monikerTable.Next(1, curMoniker, IntPtr.Zero) == 0)
                    {
                        string name = string.Empty;
                        curMoniker[0]?.GetDisplayName(bindCtx, null, out name);
                        if (name.Contains(ProgId))
                        {
                            hRes = rot.GetObject(curMoniker[0], out object obj);
                            if (hRes < 0 || obj == null)
                                throw new COMException(
                                    $"Running object table`s GetObject() returned HRESULT: {hRes:x8}");
                            DTE2? dte = obj as DTE2;
                            var solutionName = dte?.Solution.FullName;
                            if (solutionName == solutionPath)
                            {
                                _vsInstance = dte;
                                break;
                            }
                        }
                    }

                    if (_vsInstance == null)
                    {
                        Type? visualStudioType = Type.GetTypeFromProgID(ProgId, true);
                        _vsInstance = Activator.CreateInstance(visualStudioType) as EnvDTE80.DTE2;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Logger.Log(MessageType.Error, "Failed to open Visual Studio!");
            }
            finally
            {
                if (monikerTable != null) Marshal.ReleaseComObject(monikerTable);
                if (rot != null) Marshal.ReleaseComObject(rot);
                if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);
            }
        }

        public static void CloseVisualStudio()
        {
            if (_vsInstance?.Solution.IsOpen == true)
            {
                _vsInstance.ExecuteCommand("File.SaveAll");
                _vsInstance.Solution.Close(true);
            }

            _vsInstance?.Quit();
        }

        public static bool AddFilesToSolution(string solution, string projectName, string[]? files)
        {
            Debug.Assert(files?.Length > 0);
            OpenVisualStudio(solution);
            try
            {
                if (_vsInstance != null)
                {
                    if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(solution);
                    else _vsInstance.ExecuteCommand("File.SaveAll");

                    foreach (EnvDTE.Project project in _vsInstance.Solution.Projects)
                    {
                        if (project.UniqueName.Contains(projectName))
                        {
                            foreach (var file in files)
                            {
                                project.ProjectItems.AddFromFile(file);
                            }
                        }
                    }

                    var cpp = files.FirstOrDefault(x => Path.GetExtension(x) == ".cpp");
                    if (!string.IsNullOrEmpty(cpp))
                    {
                        _vsInstance.ItemOperations.OpenFile(cpp, Constants.vsViewKindTextView).Visible = true;
                    }

                    _vsInstance.MainWindow.Activate();
                    _vsInstance.MainWindow.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Failed to add files to Visual Studio Project!");
                return false;
            }

            return true;
        }

        public static bool IsDebugging()
        {
            bool res = false;
            bool trueAgain = true;
            for (int i = 0; i < 3 && trueAgain; ++i)
            {
                try
                {
                    res = _vsInstance != null && (_vsInstance.Debugger.CurrentProgram != null ||
                                                  _vsInstance.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode);
                    trueAgain = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    System.Threading.Thread.Sleep(1000);
                }
            }

            return res;
        }

        public static void BuildSolution(Project project, string configName, bool showWindow = true)
        {
            if (IsDebugging())
            {
                Logger.Log(MessageType.Error, "Visual Studio is currently running a process");
                return;
            }

            OpenVisualStudio(project.Solution);
            BuildDone = BuildSucceeded = false;

            for (int i = 0; i < 3 && !BuildDone; ++i)
            {
                try
                {
                    if (_vsInstance != null && !_vsInstance.Solution.IsOpen)
                        _vsInstance.Solution.Open(project.Solution);

                    if (_vsInstance != null)
                    {
                        _vsInstance.MainWindow.Visible = showWindow;
                        _vsInstance.Events.BuildEvents.OnBuildProjConfigBegin += BuildEventsOnOnBuildProjConfigBegin;
                        _vsInstance.Events.BuildEvents.OnBuildProjConfigDone += BuildEventsOnOnBuildProjConfigDone;

                        try
                        {
                            foreach (var pdbFile in Directory.GetFiles(
                                         Path.Combine($"{project.Path}", $@"x64\{configName}"), "*.pdb"))
                            {
                                File.Delete(pdbFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }

                        _vsInstance.Solution.SolutionBuild.SolutionConfigurations.Item(configName).Activate();
                        _vsInstance.ExecuteCommand("Build.BuildSolution");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine($"Attempt {i}:failed to build {project.Name}");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        public static void Run(Project project, string configName, bool debug)
        {
            if (_vsInstance != null && !IsDebugging() && BuildDone && BuildSucceeded)
                _vsInstance.ExecuteCommand(debug ? "Debug.Start" : "Debug.StartWithoutDebugging");
        }

        public static void Stop()
        {
            if (_vsInstance != null && IsDebugging())
            {
                _vsInstance.ExecuteCommand("Debug.StopDebugging");
            }
        }
        
        private static void BuildEventsOnOnBuildProjConfigDone(string project, string projectConfig, string platform,
            string solutionConfig, bool success)
        {
            if (_vsInstance != null)
                _vsInstance.Events.BuildEvents.OnBuildProjConfigDone -= BuildEventsOnOnBuildProjConfigDone;
            if (BuildDone) return;

            if (success) Logger.Log(MessageType.Info, $"Building {projectConfig} configuration succeeded");
            else Logger.Log(MessageType.Error, $"Building {projectConfig} configuration failed");

            BuildDone = true;
            BuildSucceeded = success;
        }

        private static void BuildEventsOnOnBuildProjConfigBegin(string project, string projectConfig, string platform,
            string solutionConfig)
        {
            if (_vsInstance != null)
                _vsInstance.Events.BuildEvents.OnBuildProjConfigBegin -= BuildEventsOnOnBuildProjConfigBegin;
            Logger.Log(MessageType.Info, $"Building {project}, {projectConfig}, {platform}, {solutionConfig} ...");
        }
    }
}