using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using EnvDTE80;
using PKEngineEditor.Utilities;

namespace PKEngineEditor.GameDev
{
    public static class VisualStudio
    {
        private static DTE2? _vsInstance;

        private static readonly string ProgId = "VisualStudio.DTE.17.0";

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
    }
}