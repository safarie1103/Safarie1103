using System;
using System.Collections.Generic;
using System.Xml.Linq;  // KAW - changed from System.Linq
using System.Text;
using System.IO;
using System.Xml;
using System.Data;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Net;
using IanArchiveReaderWrapperLib;
using System.Management;
using System.Security.Permissions;
using EES.Framework.XMLMapping;
using EES.SDK.EquipmentAdapter;
using EES.SDK.Interfaces;
using EES.Framework.Logging;
using EES.Framework.Interfaces.Logging;
using EES.Framework.Common.RuntimeSetting;
using System.Linq;



namespace IanArchiveDataLoader
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class ToolRunData
    {
        //public List<AlarmEvents> _AlarmList = new List<AlarmEvents>();
        //public List<AlarmEvents> _EventList = new List<AlarmEvents>();
        //public List<StrategyAlarmEventInfo> _EventAlarmForPublish = new List<StrategyAlarmEventInfo>();

        //public List<DateTime> _TimeStampList = new List<DateTime>();
        //public List<List<string>> _VarValues = new List<List<string>>();
        //public List<BatchAlarmEventData> _RunDataList = new List<BatchAlarmEventData>();

        //public List<string> _ProcessVarList = new List<string>();
        //public List<string> _ContextList = new List<string>();
        //public List<string> _MaterialContextList = new List<string>();
        //public ManualResetEvent _ThreadEvent = new ManualResetEvent(true);
    }

    class Program : ICollector
    {

        private static Boolean DCPUpdated = false;
        private static Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans = new Dictionary<string, List<EqDataCollectionPlanInfo>>();
        private static Dictionary<string, ArrayList> DCPlanDataItems = new Dictionary<string, ArrayList>();
        private static int MaxDelayBetweenWafers = 0;
        private static FileStream fContextData;
        private static FileStream fSignalData;
        private static FileStream fProcessedArchives;
        private static FileStream fUnProcessedArchives;
        private static Dictionary<string, UInt32> _SensorList = null;
        //private static ArchiveReaderWrapper _ArchiveReader = null;
        
        private static LogFileRing ringFile = null;    // for log file management
        private static LogFileRing StatsFile = null;    // for log file management

        private static uint NumberOfRingFiles = 100;            // for log file management
        private static uint RingFileSize = 10;                 // for log file management

        //private static ArrayList HeaderArray = new ArrayList();
        //private static ArrayList T000Array = new ArrayList();
        //private static ArrayList T008Array = new ArrayList();
        //private static ArrayList T009Array = new ArrayList();
        //private static ArrayList T028Array = new ArrayList();
        private static string toolName = string.Empty;
        private static string discoveryUrl = string.Empty;
        private static EES.SDK.Interfaces.IEquipmentAdapter _adapterSDK = null;
        private static string mainFolder = string.Empty;
        private static string processingFolder = string.Empty;
        private static string processedFolder = string.Empty;
        private static string expiredFolder = string.Empty;
        private static string newFilesFolder = string.Empty;
        private static string unProcessedFolder = string.Empty;
        private static string SignalDataFolder = string.Empty;
        private static int numFilesFound = 0;
        private static string processingStatusFile = string.Empty;
        private static string materialKeyContext = string.Empty;
        private static Dictionary<string, string> processingStatus = new Dictionary<string, string>();


        private static string ImplantFilesLocation;
        private static string BeamSetupFilesLocation;
        private static string ArchiveFilesLocation;
        private static string WorkingFolder;
        private static Boolean CreateCSVOnly = false;
        private static Boolean CreateCSVAndLoad = true;
        private static Boolean LoadOnly = true;
        private static string NetworkName = string.Empty;
        private static string Domain = string.Empty;
        private static string Username = string.Empty;
        private static string Password = string.Empty;


        private static DataTable ArchiveDataTable = new DataTable();
        //private static Dictionary<String, ArrayList> PlanDataItems = new Dictionary<string, ArrayList>();
        private static DataTable ProcessedFolders = new DataTable();

        private static DirectoryInfo ImplantsDir;
        private static DirectoryInfo BeamSetupDir;
        private static DirectoryInfo ArchivedDir;
        private static DirectoryInfo ProcessedDir;
        private static DirectoryInfo UnProcessedDir;
        private static DirectoryInfo SignalDataDir;
        private static string DataSource = string.Empty;
        private static string LoadOption = string.Empty;

        private static DirectoryInfo[] ImplantDateDirs;
        private static DirectoryInfo[] BeamSetupDateDirs;
        private static uint[] SelectedSignals;
        private static DateTime ArchiveStartTime = new DateTime();
        private static DateTime ArchiveEndTime = new DateTime();
        private static ushort ArchiveStartMilliseconds = 0;
        private static ushort ArchiveEndMilliseconds = 0;

        private static DateTime SpecifiedStartDateTimeUTC = new DateTime();
        private static DateTime SpecifiedEndDateTimeUTC = new DateTime();

        //private static DataTable ImplantData = new DataTable();
        private static ArchiveReaderWrapper _ArchiveReader = new ArchiveReaderWrapper();


        private static string[] sCommaDelimiter = new string[1];
        private static char[] cTrimArray = new char[1];

        public static Object fileLock = new Object();
        public ArrayList _sessionList = new ArrayList();
        const string E3_EVENT = "E3_EventName";
        const string OS_WINDOWS = "Windows";
        const string OS_UNIX = "Unix";
        const string EV_DELIMITER = ".";
        const string CMD_SUCCESS = "SUCCESS";
        const string CMD_FAIL = "FAIL";
        const string CMD_GET_DATA = "GET_DATA";

        public const string logDTFormat = "MM/dd hh:mm:ss.fff ";
        public static string logFilePath = "";
        public static string logFileName = "";
        public static int logFileMaxSize = 10000000;  // 10 MB
        public static string dataFilePath = "";
        public static System.DayOfWeek dowPrev = DateTime.Now.DayOfWeek;   // day of week
        static ICollector instance = null;



        const int DEFAULT_FILEREOPENINTERVAL = 0;
        const int DEFAULT_FILESIZELIMIT = 5000;        //default message max number in a log file


        //used by mutex

        //public static System.Threading.Mutex mutex;

        //capture close console event
        public delegate bool ConsoleCtrlDelegate(int dwCtrlType);
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        private const int CTRL_CLOSE_EVENT = 2;

        //using in ParsingFile method
        private static string _errFileName = string.Empty;
        private static string _inputFileName = string.Empty;
        //private static int rowId, cRowId, errRowId; //rowId stands for total processing row num in input file,

        //cRowId stands for correct row num in input file,
        //errRowId stands for error row num in input file

        //private static bool continueFlag = false;//Check whether need to send START Event
        //private static bool flag = false;        //If rows>TransferSize, set flag = true
        private static ArrayList eventList = new ArrayList();
        static Hashtable EventsInfo = new Hashtable();
        public Hashtable eventCache = new Hashtable();


        //KAW - new variables to hold EA state info...
        //public ArrayList Batchsomething???
        //public int EA_STATE ?? "current batch" "current lot" "current run"
        //  Need to cache Lots in Batch & Wafers in Lots & Wafers in Batch...

        public static int _currentRunId = -99;
        public static string _currentBatchId = "";
        public static string _currentLotId = "";
        public static ArrayList _batchList = new ArrayList();
        public static ArrayList _lotList = new ArrayList();
        public static ArrayList _waferList = new ArrayList();

        // *** Main ***
        static void Main(string[] args)
        {
            Process proc = Process.GetCurrentProcess();
            //logFilePath = logFilePath = Application.StartupPath + "\\logfiles\\";
            //string path = GetInstallPath() + @"\logfiles";
            //ringFile = new LogFileRing(logFilePath + @"\" + "_" + proc.Id.ToString(), System.Net.Dns.GetHostName(), " ", "IanArchiveDataLoader", " ", NumberOfRingFiles, RingFileSize * 1024 * 1024);
            //StatsFile = new LogFileRing(logFilePath + @"\" + "_" + proc.Id.ToString() + "_Stats", System.Net.Dns.GetHostName(), " ", "Statistics", " ", NumberOfRingFiles, RingFileSize * 1024 * 1024);
            try
            {
                // Check Input Parameters
                if (args != null && args.Length == 2)
                {
                    toolName = args[0];
                    discoveryUrl = args[1];

                    logFilePath = logFilePath = Application.StartupPath + "\\logfiles\\" + toolName + "\\";

                    ringFile = new LogFileRing(logFilePath + @"\" + toolName + "_" + proc.Id.ToString(), System.Net.Dns.GetHostName(), " ", "IanArchiveDataLoader", " ", NumberOfRingFiles, RingFileSize * 1024 * 1024);
                    StatsFile = new LogFileRing(logFilePath + @"\" + toolName + "_" + proc.Id.ToString() + "_Stats", System.Net.Dns.GetHostName(), " ", "Statistics", " ", NumberOfRingFiles, RingFileSize * 1024 * 1024);

                    Log("Starting Equipment Adapter");
                    Log(toolName);
                    Log(discoveryUrl);
                }
                else
                {
                    Log("Wrong arguments");
                    throw (new Exception("wrong arguments"));
                }

                // Establish instance of this Class
                ICollector _collector = Program.Instance();

                _collector.Init();
                _collector.ConnectToEqp();
                _collector.ConnectToE3(toolName);
                LoadConfig();
                UpdateDCP();
                Log("End of Main");

            }
            catch (Exception ex)
            {
                Log("Exception in Main" + ex.Message);
            }
            SendDataLoaderIdle(toolName, toolName + "_DataLoaderReport", DateTime.Now);

            //RunFileSystemWatcher();

        }
        public static void RunFileSystemWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = @"C:\ArchiveDataLoader\Archives\";
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = "Myfile.txt";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;


        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Log("File: " + e.FullPath + " " + e.ChangeType);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Log("File: " + e.OldFullPath + " renamed to " + e.FullPath);
        }

        // Create instance of class
        public static ICollector Instance()
        {
            if (instance == null)
            {
                instance = new Program();
            }
            return instance;
        }
        public Program()
        {

        }

        public void Init()
        {

            Log("Starting Init Method...");
            sCommaDelimiter[0] = ",";
            try
            {
                // Set the entry point to E3 - Discovery Service URL
                EquipmentAdapter.DiscoveryServiceURL = discoveryUrl;

                // Load the config from DB into Adapter
                EquipmentAdapter.GetEquipmentConfig(toolName);

                // Create adapter 
                _adapterSDK = new EquipmentAdapter();

                // Create Subscriptions
                _adapterSDK.ToolCmdEvent += new SendToolCommand(_adapterSDK_ToolCmdEvent);
                _adapterSDK.StartOfRunEvent += new SendStartOfRunEvent(_adapterSDK_StartOfRunEvent);
                _adapterSDK.EqDCPStateChangeEvent += new SendEqDCPStateChangeEvent(_adapterSDK_EqDCPStateChangeEvent);
                _adapterSDK.EndOfRunEvent += new SendEndOfRunEvent(_adapterSDK_EndOfRunEvent);
                _adapterSDK.DbBatchWriteFailEvent += new SendDbBatchWriteFailEvent(_adapterSDK_DbBatchWriteFailEvent);
                _adapterSDK.ConfigChangeEvent += new SendConfigChangeEvent(_adapterSDK_ConfigChangeEvent);
                _adapterSDK.ComStateChangeEvent += new SendCommStateChangeEvent(_adapterSDK_ComStateChangeEvent);

            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

        }
        void _adapterSDK_ConfigChangeEvent(string equipmentName, IConfigChangeEventInfo configChangeInfo)
        {
            logMsg("Starting _adapterSDK_ConfigChangeEvent...");
            if (configChangeInfo.Source == "Integration:EquipmentTemplate" || configChangeInfo.Source == "ProcessTypeEditor")
            {
                DCPUpdated = true;
                UpdateDCP();
            }
            else
            {
                LoadConfig();
            }
        }
        private static void UpdateDCP()
        {
            string functionName = "UpdateDCP";
            Log(functionName + ": Start to get UpdateDCP ");
            DCPlans = GetDCPlans(toolName);
            DCPlanDataItems = GetDCPlanDataItems(DCPlans);
            foreach (string key in DCPlanDataItems.Keys)
            {
                Log(functionName + " DCPlanDataItems[" + key + "] length: " + DCPlanDataItems.Count.ToString());
            }
        }

        private static void LoadConfig()
        {
            string functionName = "LoadConfig";
            Log(functionName + ": Start to get LoadConfig ");
            try
            {
                IEquipmentConfig eqconfig = EquipmentAdapter.GetEquipmentConfig(toolName);

                int index = 0;
                string configXml = string.Empty;

                foreach (string type in eqconfig.Type)
                {
                    if (type.CompareTo(EES.Framework.Common.DataStructures.Constants.CONFIG_TYPE_PLUGIN_DLL) == 0)
                    {
                        configXml = eqconfig.Data[index];
                        break;
                    }
                    index++;
                }
                if (configXml != string.Empty)
                {
                    XmlDocument configDoc = new XmlDocument();

                    configDoc.LoadXml(configXml);

                    XmlNode rootNode = configDoc.DocumentElement;

                    XmlNodeList DataItemMappings = rootNode.SelectSingleNode("CustomSettings").ChildNodes;
                    //_DataItemMapping.Clear();
                    foreach (XmlNode oNode in DataItemMappings)
                    {
                        if (oNode.NodeType != XmlNodeType.Comment)
                        {
                            string Name = oNode.Attributes["Name"].Value;


                            switch (Name)
                            {
                                case "MaxDelayBetweenWafers":
                                    MaxDelayBetweenWafers = Convert.ToInt16(oNode.Attributes["Value"].Value);
                                    break;
                                case "CreateCSVOnly":
                                    CreateCSVOnly = Convert.ToBoolean(oNode.Attributes["Value"].Value);
                                    break;
                                case "CreateCSVAndLoad":
                                    CreateCSVAndLoad = Convert.ToBoolean(oNode.Attributes["Value"].Value);
                                    break;
                                case "LoadOnly":
                                    LoadOnly = Convert.ToBoolean(oNode.Attributes["Value"].Value);
                                    break;
                                default:
                                    break;
                            }


                        }

                    }
                    Log(functionName + ": MaxDelayBetweenWafers: " + MaxDelayBetweenWafers);
                    Log(functionName + ": CreateCSVOnly: " + CreateCSVOnly);
                    Log(functionName + ": CreateCSVAndLoad: " + CreateCSVAndLoad);
                    Log(functionName + ": LoadOnly: " + LoadOnly);
                }
                else
                    throw new Exception("Plugin configuration not found");


            }
            catch (Exception Ex)
            {
                logMsg("Exeption loading plugin configuration " + Ex.Message);
            }


        }
        void _adapterSDK_ComStateChangeEvent(string equipmentName, ICommStateInfo commStateInfo)
        {
            Log("Starting _adapterSDK_ComStateChangeEvent...");

            Log("    equipmentName: " + equipmentName);
            Log("    commStateInfo: " + commStateInfo.Connected.ToString());
            Log("    commStateInfo: " + commStateInfo.GetType().ToString());
            Log("    commStateInfo: " + commStateInfo.MultiDSPURLSForEquipment.ToString());
            Log("    commStateInfo: " + commStateInfo.ToolNames[0].ToString());

        }


        void _adapterSDK_DbBatchWriteFailEvent(string equipmentName, IDbBatchWriteFailEventInfo writeFailInfo)
        {
            Log("Starting _adapterSDK_DbBatchWriteFailEvent...");
        }

        void _adapterSDK_EndOfRunEvent(string equipmentName, ISegmentIdInfo segmentIDInfo)
        {
            Log("Starting _adapterSDK_EndOfRunEvent...");
            Log("++++++++ Segment Tool Name: " + segmentIDInfo.ToolName);
            Log("++++++++ Segment ID (RUN_ID): " + segmentIDInfo.SegmentID);
            Log("++++++++ Segment Transaction ID: " + segmentIDInfo.TransactionID);

            _currentLotId = "";



        }

        void _adapterSDK_EqDCPStateChangeEvent(string equipmentName, string ToolName, int planID)
        {
            Log("Starting _adapterSDK_EqDCPStateChangeEvent...");
        }

        void _adapterSDK_StartOfRunEvent(string equipmentName, ISegmentIdInfo segmentIDInfo)
        {
            Log("Starting _adapterSDK_StartOfRunEvent...");
            Log("++++++++ Segment Tool Name: " + segmentIDInfo.ToolName);
            Log("++++++++ Segment ID (RUN_ID): " + segmentIDInfo.SegmentID);
            Log("++++++++ Segment Transaction ID: " + segmentIDInfo.TransactionID);

            _currentRunId = segmentIDInfo.SegmentID;


        }

        void _adapterSDK_ToolCmdEvent(string equipmentName, IToolCommandEvent cmd)
        {
            string sStatus = "FAIL";

            Log("Starting _adapterSDK_ToolCmdEvent Method...");
            Log("    Received Equipment Command with the following parameters:");
            Log("        EqupmentName = " + equipmentName);
            Log("        Command Name =" + cmd.CommandName);
            Log("        Command Tag=" + cmd.TransactionID);

            string sCmdRequestXml = cmd.RequestEventXmlData;
            sCmdRequestXml = sCmdRequestXml.Replace("&lt;", "<");
            sCmdRequestXml = sCmdRequestXml.Replace("&gt;", ">");
            Log("        CmdXml=" + sCmdRequestXml);
            string replyMsg = string.Empty;

            if (_sessionList.Contains(equipmentName))
            {
                switch (cmd.CommandName)
                {
                    case "ImportLiveImplantAndBeamSetupReports":
                        {
                            try
                            {
                                Log("Executing Equipment Command: ImportImplantAndBeamSetupReports");


                                string sEqpName;
                                replyMsg = GetXml(cmd.ReplyEventSchema);

                                numFilesFound = 0;
                                XmlDocument requestDoc = new XmlDocument();
                                requestDoc.LoadXml(sCmdRequestXml);
                                XmlNode node = null;

                                node = requestDoc.SelectSingleNode("/*/EquipmentName");
                                sEqpName = node.InnerText;
                                Log("EquipmentName: " + sEqpName);


                                node = requestDoc.SelectSingleNode("/*/ImplantFileName__ASCII");
                                String ImplantFileName = node.InnerText;
                                Log("Implant FileName " + ImplantFileName);

                                node = requestDoc.SelectSingleNode("/*/ImplantFilesLocation__ASCII");
                                ImplantFilesLocation = node.InnerText;
                                Log("Implant Files Location Location: " + ImplantFilesLocation);

                                node = requestDoc.SelectSingleNode("/*/BeamSetupFilesLocation__ASCII");
                                BeamSetupFilesLocation = node.InnerText;
                                Log("Beam setup files Location: " + BeamSetupFilesLocation);


                                node = requestDoc.SelectSingleNode("/*/WorkingFolder__ASCII");
                                WorkingFolder = node.InnerText;
                                Log("WorkingFolder Location: " + WorkingFolder);

                                node = requestDoc.SelectSingleNode("/*/NetworkName__ASCII");
                                if (node.InnerText.Split(Path.DirectorySeparatorChar).Length == 2)
                                {
                                    NetworkName = node.InnerText.Split(Path.DirectorySeparatorChar)[1];
                                    Domain = node.InnerText.Split(Path.DirectorySeparatorChar)[0];
                                    Log("NetworkName " + NetworkName);
                                    Log("Domain " + Domain);

                                    node = requestDoc.SelectSingleNode("/*/Username__ASCII");
                                    Username = node.InnerText;
                                    Log("Username " + Username);

                                    node = requestDoc.SelectSingleNode("/*/Password__ASCII");
                                    Password = node.InnerText;
                                    Log("Password " + Password);

                                    RemoteNetworkConnection RemoteNetworkConnection = new RemoteNetworkConnection();
                                    NetworkCredential NetWorkCredentionals = new NetworkCredential(Username, Password, Domain);
                                    RemoteNetworkConnection.NetworkConnection(@"\\" + NetworkName, NetWorkCredentionals);
                                }





                                string datetime = DateTime.Now.ToString().Replace('/', '_').Replace(' ', '_').Replace(':', '_');


                                processedFolder = @WorkingFolder + @"\ArchiveDataLoader\ProcessedFiles\" + toolName + @"\" + datetime + @"\";
                                unProcessedFolder = @WorkingFolder + @"\ArchiveDataLoader\UnProcessedFiles\" + toolName + @"\" + datetime + @"\";
                                SignalDataFolder = @WorkingFolder + @"\ArchiveDataLoader\SignalDataFiles\" + toolName + @"\" + datetime + @"\";

                                ProcessedDir = new DirectoryInfo(processedFolder);
                                UnProcessedDir = new DirectoryInfo(unProcessedFolder);
                                SignalDataDir = new DirectoryInfo(SignalDataFolder);

                                if (!ProcessedDir.Exists)
                                    ProcessedDir.Create();
                                else
                                {
                                    ProcessedDir.Delete(true);
                                    ProcessedDir.Create();
                                }

                                if (!UnProcessedDir.Exists)
                                    UnProcessedDir.Create();
                                else
                                {
                                    UnProcessedDir.Delete(true);
                                    UnProcessedDir.Create();
                                }

                                if (!SignalDataDir.Exists)
                                    SignalDataDir.Create();
                                else
                                {
                                    SignalDataDir.Delete(true);
                                    SignalDataDir.Create();
                                }

                                //DirectoryInfo root = new DirectoryInfo(sVCSReportsLocation); // Implants and BeamSetup folders must be directly underneath the root folder(i.e. VCSReports)
                                ImplantsDir = new DirectoryInfo(ImplantFilesLocation + @"\");
                                BeamSetupDir = new DirectoryInfo(BeamSetupFilesLocation + @"\");


                                ImplantDateDirs = ImplantsDir.GetDirectories();
                                BeamSetupDateDirs = BeamSetupDir.GetDirectories();


                                if (!ImplantsDir.Exists)
                                {
                                    Log("CMD Reply Message: " + replyMsg);
                                    handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "ImplantDir " + ImplantsDir.FullName + " does not exist.");
                                    return;

                                }
                                else if (!BeamSetupDir.Exists)
                                {
                                    Log("CMD Reply Message: " + replyMsg);
                                    handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "BeamSetupDir " + BeamSetupDir.FullName + " does not exist");
                                    return;
                                }
                                ImplantsDir = new DirectoryInfo(ImplantFilesLocation + @"\");

                                Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans = GetDCPlans(sEqpName);
                                Dictionary<string, ArrayList> DCPlanDataItems = GetDCPlanDataItems(DCPlans);
                                DataTable ImplantData = new DataTable();

                                ImplantData.Columns.Add("PPID", typeof(string));
                                ImplantData.Columns.Add("CarrierID", typeof(string));
                                ImplantData.Columns.Add("Chamber", typeof(string));
                                ImplantData.Columns.Add("LoadPort", typeof(string));
                                ImplantData.Columns.Add("LoadLock", typeof(string));
                                ImplantData.Columns.Add("LotID", typeof(string));
                                ImplantData.Columns.Add("WaferID", typeof(string));
                                ImplantData.Columns.Add("ImplantID", typeof(string));
                                ImplantData.Columns.Add("Slot", typeof(string));
                                ImplantData.Columns.Add("WaferStartTime", typeof(DateTime));
                                ImplantData.Columns.Add("WaferEndTime", typeof(DateTime));
                                ImplantData.Columns.Add("PausedResumed", typeof(Boolean));
                                ImplantData.Columns.Add("FileName", typeof(string));
                                ImplantData.Columns.Add("StartArchiveRecordNumber", typeof(string));
                                ImplantData.Columns.Add("MiddleArchiveRecordNumber", typeof(string));
                                ImplantData.Columns.Add("EndArchiveRecordNumber", typeof(string));

                                GetImplantData(new DirectoryInfo(ImplantsDir.FullName).FullName, ImplantFileName, ref ImplantData);

                                 DataRow[] query = ImplantData.Select("", "WaferStartTime asc");

                                 if (query.Length > 0)
                                 {
                                     DateTime MinTime = (DateTime)query[0][ImplantData.Columns["WaferStartTime"].Ordinal];                          // set min time to earliest start time
                                     DateTime MaxTime = (DateTime)query[query.Length - 1][ImplantData.Columns["WaferEndTime"].Ordinal];    // set max time to last end time

                                     Log("Number of files found in " + ImplantsDir.FullName + " : " + query.Length.ToString());
                                     Log("First Wafer started at " + MinTime.ToString());
                                     Log("Last Wafer ended at " + MaxTime.ToString());

                                     foreach (DataRow row in query)   // This loop is for each implant file in the current hour folder we've read into ImplantData table.
                                     {
                                         string DateDir = ImplantsDir.Parent + @"\\" + ImplantsDir.Name;
                                         SendImplantAndBeamSetupData(row, DateDir); // For each Implant file in this date directory find the beam setup file that preceeded it, then record for both.
                                     }
                                 }

                                 Log("CMD Reply Message: " + replyMsg);
                                 handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "Completed loading Implant And BeamSetup Reports.");


                            }
                            catch (Exception Ex)
                            {
                                replyMsg = GetXml(cmd.ReplyEventSchema);
                                Log("Exception " + Ex.Message + "Stack Trace " + Ex.StackTrace);
                                sStatus = Ex.Message;
                                handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, sStatus);
                            }
                            break;
                        }
                    case "ImportArchiveData":
                        {
                            try
                            {
                                Log("Executing Equipment Command: ImportArchiveData");


                                string sEqpName;
                                string sStartDateTime;
                                string sEndDateTime;

                                numFilesFound = 0;
                                XmlDocument requestDoc = new XmlDocument();
                                requestDoc.LoadXml(sCmdRequestXml);
                                XmlNode node = null;
                                replyMsg = GetXml(cmd.ReplyEventSchema);


                                node = requestDoc.SelectSingleNode("/*/DataSource__ASCII");
                                DataSource = node.InnerText;
                                Log("DataSource: " + DataSource);

                                node = requestDoc.SelectSingleNode("/*/LoadOption__ASCII");
                                LoadOption = node.InnerText;
                                Log("LoadOption: " + LoadOption);

                                node = requestDoc.SelectSingleNode("/*/EquipmentName");
                                sEqpName = node.InnerText;
                                Log("EquipmentName: " + sEqpName);

                                node = requestDoc.SelectSingleNode("/*/ImplantFilesLocation__ASCII");
                                ImplantFilesLocation = node.InnerText;
                                Log("Implant Files Location Location: " + ImplantFilesLocation);

                                node = requestDoc.SelectSingleNode("/*/BeamSetupFilesLocation__ASCII");
                                BeamSetupFilesLocation = node.InnerText;
                                Log("Beam setup files Location: " + BeamSetupFilesLocation);

                                node = requestDoc.SelectSingleNode("/*/ArchiveFilesLocation__ASCII");
                                ArchiveFilesLocation = node.InnerText;
                                Log("archive files Location: " + ArchiveFilesLocation);

                                node = requestDoc.SelectSingleNode("/*/WorkingFolder__ASCII");
                                WorkingFolder = node.InnerText;
                                Log("WorkingFolder Location: " + WorkingFolder);                               

                                node = requestDoc.SelectSingleNode("/*/StartDateTime__ASCII");
                                sStartDateTime = node.InnerText;
                                Log("Start Date Time: " + sStartDateTime);

                                node = requestDoc.SelectSingleNode("/*/EndDateTime__ASCII");
                                sEndDateTime = node.InnerText;
                                Log("End Date Time: " + sEndDateTime);

                                node = requestDoc.SelectSingleNode("/*/NetworkName__ASCII");
                                if (node.InnerText.Split(Path.DirectorySeparatorChar).Length == 2)
                                {
                                    NetworkName = node.InnerText.Split(Path.DirectorySeparatorChar)[1];
                                    Domain = node.InnerText.Split(Path.DirectorySeparatorChar)[0];
                                    Log("NetworkName " + NetworkName);
                                    Log("Domain " + Domain);
                                    node = requestDoc.SelectSingleNode("/*/Username__ASCII");
                                    Username = node.InnerText;
                                    Log("Username " + Username);

                                    node = requestDoc.SelectSingleNode("/*/Password__ASCII");
                                    Password = node.InnerText;
                                    Log("Password " + Password);
                                    RemoteNetworkConnection RemoteNetworkConnection = new RemoteNetworkConnection();
                                    NetworkCredential NetWorkCredentionals = new NetworkCredential(Username, Password, Domain);
                                    RemoteNetworkConnection.NetworkConnection(@"\\" + NetworkName, NetWorkCredentionals);
                                }


                                ArrayList oProcessEventArrayList = new ArrayList();
                                string datetime = DateTime.Now.ToString().Replace('/', '_').Replace(' ', '_').Replace(':', '_');

                                DirectoryInfo WorkingDir = new DirectoryInfo(WorkingFolder);

                                processedFolder = @WorkingFolder + @"\ArchiveDataLoader\ProcessedFiles\" + toolName + @"\" + datetime + @"\";
                                unProcessedFolder = @WorkingFolder + @"\ArchiveDataLoader\UnProcessedFiles\" + toolName + @"\" + datetime + @"\";
                                SignalDataFolder = @WorkingFolder + @"\ArchiveDataLoader\SignalDataFiles\" + toolName + @"\" + datetime + @"\";

                                // IMPORTANT NOTE:
                                // DataLoadManager and Archive Monitor send start and end times are sent in Local TZ.
                                // DataLoadManager does this because it displays available times in Local TZ.
                                // Archive monitor does this because the archive time creating and completion time stamps 
                                // the time stamps in the implant files are in Local TZ.
                                // We convert them to UTC below to validate timerange and also in GetSignalData because
                                // VCS stores times in the archive files in UTC time zone.

                                DateTime SpecifiedStartDateTimeLocal = Convert.ToDateTime(sStartDateTime);
                                DateTime SpecifiedEndDateTimeLocal = Convert.ToDateTime(sEndDateTime);


                                SpecifiedStartDateTimeUTC = ConvertTimeZone(SpecifiedStartDateTimeLocal, "UTC");
                                SpecifiedEndDateTimeUTC = ConvertTimeZone(SpecifiedEndDateTimeLocal, "UTC");


                                if (DateTime.Compare(SpecifiedStartDateTimeUTC, SpecifiedEndDateTimeUTC) >= 0)
                                {
                                    Log("CMD Reply Message: " + replyMsg);
                                    handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "Specified Start time cannot be greater than the sepcified End time.");
                                    return;

                                }

                                ProcessedDir = new DirectoryInfo(processedFolder);
                                UnProcessedDir = new DirectoryInfo(unProcessedFolder);
                                SignalDataDir = new DirectoryInfo(SignalDataFolder);

                                if (!ProcessedDir.Exists)
                                    ProcessedDir.Create();
                                else
                                {
                                    ProcessedDir.Delete(true);
                                    ProcessedDir.Create();
                                }

                                if (!UnProcessedDir.Exists)
                                    UnProcessedDir.Create();
                                else
                                {
                                    UnProcessedDir.Delete(true);
                                    UnProcessedDir.Create();
                                }

                                if (!SignalDataDir.Exists)
                                    SignalDataDir.Create();
                                else
                                {
                                    SignalDataDir.Delete(true);
                                    SignalDataDir.Create();
                                }

                                //DirectoryInfo root = new DirectoryInfo(sVCSReportsLocation); // Implants and BeamSetup folders must be directly underneath the root folder(i.e. VCSReports)
                                ImplantsDir = new DirectoryInfo(ImplantFilesLocation + @"\");
                                BeamSetupDir = new DirectoryInfo(BeamSetupFilesLocation + @"\");
                                ArchivedDir = new DirectoryInfo(ArchiveFilesLocation + @"\");


                                ImplantDateDirs = ImplantsDir.GetDirectories();
                                BeamSetupDateDirs = BeamSetupDir.GetDirectories();


                                if (!ImplantsDir.Exists)
                                {
                                    Log("CMD Reply Message: " + replyMsg);
                                    handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "ImplantDir " + ImplantsDir.FullName + " does not exist.");
                                    return;

                                }
                                else if (!BeamSetupDir.Exists)
                                {
                                    Log("CMD Reply Message: " + replyMsg);
                                    handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "BeamSetupDir " + BeamSetupDir.FullName + " does not exist");
                                    return;
                                }
                                else if (!ArchivedDir.Exists)
                                {
                                    Log("CMD Reply Message: " + replyMsg);
                                    handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "ArchivedDir " + ArchivedDir.FullName + " does not exist");
                                    return;

                                }

                                ArchiveStartTime = new DateTime();
                                ArchiveEndTime = new DateTime();
                                ArchiveStartMilliseconds = 0;
                                ArchiveEndMilliseconds = 0;
                                if (DataSource == "LCAP")
                                {
                                    DirectoryInfo LocalArchivedDir = new DirectoryInfo(Application.StartupPath + @"\ArchivedFiles\" + toolName);
                                    if (!LocalArchivedDir.Exists)
                                        LocalArchivedDir.Create();

                                    // The Archive Directory for LCAP is configured to be the location of the archives on the tool(i.e. \\172.18.80.82\f$\VCSArchive\V80 07 50 148_StgSet3)
                                    // GetSourcePath returns "VCSArchive\V80 07 50 148_StgSet3" portion of the path
                                    // Archive Monitor constructs the folder to which it copies the archive files in the working folder under E3ArchiveMonitor and the same toolname as the instance of DataLoader.
                                    string ArchiveSourcePath = GetSourcePath(ArchivedDir.FullName);
                                    DirectoryInfo ArchiveMonitorWorkingDir = new DirectoryInfo(WorkingFolder + @"\E3ArchiveMonitor\" + toolName);
                                    DirectoryInfo ArchiveSourceDir = new DirectoryInfo(ArchiveMonitorWorkingDir + ArchiveSourcePath);



                                    FileInfo[] ArchiveFiles = ArchiveSourceDir.GetFiles("*.IanArchive", SearchOption.TopDirectoryOnly).OrderBy(p => p.CreationTime).ToArray();

                                    foreach (FileInfo file in ArchiveFiles)
                                    {

                                        Log("Moving  " + ArchiveSourceDir.FullName + @"\" +  file + " to " + LocalArchivedDir.FullName + @"\" +  file);
                                        File.Move(Path.Combine(ArchiveSourceDir.FullName, file.Name ), Path.Combine(LocalArchivedDir.FullName, file.Name));

                                        while (IsFileLocked(Path.Combine(LocalArchivedDir.FullName, file.Name))) { }
                                    }
                                    // Temporarily set the Global ArchivedDir to the Source dir to copy the IanArchiveInfo file over
                                    ArchivedDir = ArchiveSourceDir;
                                    CopyIanArchiveInfoFile(LocalArchivedDir);
                                    // From Here on, the ArchivedDir will be set to the local folder where we've moved the IanArchive and IanArchiveInfo files over.
                                    ArchivedDir = new DirectoryInfo(LocalArchivedDir.FullName  + @"\");
                                    Log("New Archive Directory is :" + ArchivedDir.FullName);
                                }



                                if (GetTimeRange(ArchivedDir, ref ArchiveStartTime, ref ArchiveStartMilliseconds, ref ArchiveEndTime, ref ArchiveEndMilliseconds))
                                {
                                    if (DateTime.Compare(SpecifiedStartDateTimeUTC, ArchiveStartTime) >= 0 && DateTime.Compare(SpecifiedEndDateTimeUTC, ArchiveEndTime.AddMilliseconds(ArchiveEndMilliseconds)) <= 0)
                                    {
                                        Log("CMD Reply Message: " + replyMsg);
                                        handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "Dataloader has completed initial validations and has begun loading data. Please check tool history report or dataloader log file to confirm data is being loaded.");

                                        InitializeArchiveReader(ArchivedDir);
                                        SelectedSignals = GetSignals(ArchivedDir, DCPlanDataItems);
                                        DetachArchiveReader();

                                        ArchiveStartTime = SpecifiedStartDateTimeUTC;
                                        ArchiveEndTime = SpecifiedEndDateTimeUTC;
                                        ArchiveStartTime = ArchiveStartTime.AddMilliseconds(ArchiveStartMilliseconds);
                                        ArchiveEndTime = ArchiveEndTime.AddMilliseconds(ArchiveEndMilliseconds);
                                        Log("Loading Archive data from Start time:" + ArchiveStartTime.ToString() + " to End  time:" + ArchiveEndTime.ToString());

                                        //writeSignalData();

                                        ProcessedFolders = new DataTable();
                                        ProcessedFolders.Columns.Add("DateDir", typeof(string));
                                        ProcessedFolders.Columns.Add("TimeDir", typeof(string));

                                        InitializeArchiveReader(ArchivedDir);
                                        while (ArchiveStartTime <= ArchiveEndTime)
                                        {
                                            DateTime Tmp_endTime = ArchiveStartTime.AddMinutes(60);
                                            Object objSignalResults = new Object();
                                            DataTable VCSDirs = GetVCSDirs(ArchiveStartTime, Tmp_endTime, ImplantsDir);  // this will get every 'hour' folder that falls with start and end time range.(i.e. 00, 01,..13,14)

                                            if (VCSDirs.Rows.Count > 0)
                                            {
                                                DataRow[] query = VCSDirs.Select("", "");
                                                if (query.Length != 0)
                                                {
                                                    foreach (DataRow row in query)
                                                    {

                                                        DirectoryInfo thisDir = new DirectoryInfo(ImplantsDir.FullName + row[0].ToString() + @"\" + row[1] + @"\");
                                                        string selectionCriteria = "DateDir =  '" + row[0].ToString() + "' and " + "TimeDir = '" + row[1].ToString() + "'";
                                                        DataRow[] ProcessedTimeDirs = ProcessedFolders.Select(selectionCriteria, "");
                                                        if (ProcessedTimeDirs.Length <= 0)
                                                        {
                                                            ProcessDirectory(row[0].ToString() + @"\" + row[1] + @"\");
                                                            ProcessedFolders.Rows.Add(row[0].ToString(), row[1].ToString());
                                                        }
                                                    }
                                                }
                                            }
                                            ArchiveStartTime = Tmp_endTime;
                                            VCSDirs.Clear();
                                        }
                                        DetachArchiveReader();
                                    }
                                    else
                                    {
                                        Log("Given Start/end  time does not fall within archive star/end time");
                                        Log("CMD Reply Message: " + replyMsg);
                                        handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "Given Start/end  time does not fall within archive star/end time");
                                    }
                                }
                                else
                                {
                                    replyMsg = GetXml(cmd.ReplyEventSchema);
                                    Log("CMD Reply Message: " + replyMsg);
                                    handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, "Error Getting info from .IanArchiveInfoFile.");

                                }
                            }
                            catch (Exception Ex)
                            {
                                Log("Exception " + Ex.Message + "Stack Trace " + Ex.StackTrace);
                                replyMsg = GetXml(cmd.ReplyEventSchema);
                                Log("CMD Reply Message: " + replyMsg);

                                handleCmdReply(equipmentName, cmd.TransactionID, cmd.ReplyEventId.ToString(), replyMsg, Ex.Message);
                            }
                            break;
                        }
                    default:
                        {
                            Log(" Unknown Command is received " + cmd.CommandName);
                            break;
                        }

                }

                Log(" Completed processing" + cmd.CommandName);
            }
            else
            {
                Log("        Equipment Command is not processed, There is no active session for EquipmentName=" + equipmentName);
            }
            try
            {
                if (DataSource == "LCAP")
                {
                    DirectoryInfo LocalArchivedDir = new DirectoryInfo(Application.StartupPath + @"\ArchivedFiles\" + toolName  );
                    if (LocalArchivedDir.Exists)
                        LocalArchivedDir.Delete(true);
                }
            }
            catch (Exception Ex1)
            {
                Log("Exception Cleaning up " + Ex1.Message);
                SendDataLoaderIdle(toolName, toolName + "_DataLoaderReport", DateTime.Now);
            }
            SendDataLoaderIdle(toolName, toolName + "_DataLoaderReport", DateTime.Now);

        }

        private static string GetSourcePath(string SourceDir)
        {

            string IPRegex = "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}";
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(IPRegex, options);
            string SourcePath = regex.Replace(SourceDir, "");
            regex = new Regex(@"\D\$", options);
            SourcePath = regex.Replace(SourcePath, "");
            regex = new Regex(@"\D\:", options);
            SourcePath = regex.Replace(SourcePath, "");
            regex = new Regex(@"\\{2,}", options);
            SourcePath = regex.Replace(SourcePath, @"\");

            return SourcePath;

        }

        private static Boolean IsFileLocked(String FileName)
        {
            try
            {
                if (File.Exists(FileName))
                {
                    FileStream stream = null;
                    try
                    {
                        stream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    }
                    catch (Exception Ex)
                    {
                        int ErrorCode = Marshal.GetHRForException(Ex) & ((1 << 16) - 1);
                        if ((Ex is IOException) && (ErrorCode == 32 || ErrorCode == 33))
                        {
                            Log("File " + FileName + " is locked");
                            return true;
                        }
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Close();
                    }
                }
            }
            catch (Exception Ex)
            {
                Log("Exception in IsFileLocked " + Ex.Message);
            }
            return false;
        }

        
        private static void CopyIanArchiveInfoFile(DirectoryInfo DestinationDir)
        {
            try
            {
                FileInfo[] files;
                files = ArchivedDir.GetFiles("*.IanArchiveInfo", SearchOption.AllDirectories);

                if (files.Length != 0)
                {

                    string filename = ArchivedDir.FullName.ToString() + files[0].Name.ToString();

                    string SourceFile = Path.Combine(ArchivedDir.FullName, files[0].Name.ToString());
                    string DestinationFile = Path.Combine(DestinationDir.FullName, files[0].Name.ToString());

                    Log("Copying " + SourceFile + " to " + DestinationFile);

                    File.Copy(SourceFile, DestinationFile, true);

                    while (IsFileLocked(DestinationFile)) { }
                }
            }
            catch (Exception Ex)
            {
                Log("Exception in CopyIanArchiveInfoFile " + Ex.Message);
            }
        }

        private static void writeSignalData(string Data)
        {
            Byte[] output = new UTF8Encoding(true).GetBytes(Data + "\n");

            fSignalData.Write(output, 0, output.Length);
        }

        private static void writeContextData(string Data)
        {
            Byte[] output = new UTF8Encoding(true).GetBytes(Data + "\n");

            fContextData.Write(output, 0, output.Length);
        }
        private static void InitDataTables()
        {






        }

        private static Boolean FileExists(string FileName)
        {
            Boolean bFileExists = false;

            if (!File.Exists(FileName))
            {
                FileStream foutput = File.OpenWrite(FileName);
                Byte[] output = new UTF8Encoding(true).GetBytes("PPID,CarrierID,LotID,WaferID,ImplantID,Wafer Start Time, Wafer End Time, Paused/Resumed,FileName,ToolName,StartRecordNumber,EndRecordNumber,Archive Data Found" + "\n");
                foutput.Write(output, 0, output.Length);
                foutput.Close();

            }

            return bFileExists;

        }

        private static void GetContextData(DirectoryInfo Datedir, string EqpName, Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans, Dictionary<string, ArrayList> DCPlanDataItems)
        {
            string functionName = "GetContextData";
            Log(functionName + ": Start to get GetContextData in " + Datedir.Name.ToString());

            DataTable ImplantData = new DataTable();

            try
            {
                ImplantData.Columns.Add("PPID", typeof(string));
                ImplantData.Columns.Add("CarrierID", typeof(string));
                ImplantData.Columns.Add("Chamber", typeof(string));
                ImplantData.Columns.Add("LoadPort", typeof(string));
                ImplantData.Columns.Add("LoadLock", typeof(string));
                ImplantData.Columns.Add("LotID", typeof(string));
                ImplantData.Columns.Add("WaferID", typeof(string));
                ImplantData.Columns.Add("ImplantID", typeof(string));
                ImplantData.Columns.Add("WaferStartTime", typeof(DateTime));
                ImplantData.Columns.Add("WaferEndTime", typeof(DateTime));
                ImplantData.Columns.Add("PausedResumed", typeof(Boolean));
                ImplantData.Columns.Add("FileName", typeof(string));
                ImplantData.Columns.Add("StartArchiveRecordNumber", typeof(string));
                ImplantData.Columns.Add("MiddleArchiveRecordNumber", typeof(string));
                ImplantData.Columns.Add("EndArchiveRecordNumber", typeof(string));

                GetImplantData(Datedir.FullName, ref ImplantData);
                //GetBeamSetupData(Datedir.FullName.Replace("Implants", "BeamSetup"), ref ImplantData);
                DataRow[] query = ImplantData.Select("", "WaferStartTime asc");

                if (query.Length > 0)
                {
                    Log(functionName + ": Number of files found  " + query.Length.ToString());

                    foreach (DataRow row in query)   // This loop is for each implant file we've read into ImplantData table.
                    {
                        Int64 StartArchiveRecordNumber = Convert.ToInt64(row[ImplantData.Columns["StartArchiveRecordNumber"].Ordinal].ToString());
                        Int64 MiddleArchiveRecordNumber = Convert.ToInt64(row[ImplantData.Columns["MiddleArchiveRecordNumber"].Ordinal].ToString());
                        Int64 EndArchiveRecordNumber = Convert.ToInt64(row[ImplantData.Columns["EndArchiveRecordNumber"].Ordinal].ToString());
                        string WaferID = row[ImplantData.Columns["WaferID"].Ordinal].ToString();
                        string ImplantID = row[ImplantData.Columns["ImplantID"].Ordinal].ToString();
                        string PPID = row[ImplantData.Columns["PPID"].Ordinal].ToString();
                        string LotID = row[ImplantData.Columns["LotID"].Ordinal].ToString();
                        string CarrierID = row[ImplantData.Columns["CarrierID"].Ordinal].ToString();
                        string FileName = row[ImplantData.Columns["FileName"].Ordinal].ToString();
                        string Chamber = row[ImplantData.Columns["Chamber"].Ordinal].ToString();
                        string LoadPort = row[ImplantData.Columns["LoadPort"].Ordinal].ToString();
                        string LoadLock = row[ImplantData.Columns["LoadLock"].Ordinal].ToString();
                        DateTime WaferImplantStartTime = (DateTime)row[ImplantData.Columns["WaferStartTime"].Ordinal];
                        DateTime WaferImplantEndTime = (DateTime)row[ImplantData.Columns["WaferEndTime"].Ordinal];
                        //string Header = "ToolID,PPID,WaferID,ImplantID,LotID,CarrierID,Chamber,LoadPort,LoadLock,StartArchiveRecordNumber,MiddleArchiveRecordNumberEndArchiveRecordNumber,WaferImplantStartTime,WaferImplantEndTime";
                        string thisLine = EqpName + "," + PPID + "," + WaferID + "," + ImplantID + "," + LotID + "," + CarrierID + "," + Chamber + "," + LoadPort + "," + LoadLock;
                        thisLine += "," + StartArchiveRecordNumber.ToString() + "," + MiddleArchiveRecordNumber.ToString() + "," + EndArchiveRecordNumber.ToString() + "," + row[ImplantData.Columns["WaferStartTime"].Ordinal] + "," + row[ImplantData.Columns["WaferEndTime"].Ordinal] + "\n";
                        writeContextData(thisLine);
                    }
                }
            }
            catch (Exception Ex)
            {
                Log("Error in GetContextData  " + Ex.Message);
            }
            ImplantData.Clear();
        }
        private static void SendImplantAndBeamSetupData(DataRow row, string DateDir)
        {
            string functionName = "SendImplantAndBeamSetupData";
            Log(functionName + ": SendImplantAndBeamSetupData " + DateDir);

            DataTable ImplantData = new DataTable();
            ImplantData.Columns.Add("PPID", typeof(string));
            ImplantData.Columns.Add("CarrierID", typeof(string));
            ImplantData.Columns.Add("Chamber", typeof(string));
            ImplantData.Columns.Add("LoadPort", typeof(string));
            ImplantData.Columns.Add("LoadLock", typeof(string));
            ImplantData.Columns.Add("LotID", typeof(string));
            ImplantData.Columns.Add("WaferID", typeof(string));
            ImplantData.Columns.Add("ImplantID", typeof(string));
            ImplantData.Columns.Add("Slot", typeof(string));

            ImplantData.Columns.Add("WaferStartTime", typeof(DateTime));
            ImplantData.Columns.Add("WaferEndTime", typeof(DateTime));
            ImplantData.Columns.Add("PausedResumed", typeof(Boolean));
            ImplantData.Columns.Add("FileName", typeof(string));
            ImplantData.Columns.Add("StartArchiveRecordNumber", typeof(string));
            ImplantData.Columns.Add("MiddleArchiveRecordNumber", typeof(string));
            ImplantData.Columns.Add("EndArchiveRecordNumber", typeof(string));
            try
            {
                // Get Implant file info : This reads the header information from implant file.
                cFileInfo ImplantFileInfo = new cFileInfo();
                ImplantFileInfo = GetImplantFileInfo(row[ImplantData.Columns["FileName"].Ordinal].ToString());

                ImplantData.Rows.Add(ImplantFileInfo.PPID, ImplantFileInfo.CarrierID, ImplantFileInfo.Chamber, ImplantFileInfo.LoadPort,
                                        ImplantFileInfo.LoadLock, ImplantFileInfo.LotID, ImplantFileInfo.WaferID, ImplantFileInfo.ImplantID, ImplantFileInfo.Slot,
                                        ImplantFileInfo.WaferStartTime, ImplantFileInfo.WaferEndTime, ImplantFileInfo.Resumed, ImplantFileInfo.FileName,
                                        ImplantFileInfo.StartArchiveRecordNumber, ImplantFileInfo.MiddleArchiveRecordNumber, ImplantFileInfo.EndArchiveRecordNumber);
          

                DataTable BeamSetupData = GetBeamsetupData(ImplantData,DateDir);

                SendBeamSetupData(BeamSetupData);

                SendImplantData(ImplantData);
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " : " + Ex.Message);
            }
        }
        private static DataTable GetBeamsetupData(DataTable ImplantData, string DateDir)
        {
            string functionName = "GetBeamsetupData";
            Log(functionName + ": GetBeamsetupData " + DateDir);

            DataTable BeamSetupData = new DataTable();
            BeamSetupData.Columns.Add("PPID", typeof(string));
            BeamSetupData.Columns.Add("CarrierID", typeof(string));
            BeamSetupData.Columns.Add("Chamber", typeof(string));
            BeamSetupData.Columns.Add("LoadPort", typeof(string));
            BeamSetupData.Columns.Add("LoadLock", typeof(string));
            BeamSetupData.Columns.Add("LotID", typeof(string));
            BeamSetupData.Columns.Add("WaferID", typeof(string));
            BeamSetupData.Columns.Add("ImplantID", typeof(string));
            BeamSetupData.Columns.Add("Slot", typeof(string));
            BeamSetupData.Columns.Add("BeamSetupID ", typeof(string));
            BeamSetupData.Columns.Add("SetupResult", typeof(string));
            BeamSetupData.Columns.Add("BeamSetupType", typeof(string));

            BeamSetupData.Columns.Add("BeamEnergy", typeof(string));
            BeamSetupData.Columns.Add("WaferStartTime", typeof(DateTime));
            BeamSetupData.Columns.Add("WaferEndTime", typeof(DateTime));
            BeamSetupData.Columns.Add("FileName", typeof(string));
            BeamSetupData.Columns.Add("Specie", typeof(string));
            try
            {
                string DirName = new DirectoryInfo(BeamSetupDir.FullName + DateDir).FullName;  //Point to beamsetup dir for the same day/time directory as Implant file's day/time directory
                if (Directory.Exists(DirName))
                {

                    DataRow[] query = ImplantData.Select("", "WaferStartTime asc");
                    if (query.Length == 1)
                    {
                        DateTime MinTime = (DateTime)query[0][ImplantData.Columns["WaferStartTime"].Ordinal];                          // set min time to earliest start time
                        DateTime MaxTime = (DateTime)query[ImplantData.Rows.Count - 1][ImplantData.Columns["WaferEndTime"].Ordinal];    // set max time to last end time

                        //DirectoryInfo BeamSetupTimeDirs = new DirectoryInfo(DirName + "\\");
                        ArrayList BeamSetupFiles = new ArrayList();
                        BeamSetupFiles = GetFileList(DirName);

                        foreach (FileInfo BeamSetupFile in BeamSetupFiles)
                        {
                            cFileInfo BeamSetupFileInfo = new cFileInfo();
                            BeamSetupFileInfo = GetBeamSetupFileInfo(BeamSetupFile.FullName);


                            if (DateTime.Compare(BeamSetupFileInfo.WaferEndTime, MinTime) <= 0 && BeamSetupFileInfo.SetupResult == "Success")
                            {
                                BeamSetupFileInfo.CarrierID = query[0][ImplantData.Columns["CarrierID"].Ordinal].ToString();
                                BeamSetupFileInfo.Chamber = query[0][ImplantData.Columns["Chamber"].Ordinal].ToString();
                                BeamSetupFileInfo.LoadPort = query[0][ImplantData.Columns["LoadPort"].Ordinal].ToString();
                                BeamSetupFileInfo.LoadLock = query[0][ImplantData.Columns["LoadLock"].Ordinal].ToString();
                                BeamSetupFileInfo.LotID = query[0][ImplantData.Columns["LotID"].Ordinal].ToString();
                                BeamSetupFileInfo.WaferID = query[0][ImplantData.Columns["WaferID"].Ordinal].ToString();
                                BeamSetupFileInfo.ImplantID = query[0][ImplantData.Columns["ImplantID"].Ordinal].ToString();
                                BeamSetupFileInfo.Slot = query[0][ImplantData.Columns["Slot"].Ordinal].ToString();

                                BeamSetupData.Rows.Add(BeamSetupFileInfo.PPID, BeamSetupFileInfo.CarrierID, BeamSetupFileInfo.Chamber,
                                    BeamSetupFileInfo.LoadPort, BeamSetupFileInfo.LoadLock, BeamSetupFileInfo.LotID, BeamSetupFileInfo.WaferID,
                                    BeamSetupFileInfo.ImplantID, BeamSetupFileInfo.Slot, BeamSetupFileInfo.BeamSetupID,
                                    BeamSetupFileInfo.SetupResult, BeamSetupFileInfo.BeamSetupType,

                                    BeamSetupFileInfo.BeamEnergy, BeamSetupFileInfo.WaferStartTime, BeamSetupFileInfo.WaferEndTime, BeamSetupFileInfo.FileName, BeamSetupFileInfo.Specie);
                            }
                        }
                        if (BeamSetupData.Rows.Count == 0) // If beamsetup file for current hour was not found, try the previous hour
                        {
                            string[] DatePath = DateDir.Split(Path.DirectorySeparatorChar);
                            int Year = Convert.ToInt16(DatePath[0].Split('-')[0]);
                            int Month = Convert.ToInt16(DatePath[0].Split('-')[1]);
                            int Day = Convert.ToInt16(DatePath[0].Split('-')[2]);


                            DateTime ThisDay = new DateTime(Year, Month, Day);

                            String Time = DatePath[1];

                            if (Time == "00") // if this is the first  hour of the new day, fetch  beamsetups from previous day
                            {
                                String PreviousDay = ThisDay.AddDays(-1).ToString("yyyy-MM-dd");
                                String PreviousHour = "23";
                                DateDir = PreviousDay + @Path.DirectorySeparatorChar + PreviousHour;
                            }
                            else
                            {
                                DateDir = ThisDay.ToString("yyyy-MM-dd") + @Path.DirectorySeparatorChar + ((Convert.ToInt16(Time) - 1).ToString().Length == 1 ? "0" + (Convert.ToInt16(Time) - 1).ToString() : (Convert.ToInt16(Time) - 1).ToString());
                            }

                            DirName = new DirectoryInfo(BeamSetupDir.FullName + DateDir).FullName;
                            if (Directory.Exists(DirName))
                            {
                                BeamSetupFiles = GetFileList(DirName);

                                foreach (FileInfo BeamSetupFile in BeamSetupFiles)
                                {
                                    cFileInfo BeamSetupFileInfo = new cFileInfo();
                                    BeamSetupFileInfo = GetBeamSetupFileInfo(BeamSetupFile.FullName);


                                    if (DateTime.Compare(BeamSetupFileInfo.WaferEndTime, MinTime) <= 0 && BeamSetupFileInfo.SetupResult == "Success")
                                    {
                                        BeamSetupFileInfo.CarrierID = query[0][ImplantData.Columns["CarrierID"].Ordinal].ToString();
                                        BeamSetupFileInfo.Chamber = query[0][ImplantData.Columns["Chamber"].Ordinal].ToString();
                                        BeamSetupFileInfo.LoadPort = query[0][ImplantData.Columns["LoadPort"].Ordinal].ToString();
                                        BeamSetupFileInfo.LoadLock = query[0][ImplantData.Columns["LoadLock"].Ordinal].ToString();
                                        BeamSetupFileInfo.LotID = query[0][ImplantData.Columns["LotID"].Ordinal].ToString();
                                        BeamSetupFileInfo.WaferID = query[0][ImplantData.Columns["WaferID"].Ordinal].ToString();
                                        BeamSetupFileInfo.ImplantID = query[0][ImplantData.Columns["ImplantID"].Ordinal].ToString();
                                        BeamSetupFileInfo.Slot = query[0][ImplantData.Columns["Slot"].Ordinal].ToString();

                                        BeamSetupData.Rows.Add(BeamSetupFileInfo.PPID, BeamSetupFileInfo.CarrierID, BeamSetupFileInfo.Chamber,
                                     BeamSetupFileInfo.LoadPort, BeamSetupFileInfo.LoadLock, BeamSetupFileInfo.LotID, BeamSetupFileInfo.WaferID,
                                     BeamSetupFileInfo.ImplantID, BeamSetupFileInfo.Slot, BeamSetupFileInfo.BeamSetupID,
                                     BeamSetupFileInfo.SetupResult, BeamSetupFileInfo.BeamSetupType,

                                     BeamSetupFileInfo.BeamEnergy, BeamSetupFileInfo.WaferStartTime, BeamSetupFileInfo.WaferEndTime, BeamSetupFileInfo.FileName, BeamSetupFileInfo.Specie);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Log(functionName);
                    }
                }
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " : " + Ex.Message);
            }

            return BeamSetupData;
        }
        private static void SendImplantData(DataTable ImplantData)
        {
            string functionName = "SendImplantData";
            Log(functionName);

            try
            {

                DataRow[] ImplantDataQuery = ImplantData.Select("", "WaferStartTime asc");
                if (ImplantDataQuery.Length > 0)
                {
                    foreach (DataRow ImpantDataRow in ImplantDataQuery)   // This loop is for each implant file in the current hour folder we've read into ImplantData table.
                    {
                        string PPID = ImpantDataRow[ImplantData.Columns["PPID"].Ordinal].ToString();
                        string CarrierID = ImpantDataRow[ImplantData.Columns["CarrierID"].Ordinal].ToString();
                        string Chamber = ImpantDataRow[ImplantData.Columns["Chamber"].Ordinal].ToString();
                        string LoadPort = ImpantDataRow[ImplantData.Columns["LoadPort"].Ordinal].ToString();
                        string LoadLock = ImpantDataRow[ImplantData.Columns["LoadLock"].Ordinal].ToString();
                        string LotID = ImpantDataRow[ImplantData.Columns["LotID"].Ordinal].ToString();
                        string WaferID = ImpantDataRow[ImplantData.Columns["WaferID"].Ordinal].ToString();
                        string ImplantID = ImpantDataRow[ImplantData.Columns["ImplantID"].Ordinal].ToString();
                        string Slot = ImpantDataRow[ImplantData.Columns["Slot"].Ordinal].ToString();

                        DateTime WaferImplantStartTime = (DateTime)ImpantDataRow[ImplantData.Columns["WaferStartTime"].Ordinal];
                        DateTime WaferImplantEndTime = (DateTime)ImpantDataRow[ImplantData.Columns["WaferEndTime"].Ordinal];
                        string FileName = ImpantDataRow[ImplantData.Columns["FileName"].Ordinal].ToString();

                        Dictionary<string, string> ContextVariableNameValue = new Dictionary<string, string>();
                        ContextVariableNameValue.Add("ToolID", toolName);
                        ContextVariableNameValue.Add("PPID", PPID);
                        ContextVariableNameValue.Add("CarrierID", CarrierID);
                        ContextVariableNameValue.Add("Chamber", Chamber);
                        ContextVariableNameValue.Add("LoadPort", LoadPort);
                        ContextVariableNameValue.Add("LoadLock", LoadLock);
                        ContextVariableNameValue.Add("LotID", LotID);
                        ContextVariableNameValue.Add("WaferID", WaferID);
                        ContextVariableNameValue.Add("ImplantID", ImplantID);
                        ContextVariableNameValue.Add("Slot", Slot);

                        SendStartOfScan(toolName, toolName + "_ImplantReport", DCPlans, WaferImplantStartTime, ContextVariableNameValue);

                        Dictionary<string, string> ParameterNameValue = ReadDataFile(ImpantDataRow,"Implant");
                        ParameterNameValue.Add("WaferImplantStartTime", WaferImplantStartTime.ToString());
                        ParameterNameValue.Add("WaferImplantEndTime", WaferImplantEndTime.ToString());
                        ParameterNameValue.Add("FileName", FileName);
                        SendData(toolName, toolName + "_ImplantReport", DCPlans, WaferImplantStartTime, ContextVariableNameValue, ParameterNameValue);

                        SendEndOfScan(toolName, toolName + "_ImplantReport", DCPlans, WaferImplantEndTime, ContextVariableNameValue);
                        Log(functionName);
                    }
                }

            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " : " + Ex.Message);
            }

        }
        private static void SendBeamSetupData(DataTable BeamSetupData)
        {
            string functionName = "SendBeamSetupData";
            Log(functionName);
            try
            {
                //BeamSetupData contains data for ALL BeamSetup files that were found. we are interested only in the latest one.
                DataRow[] BeamSetupQuery = BeamSetupData.Select("", "WaferEndTime desc");
                if (BeamSetupQuery.Length > 0)
                {

                    foreach (DataRow BeamSetupRow in BeamSetupQuery)   // This loop is for each implant file in the current hour folder we've read into ImplantData table.
                    {
                        string PPID = BeamSetupRow[BeamSetupData.Columns["PPID"].Ordinal].ToString();
                        string CarrierID = BeamSetupRow[BeamSetupData.Columns["CarrierID"].Ordinal].ToString();
                        string Chamber = BeamSetupRow[BeamSetupData.Columns["Chamber"].Ordinal].ToString();
                        string LoadPort = BeamSetupRow[BeamSetupData.Columns["LoadPort"].Ordinal].ToString();
                        string LoadLock = BeamSetupRow[BeamSetupData.Columns["LoadLock"].Ordinal].ToString();
                        string LotID = BeamSetupRow[BeamSetupData.Columns["LotID"].Ordinal].ToString();
                        string WaferID = BeamSetupRow[BeamSetupData.Columns["WaferID"].Ordinal].ToString();
                        string ImplantID = BeamSetupRow[BeamSetupData.Columns["ImplantID"].Ordinal].ToString();
                        string Slot = BeamSetupRow[BeamSetupData.Columns["Slot"].Ordinal].ToString();
                        string BeamSetupID = BeamSetupRow[BeamSetupData.Columns["BeamSetupID "].Ordinal].ToString();
                        string SetupResult = BeamSetupRow[BeamSetupData.Columns["SetupResult"].Ordinal].ToString();
                        string BeamSetupType = BeamSetupRow[BeamSetupData.Columns["BeamSetupType"].Ordinal].ToString();

                        string BeamEnergy = BeamSetupRow[BeamSetupData.Columns["BeamEnergy"].Ordinal].ToString();
                        DateTime WaferImplantStartTime = (DateTime)BeamSetupRow[BeamSetupData.Columns["WaferStartTime"].Ordinal];
                        DateTime WaferImplantEndTime = (DateTime)BeamSetupRow[BeamSetupData.Columns["WaferEndTime"].Ordinal];
                        string FileName = BeamSetupRow[BeamSetupData.Columns["FileName"].Ordinal].ToString();
                        string Specie = BeamSetupRow[BeamSetupData.Columns["Specie"].Ordinal].ToString();


                        Dictionary<string, string> ContextVariableNameValue = new Dictionary<string, string>();
                        ContextVariableNameValue.Add("ToolID", toolName);
                        ContextVariableNameValue.Add("PPID", PPID);
                        ContextVariableNameValue.Add("CarrierID", CarrierID);
                        ContextVariableNameValue.Add("Chamber", Chamber);
                        ContextVariableNameValue.Add("LoadPort", LoadPort);
                        ContextVariableNameValue.Add("LoadLock", LoadLock);
                        ContextVariableNameValue.Add("LotID", LotID);
                        ContextVariableNameValue.Add("WaferID", WaferID);
                        ContextVariableNameValue.Add("ImplantID", ImplantID);
                        ContextVariableNameValue.Add("Slot", Slot);
                        ContextVariableNameValue.Add("BeamSetupID", BeamSetupID);
                        ContextVariableNameValue.Add("SetupResult", SetupResult);
                        ContextVariableNameValue.Add("BeamSetupType", BeamSetupType);
                        ContextVariableNameValue.Add("Specie", BeamSetupType);

                        SendStartOfScan(toolName, toolName + "_BeamSetupReport", DCPlans, WaferImplantStartTime, ContextVariableNameValue);
                        Dictionary<string, string> ParameterNameValue = ReadDataFile(BeamSetupRow,"BeamSetup");
                        ParameterNameValue.Add("BeamEnergy", BeamEnergy);
                        ParameterNameValue.Add("WaferImplantStartTime", WaferImplantStartTime.ToString());
                        ParameterNameValue.Add("WaferImplantEndTime", WaferImplantEndTime.ToString());
                        ParameterNameValue.Add("FileName", FileName);
                        SendData(toolName, toolName + "_BeamSetupReport", DCPlans, WaferImplantStartTime, ContextVariableNameValue, ParameterNameValue);

                        SendEndOfScan(toolName, toolName + "_BeamSetupReport", DCPlans, WaferImplantEndTime, ContextVariableNameValue);
                        Log("SendBeamSetupData: " + FileName);
                        break; // Process only the 1st record(which is the one we want!)
                    }
                }
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " : " + Ex.Message);
            }

        }

        private static void ProcessDirectory(string DateDir)
        {
            string functionName = "ProcessDirectory";
            Log(functionName + ": Start to get ProcessDirectory in " + DateDir);
            string GetSignalDataStatus;

            DataTable ImplantData = new DataTable();
            DataTable DataLoaderReport = new DataTable();


            try
            {
                if (DCPlanDataItems.ContainsKey(toolName + "_DataLoaderReport"))
                {
                    DataLoaderReport.Columns.Add("DateTime", typeof(DateTime));
                    DataLoaderReport.Columns.Add("ModuleName", typeof(string));
                    DataLoaderReport.Columns.Add("FileName", typeof(string));
                    DataLoaderReport.Columns.Add("LotID", typeof(string));
                    DataLoaderReport.Columns.Add("WaferID", typeof(string));
                    DataLoaderReport.Columns.Add("WaferStartTime", typeof(string));
                    DataLoaderReport.Columns.Add("WaferEndTime", typeof(string));
                    DataLoaderReport.Columns.Add("StartRecordNumber", typeof(string));
                    DataLoaderReport.Columns.Add("EndRecordNumber", typeof(string));
                    DataLoaderReport.Columns.Add("ReasonNotFound", typeof(string));
                    DataLoaderReport.Columns.Add("Found", typeof(string));
                    DataLoaderReport.Columns.Add("NumberOfSensors", typeof(string));
                    DataLoaderReport.Columns.Add("NumberOfRecords", typeof(string));
                    DataLoaderReport.Columns.Add("TimeDataSentToE3", typeof(DateTime));
                    DataLoaderReport.Columns.Add("TimeToSendWaferData", typeof(string));
                    DataLoaderReport.Columns.Add("ImplantTime", typeof(string));
                    DataLoaderReport.Columns.Add("Slot", typeof(string));
                    DataLoaderReport.Columns.Add("HostName", typeof(string));
                    DataLoaderReport.Columns.Add("ImplantID", typeof(string));

                }

                ImplantData.Columns.Add("PPID", typeof(string));
                ImplantData.Columns.Add("CarrierID", typeof(string));
                ImplantData.Columns.Add("Chamber", typeof(string));
                ImplantData.Columns.Add("LoadPort", typeof(string));
                ImplantData.Columns.Add("LoadLock", typeof(string));
                ImplantData.Columns.Add("LotID", typeof(string));
                ImplantData.Columns.Add("WaferID", typeof(string));
                ImplantData.Columns.Add("ImplantID", typeof(string));
                ImplantData.Columns.Add("Slot", typeof(string));
                ImplantData.Columns.Add("WaferStartTime", typeof(DateTime));
                ImplantData.Columns.Add("WaferEndTime", typeof(DateTime));
                ImplantData.Columns.Add("PausedResumed", typeof(Boolean));
                ImplantData.Columns.Add("FileName", typeof(string));
                ImplantData.Columns.Add("StartArchiveRecordNumber", typeof(string));
                ImplantData.Columns.Add("MiddleArchiveRecordNumber", typeof(string));
                ImplantData.Columns.Add("EndArchiveRecordNumber", typeof(string));

                GetImplantData(new DirectoryInfo(ImplantsDir.FullName + DateDir).FullName, ref ImplantData);

                

                DataRow[] query = ImplantData.Select("", "WaferStartTime asc");

                if (query.Length > 0)
                {
                    DateTime MinTime = (DateTime)query[0][ImplantData.Columns["WaferStartTime"].Ordinal];                          // set min time to earliest start time
                    DateTime MaxTime = (DateTime)query[query.Length - 1][ImplantData.Columns["WaferEndTime"].Ordinal];    // set max time to last end time

                    Log(functionName + ": Number of files found in " +  DateDir + " : " + query.Length.ToString());
                    Log(functionName + ": First Wafer started at " + MinTime.ToString());
                    Log(functionName + ": Last Wafer ended at " + MaxTime.ToString());

                    foreach (DataRow row in query)   // This loop is for each implant file in the current hour folder we've read into ImplantData table.
                    {

                        DateTime WaferImplantStartTime = (DateTime)row[ImplantData.Columns["WaferStartTime"].Ordinal];
                        DateTime WaferImplantEndTime = (DateTime)row[ImplantData.Columns["WaferEndTime"].Ordinal];

                        DateTime WaferImplantStartTimeUTC = ConvertTimeZone(WaferImplantStartTime, "UTC");
                        DateTime WaferImplantEndTimeUTC = ConvertTimeZone(WaferImplantEndTime, "UTC");

                        DateTime SpecifiedStartDateTimeLocal = ConvertTimeZone(SpecifiedStartDateTimeUTC, "LOCAL");
                        DateTime SpecifiedEndDateTimeLocal = ConvertTimeZone(SpecifiedEndDateTimeUTC, "LOCAL");

                        Log(functionName + ": Wafer Implant Start Time(Local) " + WaferImplantStartTime.ToString());
                        Log(functionName + ": Wafer Implant End Time(local) " + WaferImplantEndTime.ToString());
                        Log(functionName + " ");
                        Log(functionName + ": Specified Start Time(Local) " + SpecifiedStartDateTimeLocal.ToString());
                        Log(functionName + ": Specified End Time(Local) " + SpecifiedEndDateTimeLocal.ToString());

                        if (DateTime.Compare(WaferImplantStartTimeUTC, SpecifiedStartDateTimeUTC) >= 0 && DateTime.Compare(WaferImplantEndTimeUTC, SpecifiedEndDateTimeUTC) <= 0)
                        {
                            Stopwatch RunProcessingTimer = new Stopwatch();
                            RunProcessingTimer.Start();
                            Dictionary<string, string> DLR_ContextVariableNameValue = new Dictionary<string, string>();
                            if (DCPUpdated)
                            {
                                SelectedSignals = GetSignals(ArchivedDir, DCPlanDataItems);
                                Log(functionName + " Number of signals found: " + SelectedSignals.Length.ToString());
                                DCPUpdated = false;
                            }
                            if ((LoadOption == "ALL" || LoadOption == "ImplantAndBeamSetup"))
                            {
                                SendImplantAndBeamSetupData(row, DateDir); // For each Implant file in this date directory find the beam setup file that preceeded it, then record for both.
                            }

                            Int64 StartArchiveRecordNumber;
                            Int64 EndArchiveRecordNumber;
                            try
                            {
                                StartArchiveRecordNumber = Convert.ToInt64(row[ImplantData.Columns["StartArchiveRecordNumber"].Ordinal].ToString());
                                EndArchiveRecordNumber = Convert.ToInt64(row[ImplantData.Columns["EndArchiveRecordNumber"].Ordinal].ToString());
                            }
                            catch (Exception)
                            {
                                StartArchiveRecordNumber = 0;
                                EndArchiveRecordNumber = 0;
                                Log(functionName + ": error in parsing start and/or end record number. recordnumber must be an integer.");
                                Log(functionName + ": Start Record number found was: " + row[ImplantData.Columns["StartArchiveRecordNumber"].Ordinal].ToString());
                                Log(functionName + ": End Record number found was: " + row[ImplantData.Columns["EndArchiveRecordNumber"].Ordinal].ToString());
                                continue;
                            }
                            string WaferID = row[ImplantData.Columns["WaferID"].Ordinal].ToString();
                            string ImplantID = row[ImplantData.Columns["ImplantID"].Ordinal].ToString();
                            string PPID = row[ImplantData.Columns["PPID"].Ordinal].ToString();
                            string LotID = row[ImplantData.Columns["LotID"].Ordinal].ToString();
                            string CarrierID = row[ImplantData.Columns["CarrierID"].Ordinal].ToString();
                            string FileName = row[ImplantData.Columns["FileName"].Ordinal].ToString();
                            string Chamber = row[ImplantData.Columns["Chamber"].Ordinal].ToString();
                            string LoadPort = row[ImplantData.Columns["LoadPort"].Ordinal].ToString();
                            string LoadLock = row[ImplantData.Columns["LoadLock"].Ordinal].ToString();
                            string Slot = row[ImplantData.Columns["Slot"].Ordinal].ToString();

                            try
                            {
                                Log(functionName + ": WaferID " + WaferID);
                                Log(functionName + ": Wafer Implant Start Record Number " + StartArchiveRecordNumber.ToString());
                                Log(functionName + ": Wafer Implant End Record Number " + EndArchiveRecordNumber.ToString());

                                if (DCPlanDataItems.ContainsKey(toolName + "_DataLoaderReport"))
                                {
                                    DLR_ContextVariableNameValue.Add("ToolID", toolName);
                                    DLR_ContextVariableNameValue.Add("ModuleName", toolName + "_ArchivedData");
                                    DLR_ContextVariableNameValue.Add("WaferID", WaferID);
                                    DLR_ContextVariableNameValue.Add("ReportName", "Report_" + ArchiveStartTime.ToString() + " to " + ArchiveEndTime.ToString());
                                    SendStartOfReport(toolName, toolName + "_DataLoaderReport", DateTime.Now, DLR_ContextVariableNameValue);
                                }

                                DateTime tmp_StartTime = WaferImplantStartTimeUTC;
                                string DiffInMinutes = ((WaferImplantEndTimeUTC - WaferImplantStartTimeUTC).TotalMinutes).ToString();

                                if (Convert.ToInt16(Convert.ToDouble(DiffInMinutes)) > 20)
                                    WaferImplantEndTimeUTC = tmp_StartTime.AddMinutes(20);  // This will set a cap to the ammount of data we will extract from the archive file(s).

                                if (LoadOption == "Archived" || LoadOption == "ALL")
                                {
                                    GetSignalDataStatus = GetSignalData(ArchivedDir, SelectedSignals, WaferImplantStartTimeUTC, Convert.ToUInt16(WaferImplantStartTimeUTC.Millisecond.ToString()), WaferImplantEndTimeUTC, Convert.ToUInt16(WaferImplantEndTimeUTC.Millisecond.ToString()));
                                    if (GetSignalDataStatus == "OK")
                                    {
                                        DataRow[] RecordNumberQuery = ArchiveDataTable.Select("", "RecordNumber asc");

                                        Log(functionName + ": Start Record Number found in archive file for wafer  " + WaferID + ":" + RecordNumberQuery[0][ArchiveDataTable.Columns["RecordNumber"].Ordinal]);
                                        Log(functionName + ": End Record Number found in archive file for wafer  " + WaferID + ":" + RecordNumberQuery[RecordNumberQuery.Length - 1][ArchiveDataTable.Columns["RecordNumber"].Ordinal]);
                                        Int64 RetrievedEndRecordNumber = Convert.ToInt64(RecordNumberQuery[RecordNumberQuery.Length - 1][ArchiveDataTable.Columns["RecordNumber"].Ordinal]);

                                        // check again if partial data was retrieved to adjust the end record number to avoid EndRecordNumber > RetrivedEndRecordNumber
                                        if (Convert.ToInt16(Convert.ToDouble(DiffInMinutes)) > 20 || EndArchiveRecordNumber > RetrievedEndRecordNumber)
                                        {
                                            EndArchiveRecordNumber = RetrievedEndRecordNumber;
                                        }


                                        foreach (string key in DCPlanDataItems.Keys)
                                        {
                                            if (key == toolName + "_ArchivedData")
                                            {
                                                Dictionary<string, string> ContextVariableNameValue = new Dictionary<string, string>();
                                                ContextVariableNameValue.Add("Slot", Slot);
                                                ContextVariableNameValue.Add("ToolID", toolName);
                                                ContextVariableNameValue.Add("PPID", PPID);
                                                ContextVariableNameValue.Add("WaferID", WaferID);
                                                ContextVariableNameValue.Add("ImplantID", ImplantID);
                                                ContextVariableNameValue.Add("LotID", LotID);
                                                ContextVariableNameValue.Add("CarrierID", CarrierID);
                                                ContextVariableNameValue.Add("Chamber", Chamber);
                                                ContextVariableNameValue.Add("LoadPort", LoadPort);
                                                ContextVariableNameValue.Add("LoadLock", LoadLock);

                                                Log(functionName + ": Sending archivedata for tool name " + toolName);
                                                Boolean Found = SendArchiveData(toolName, key, StartArchiveRecordNumber, EndArchiveRecordNumber, WaferImplantStartTime, WaferImplantEndTime,
                                                                                        ContextVariableNameValue, new DirectoryInfo(ImplantsDir.FullName + DateDir));

                                                if (DCPlanDataItems.ContainsKey(toolName + "_DataLoaderReport"))
                                                {
                                                    DataLoaderReport.Rows.Add(DateTime.Now, key, FileName, LotID, WaferID, WaferImplantStartTime, WaferImplantEndTime,
                                                        StartArchiveRecordNumber, EndArchiveRecordNumber,
                                                        (Found ? "NA" : "Could not send archive data to E3. Check DataLoader's Logfiles."), Found.ToString(),
                                                        ArchiveDataTable.Columns.Count.ToString(),
                                                        ArchiveDataTable.Select("RecordNumber >=  " + StartArchiveRecordNumber + " and " + "RecordNumber <= " + EndArchiveRecordNumber, "").Length,
                                                        DateTime.Now,
                                                        (DateTime.Now - WaferImplantEndTime).TotalSeconds.ToString(), (WaferImplantEndTime - WaferImplantStartTime).TotalSeconds.ToString(), Slot, Dns.GetHostName(), ImplantID);
                                                    SendDataLoaderReport(toolName, toolName + "_DataLoaderReport", DataLoaderReport);
                                                    SendEndOfReport(toolName, toolName + "_DataLoaderReport", DateTime.Now.AddMilliseconds(10), DLR_ContextVariableNameValue);
                                                    DataLoaderReport.Clear();
                                                }
                                                RunProcessingTimer.Stop();
                                            }
                                        } // End of foreach key
                                    } //Signal data not found
                                    else
                                    {
                                        Log("Could not get signal data for this Wafer between date range " +
                                                 WaferImplantStartTime.ToString() + " and " + WaferImplantEndTime.ToString());
                                        if (DCPlanDataItems.ContainsKey(toolName + "_DataLoaderReport"))
                                        {
                                            DataLoaderReport.Rows.Add(DateTime.Now, toolName + "_ArchivedData", FileName, LotID, WaferID,
                                                WaferImplantStartTime, WaferImplantEndTime,
                                                StartArchiveRecordNumber, EndArchiveRecordNumber,
                                                "Could not get signal data for the wafer." + GetSignalDataStatus, "False",
                                                ArchiveDataTable.Columns.Count.ToString(),
                                                ArchiveDataTable.Select("RecordNumber >=  " + StartArchiveRecordNumber + " and " + "RecordNumber <= " + EndArchiveRecordNumber, "").Length,
                                                DateTime.Now, RunProcessingTimer.ElapsedMilliseconds.ToString(), (WaferImplantEndTime - WaferImplantStartTime).TotalSeconds.ToString(),Slot,Dns.GetHostName());
                                            SendDataLoaderReport(toolName, toolName + "_DataLoaderReport", DataLoaderReport);
                                            SendEndOfReport(toolName, toolName + "_DataLoaderReport", DateTime.Now.AddMilliseconds(10), DLR_ContextVariableNameValue);
                                            DataLoaderReport.Clear();
                                        }
                                        RunProcessingTimer.Stop();
                                    }
                                }
                            }
                            catch (Exception Ex)
                            {
                                Log(functionName + "Exception while Processing  " + new DirectoryInfo(ImplantsDir.FullName + DateDir).FullName + Ex.Message);
                            }
                        }
                        else // Wafer time range not within specified range
                        {
                            Log(functionName + " Wafer time range not within specified range");
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                Log("Error in Process Directory " + Ex.Message);
            }
            ImplantData.Clear();
        }

        private static DateTime ConvertTimeZone(DateTime DateTime, String TimeZone)
        {
            DateTime ConvertedDate = new DateTime();
            if (TimeZone == "UTC")
            {
                ConvertedDate = DateTime.SpecifyKind(DateTime, DateTimeKind.Local);
                ConvertedDate = ConvertedDate.ToUniversalTime();
            }
            else if (TimeZone == "LOCAL")
            {
                ConvertedDate = DateTime.SpecifyKind(DateTime, DateTimeKind.Utc);
                ConvertedDate = ConvertedDate.ToLocalTime();
            }

            return ConvertedDate;
        }

        private static string GetEventSchema(string eventName, string toolInstanceName)
        {
            string functionName = "GetEventSchema";
            Log(functionName + ": Start to get event schema for event " + eventName);

            string eventSchema = string.Empty;
            try
            {
                if (_adapterSDK == null)
                {
                    Log("Equipment adapter is not initialized....");
                    return "";
                }

                Dictionary<string, IEventInfo[]> toolProcessEvents = _adapterSDK.GetEquipmentProcessEvents(toolName);

                if (toolProcessEvents.ContainsKey(toolInstanceName))
                {
                    Log(functionName + ": ToolName [" + toolInstanceName + "] exist in equipment process type events");
                    IEventInfo[] events = toolProcessEvents[toolInstanceName];
                    if (events != null)
                    {
                        foreach (IEventInfo eventInfo in events)
                        {
                            if (eventInfo.EventName.CompareTo(eventName) == 0)
                            {
                                eventSchema = eventInfo.EventSchema;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(functionName + ": " + ex.Message);
            }
            Log(functionName + ": Get event schema for event " + eventName + "complete. The schema is " + eventSchema);
            return eventSchema;
        }

        private static Dictionary<string, List<EqDataCollectionPlanInfo>> GetDCPlans(string equipmentName)
        {
            string functionName = "GetDCPlans";
            Log(functionName + ": Start to get GetDCPlans ");
            Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans = null;
            string eventSchema = string.Empty;
            try
            {
                if (_adapterSDK == null)
                {
                    Log("Equipment adapter is not initialized....");
                    return null;
                }

                //EquipmentAdapter _testSDK = new EquipmentAdapter();

                DCPlans = _adapterSDK.GetEqDataCollectionPlans(equipmentName);
            }
            catch (Exception ex)
            {
                Log(functionName + ": " + ex.Message);
            }
            Log(functionName + ": Get DCP Plan complete");
            return DCPlans;
        }

        private static void GetStartRunInfo(string toolName, Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans, ref string StartScanEventID, ref string[] StartTriggerVIDList)
        {
            string functionName = "GetStartRunInfo";
            Log(functionName + ": Start to GetStartRunInfo ");
            try
            {

                foreach (EqDataCollectionPlanInfo planInfo in DCPlans[toolName])
                {
                    //foreach (EqDataCollectionPlanInfo.DCPItemGroup ItemGroup in planInfo.ItemGroups)
                    //{
                    //    switch (ItemGroup.CollectionMethod)
                    //    {
                    //        case DCPDataCollectionMethod.Poll:
                    ArrayList PollStartTriggerVIDList = new ArrayList();
                    StartScanEventID = (planInfo.StartTrigger.Events.Count == 0 ? "NaN" : planInfo.StartTrigger.Events[0].AlarmEventID.ToString());
                    if (planInfo.StartTrigger.Events.Count != 0 && planInfo.EndTrigger.Events.Count != 0)
                    {
                        foreach (AlarmEvent Event in planInfo.Events)
                        {
                            if (Event.AlarmEventID.ToString() == planInfo.StartTrigger.Events[0].AlarmEventID.ToString())
                            {
                                foreach (ParamItem Item in Event.Parameters)
                                {
                                    if (!PollStartTriggerVIDList.Contains(Item.Parameter))
                                    {
                                        PollStartTriggerVIDList.Add(Item.Parameter);
                                    }
                                }
                            }
                        }
                        StartTriggerVIDList = (string[])PollStartTriggerVIDList.ToArray(typeof(string));
                    }
                    //        break;
                    //    default:
                    //        break;
                    //}
                    //}

                }
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " " + Ex.Message);
            }
        }

        private static void GetEndRunInfo(string toolName, Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans, ref string EndScanEventID, ref string[] EndTriggerVIDList)
        {
            string functionName = "GetEndRunInfo";
            Log(functionName + ": Start to GetEndRunInfo ");

            try
            {
                foreach (EqDataCollectionPlanInfo planInfo in DCPlans[toolName])
                {
                    //foreach (EqDataCollectionPlanInfo.DCPItemGroup ItemGroup in planInfo.ItemGroups)
                    //{
                    //    switch (ItemGroup.CollectionMethod)
                    //    {
                    //        case DCPDataCollectionMethod.Poll:
                    ArrayList PollEndTriggerVIDList = new ArrayList();
                    EndScanEventID = (planInfo.EndTrigger.Events.Count == 0 ? "NaN" : planInfo.EndTrigger.Events[0].AlarmEventID.ToString());


                    if (planInfo.StartTrigger.Events.Count != 0 && planInfo.EndTrigger.Events.Count != 0)
                    {
                        foreach (AlarmEvent Event in planInfo.Events)
                        {
                            if (Event.AlarmEventID.ToString() == planInfo.EndTrigger.Events[0].AlarmEventID.ToString())
                            {
                                foreach (ParamItem Item in Event.Parameters)
                                {
                                    if (!PollEndTriggerVIDList.Contains(Item.Parameter))
                                    {
                                        PollEndTriggerVIDList.Add(Item.Parameter);
                                    }
                                }
                            }
                        }
                        EndTriggerVIDList = (string[])PollEndTriggerVIDList.ToArray(typeof(string));
                    }
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //}

                }
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " " + Ex.Message);
            }

        }

        private static Dictionary<string, ArrayList> GetDCPlanDataItems(Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans)
        {
            string functionName = "GetDCPlanDataItems";
            Log(functionName + ": Start to get GetDCPlanDataItems ");
            Dictionary<string, ArrayList> DCPlanDataItems = new Dictionary<string, ArrayList>();
            try
            {
                foreach (string toolName in DCPlans.Keys)
                {
                    if (DCPlans[toolName] == null)
                        continue;


                    foreach (EqDataCollectionPlanInfo planInfo in DCPlans[toolName])
                    {
                        foreach (EqDataCollectionPlanInfo.DCPItemGroup ItemGroup in planInfo.ItemGroups)
                        {
                            switch (ItemGroup.CollectionMethod)
                            {
                                case DCPDataCollectionMethod.Poll:
                                    ArrayList DCPollVIDList = new ArrayList();
                                    ArrayList PollStartTriggerVIDList = new ArrayList();
                                    ArrayList PollEndTriggerVIDList = new ArrayList();

                                    foreach (ParamItem Item in ItemGroup.Parameters)
                                    {
                                        if (!DCPollVIDList.Contains(Item.Parameter))
                                        {
                                            DCPollVIDList.Add(Item.Parameter);
                                        }
                                    }
                                    DCPlanDataItems.Add(toolName, DCPollVIDList);
                                    break;
                                default:
                                    break;
                            }
                        }

                    }
                }
            }
            catch (Exception Ex)
            {
                Log("Exception in GetDCPlanDataItems: " + Ex.Message);
            }
            Log("GetDCPlanDataItems Complete ");
            return DCPlanDataItems;
        }
        private static void GetImplantData(String DirName, String FileName, ref DataTable ImplantData)
        {

            string functionName = "GetImplantData";
            Log(functionName + ": Start to get GetImplantData in " + DirName);
            if (Directory.Exists(DirName))
            {
                DirectoryInfo ImplantTimeDirs = new DirectoryInfo(DirName + "\\");
                //foreach (DirectoryInfo TimeDir in ImplantTimeDirs.GetDirectories())
                //{
                //Log("Searching " + TimeDir.FullName);
                ArrayList ImplantFiles = new ArrayList();
                ImplantFiles = GetFileList(DirName);
                //ImplantFiles = GetFileList(TimeDir.FullName);
                foreach (FileInfo ImplantFile in ImplantFiles)
                {
                    if (ImplantFile.Name == FileName)
                    {
                        cFileInfo ImplantFileInfo = new cFileInfo();
                        ImplantFileInfo = GetImplantFileInfo(ImplantFile.FullName);
                        ImplantData.Rows.Add(ImplantFileInfo.PPID, ImplantFileInfo.CarrierID, ImplantFileInfo.Chamber, ImplantFileInfo.LoadPort, ImplantFileInfo.LoadLock, ImplantFileInfo.LotID, ImplantFileInfo.WaferID,
                            ImplantFileInfo.ImplantID, ImplantFileInfo.Slot, ImplantFileInfo.WaferStartTime, ImplantFileInfo.WaferEndTime, ImplantFileInfo.Resumed, ImplantFileInfo.FileName,
                            ImplantFileInfo.StartArchiveRecordNumber, ImplantFileInfo.MiddleArchiveRecordNumber, ImplantFileInfo.EndArchiveRecordNumber);
                    }
                }
                //}
            }
        }

        private static void GetImplantData(String DirName, ref DataTable ImplantData)
        {

            string functionName = "GetImplantData";
            Log(functionName + ": Start to get GetImplantData in " + DirName);
            if (Directory.Exists(DirName))
            {
                DirectoryInfo ImplantTimeDirs = new DirectoryInfo(DirName + "\\");
                //foreach (DirectoryInfo TimeDir in ImplantTimeDirs.GetDirectories())
                //{
                //Log("Searching " + TimeDir.FullName);
                ArrayList ImplantFiles = new ArrayList();
                ImplantFiles = GetFileList(DirName);
                //ImplantFiles = GetFileList(TimeDir.FullName);
                foreach (FileInfo ImplantFile in ImplantFiles)
                {
                    cFileInfo ImplantFileInfo = new cFileInfo();
                    ImplantFileInfo = GetImplantFileInfo(ImplantFile.FullName);
                    ImplantData.Rows.Add(ImplantFileInfo.PPID, ImplantFileInfo.CarrierID, ImplantFileInfo.Chamber, ImplantFileInfo.LoadPort, ImplantFileInfo.LoadLock, ImplantFileInfo.LotID, ImplantFileInfo.WaferID,
                        ImplantFileInfo.ImplantID, ImplantFileInfo.Slot, ImplantFileInfo.WaferStartTime, ImplantFileInfo.WaferEndTime, ImplantFileInfo.Resumed, ImplantFileInfo.FileName, 
                        ImplantFileInfo.StartArchiveRecordNumber, ImplantFileInfo.MiddleArchiveRecordNumber, ImplantFileInfo.EndArchiveRecordNumber);
                }
                //}
            }
        }
        private ArrayList readDirectory(string folderName, string batchID)
        {
            ArrayList readyFiles = new ArrayList(); // laser edge data files - ready to process

            try
            {
                DirectoryInfo di = new DirectoryInfo(folderName);   // folder where the data files exist
                FileInfo[] files = di.GetFiles();                   // file info for each entry in the directory
                foreach (FileInfo file in files)
                {
                    if (file.Name.ToLower().Contains(batchID))
                    {
                        readyFiles.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception creating simulated data file. ex=" + ex.Message);
            }
            return readyFiles;
        }

        public void ConnectToEqp()
        {
            // Validate Path to Data File
            Log("Starting ConnectToEqp Method...");
        }

        public void ConnectToE3(string eqpName)
        {
            Log("Starting ConnectToE3 Method...");
            //Start a session on DSP - for Failover and load balancing.

            try
            {
                bool isSessionValid = _adapterSDK.StartSession(toolName, false, false, DateTime.Now);

                Log("Is Session Valid: " + isSessionValid.ToString());

                if (isSessionValid)
                {
                    if (!_sessionList.Contains(eqpName))
                    {
                        lock (_sessionList)
                        {
                            _sessionList.Add(eqpName);
                        }
                    }
                    LoadSDKEvents(eqpName);
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        public void SendDataToE3(object data)
        {
            Log("Starting SendDataToE3 Method...");
        }
        public void DisconnectFromE3(string eqpName)
        {
            Log("Starting DisconnectFromE3 Method...");
        }
        public void DisconnectFromEqp()
        {
            Log("Starting DisconnectFromEqp Method...");
        }

        protected static void Dispose(bool disposing)
        {
            Log("Starting Dispose Method...");
            if (disposing)
            {
                if (_adapterSDK != null)
                {
                    //if (_adapterSDK.HasValidSession(toolName))
                    //{
                    //    _adapterSDK.EndSession(toolName);
                    //    Console.WriteLine("End Session");
                    //    Log("End Session");
                    //    Console.Read();
                    //}
                }
            }
            Environment.Exit(0);
        }

        public static string GetInstallPath()
        {
            //RegistryKey _reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Applied Materials\CollectorSDK");

            //String _installLocation = (String)_reg.GetValue("InstallLocation");
            //if (_installLocation.Trim().Length == 0)
            //{
            //    throw new Exception("Equipment adapter SDK install location is missing");
            //}
            //return _installLocation;
            return EES.Framework.Common.RuntimeSetting.RuntimeSetting.SDKInstallDirectory;
        }

        public static void Log(string msg)
        {
            lock (ringFile)
            {
                try
                {
                    ringFile.Write("IanArchiveDataLoader", LogCategory.AuditInfo, msg, "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Logger thrown exception: " + ex.Message);
                }
            }
        }

        public static void LogStats(string msg)
        {
            lock (StatsFile)
            {
                try
                {
                    StatsFile.Write("Statistics", LogCategory.AuditInfo, msg, "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Logger thrown exception: " + ex.Message);
                }
            }
        }

        // Log message to Logfile
        public static void logMsg(string msg)
        {
            lock (fileLock)
            {
                msg = DateTime.Now.ToString(logDTFormat) + msg;
                Console.WriteLine(msg);
                DirectoryInfo dl = new DirectoryInfo(logFilePath);

                if (!dl.Exists)
                {
                    System.IO.Directory.CreateDirectory(logFilePath);
                }
                DateTime now = DateTime.Now;

                if (logFileName.Length == 0)
                {
                    logFileName = logFilePath + toolName + "_" + now.ToString("yyyyMMdd_HHmmss") + ".txt";
                }
                else
                {
                    FileInfo fi = new FileInfo(logFileName);
                    if (fi.Length > logFileMaxSize)
                    {
                        // Then start a new one
                        logFileName = logFilePath + toolName + "_" + now.ToString("yyyyMMdd_HHmmss") + ".txt";
                    }
                }

                StreamWriter sw = new StreamWriter(logFileName, true);
                sw.WriteLine(msg);
                sw.Close();
            }
        }


        private static string GetXml(string schema)
        {
            StringWriter stringWriter = new StringWriter();

            XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
            xmlTextWriter.WriteStartDocument(true);
            xmlTextWriter.Formatting = Formatting.Indented;

            XSDParser schemaParser = new XSDParser(schema);
            MemberInfo memberInfo = schemaParser.GetSchemaType();
            GetMemberXml(xmlTextWriter, memberInfo, schemaParser);
            string retVal = stringWriter.ToString();
            stringWriter.Close();
            return retVal;
        }

        private static void GetMemberXml(XmlTextWriter xmlTextWriter, MemberInfo memberInfo, XSDParser schemaParser)
        {
            if (memberInfo != null)
            {
                xmlTextWriter.WriteStartElement(memberInfo.Name);

                if (memberInfo.ArrayType || memberInfo.ComplexType)
                {
                    MemberInfo[] members = schemaParser.GetMembers(memberInfo.DataType);
                    foreach (MemberInfo member in members)
                        GetMemberXml(xmlTextWriter, member, schemaParser);
                }
                else
                    xmlTextWriter.WriteString(string.Empty);

                xmlTextWriter.WriteEndElement();
            }
        }


        private void LoadSDKEvents(string equipmentName)
        {
            Log("    LoadSDKEvents  EquipmentName = " + equipmentName);
            try
            {
                lock (_adapterSDK)
                {
                    IEventInfo[] eqevents = _adapterSDK.GetEquipmentEvents(equipmentName);
                    lock (EventsInfo)
                    {
                        if (EventsInfo.Contains(equipmentName))
                        {
                            EventsInfo[equipmentName] = eqevents;
                        }
                        else
                        {
                            EventsInfo.Add(equipmentName, eqevents);
                        }
                        lock (eventCache)
                        {
                            foreach (object key in eventCache)
                            {
                                string eventID = key.ToString();
                                if (eventID.StartsWith(equipmentName))
                                {
                                    eventCache.Remove(key);
                                }
                            }
                            foreach (IEventInfo eveInfo in eqevents)
                            {
                                string xml = GetXml(eveInfo.EventSchema);
                                Log(" LoadSDKEvents  EventID  = " + eveInfo.EventID + " EventXml= " + xml);

                                if (eventCache.Contains(equipmentName + eveInfo.EventID))
                                {
                                    eventCache[equipmentName + eveInfo.EventID] = xml;
                                }
                                else
                                {
                                    eventCache.Add(equipmentName + eveInfo.EventID, xml);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception in loading SDK events ex: " + ex.Message);
            }
        }

        void handleCmdReply(string eqName, string transID, string replyeventID, string msg, string status)
        {
            try
            {
                Log("starting handleCmdReply....");

                XmlDocument resultDoc = new XmlDocument();
                resultDoc.LoadXml(msg);

                XmlNode rootNode = resultDoc.DocumentElement;
                XmlNode statusNode = rootNode.SelectSingleNode("/*/STATUS__ASCII");
                if (statusNode != null)
                {
                    Log("adding status");
                    statusNode.InnerXml = status;
                }

                Log("        Sending XML Reply: " + rootNode.OuterXml);

                _adapterSDK.SendCommandReply(eqName, DateTime.Now, transID, Convert.ToInt32(replyeventID), rootNode.OuterXml);
            }
            catch (Exception Ex)
            {
                Log("Error in handleCmdReply...." + Ex.Message);
            }
        }
        private static cFileInfo GetBeamSetupFileInfo(String FileName)
        {
            StreamReader sr = null;
            cFileInfo FileInfo = new cFileInfo();
            try
            {
                if (File.Exists(FileName))
                {
                    FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                    sr = new StreamReader(fs);
                    FileInfo.FileName = FileName;
                    string line = "";
                    string[] thisline;
                    while (sr.Peek() != -1)
                    {
                        try
                        {
                            line = sr.ReadLine();
                            if (line.Trim().StartsWith("PPID"))
                            {
                                line = line.Trim();
                                thisline = line.Split(':');
                                FileInfo.PPID = (thisline[1].ToString().Contains(".") ? thisline[1].Split('.')[1].Trim() : thisline[1].ToString());
                            }
                            else if (line.Trim().StartsWith("Carrier"))
                            {
                                line = line.Trim();
                                thisline = line.Split('-');
                                string sTemp = thisline[0].ToString().Trim().Split(':')[1].Trim();
                                string LotID = sTemp.Substring(0, sTemp.Length - sTemp.IndexOf("(Slot")).Trim();
                                sTemp = thisline[1].ToString().Trim().Split(':')[1].Trim();
                                string WaferID = sTemp.Trim();
                                FileInfo.LotID = LotID;
                                FileInfo.WaferID = WaferID;
                            }
                            else if (line.Trim().StartsWith("Start Time"))
                            {
                                line = line.Trim();
                                string sTemp = line.Substring(line.IndexOf("Start Time : ") + "Start Time : ".Length, line.Length - "Start Time : ".Length).TrimEnd('-').Trim();
                                FileInfo.WaferStartTime = DateTime.Parse(sTemp);

                            }
                            else if (line.Trim().StartsWith("End Time"))
                            {
                                line = line.Trim();
                                string sTemp = line.Substring(line.IndexOf("End Time : ") + "End Time : ".Length, line.Length - "End Time : ".Length).TrimEnd('-').Trim();
                                FileInfo.WaferEndTime = DateTime.Parse(sTemp);
                            }

                            else if (line.Trim().StartsWith("BeamSetupID "))
                            {
                                line = line.Trim();
                                string sTemp = line.Substring(line.IndexOf("BeamSetupID : ") + "BeamSetupID : ".Length, line.Length - "BeamSetupID : ".Length).TrimEnd('-').Trim();
                                FileInfo.BeamSetupID = sTemp;
                            }
                            else if (line.Trim().StartsWith("Setup Result"))
                            {
                                line = line.Trim();
                                string sTemp = line.Substring(line.IndexOf("Setup Result : ") + "Setup Result : ".Length, line.Length - "Setup Result : ".Length).TrimEnd('-').Trim();
                                FileInfo.SetupResult = sTemp;
                            }
                            else if (line.Trim().StartsWith("Beam Energy"))
                            {
                                line = line.Trim();
                                string sTemp = line.Substring(line.IndexOf("Beam Energy : ") + "Beam Energy : ".Length, line.Length - "Beam Energy : ".Length).TrimEnd('-').Trim();
                                FileInfo.BeamEnergy = sTemp;
                            }
                            else if (line.Trim().StartsWith("Type"))
                            {
                                line = line.Trim();
                                string sTemp = line.Substring(line.IndexOf("Type : ") + "Type : ".Length, line.Length - "Type : ".Length).TrimEnd('-').Trim();
                                FileInfo.BeamSetupType = sTemp;
                            }
                            else if (line.Trim().StartsWith("Specie"))
                            {
                                line = line.Trim();
                                string sTemp = line.Substring(line.IndexOf("Specie : ") + "Specie : ".Length, line.Length - "Specie : ".Length).TrimEnd('-').Trim();
                                FileInfo.Specie = sTemp;
                            }
                            else if (line.Trim().StartsWith("Parameter"))
                            {
                                break;
                            }
                        }
                        catch(Exception)
                        {
                            continue;
                        }
                    }
                   
                }
                sr.Close();
                return FileInfo;
            }
            catch (Exception Ex)
            {
                Log("Exception parsing file " + FileName + " " + Ex.Message);
                return FileInfo;
            }
        }

        private static string GetYesterday(String Date)
        {
            string sYesterday = "";

            DateTime DT_Yesterday = DateTime.Parse(Date).AddDays(-1);
            sYesterday = DT_Yesterday.Year.ToString() + "-" + (DT_Yesterday.Month.ToString().Length == 1 ? "0" + DT_Yesterday.Month.ToString() : DT_Yesterday.Month.ToString()) + "-" + (DT_Yesterday.Day.ToString().Length == 1 ? "0" + DT_Yesterday.Day.ToString() : DT_Yesterday.Day.ToString());

            return sYesterday;
        }

        private static string GetTomorrow(String Date)
        {
            string sTomorrow = "";

            DateTime DT_Tomorrow = DateTime.Parse(Date).AddDays(1);
            sTomorrow = DT_Tomorrow.Year.ToString() + "-" + (DT_Tomorrow.Month.ToString().Length == 1 ? "0" + DT_Tomorrow.Month.ToString() : DT_Tomorrow.Month.ToString()) + "-" + (DT_Tomorrow.Day.ToString().Length == 1 ? "0" + DT_Tomorrow.Day.ToString() : DT_Tomorrow.Day.ToString());

            return sTomorrow;
        }

        private static cFileInfo GetImplantFileInfo(String FileName)
        {
            StreamReader sr = null;
            cFileInfo FileInfo = new cFileInfo();
            FileInfo.Resumed = false;
            try
            {
                if (File.Exists(FileName))
                {
                    FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                    sr = new StreamReader(fs);
                    FileInfo.FileName = FileName;
                    string line = "";
                    string[] thisline;
                    while (sr.Peek() != -1)
                    {
                        try
                        {
                            line = sr.ReadLine();
                            if (line.Trim().StartsWith("PPID"))
                            {
                                line = line.Trim();
                                thisline = line.Split(':');
                                FileInfo.PPID = (thisline[1].ToString().Contains(".") ? thisline[1].Split('.')[1].Trim() : thisline[1].ToString()); ;
                            }
                            else if (line.Trim().StartsWith("Carrier"))
                            {
                                line = line.Trim();
                                thisline = line.Split('-');
                                string sTemp = thisline[0].ToString().Trim().Split(':')[1].Trim();
                                string CarrierID = sTemp.Substring(0, sTemp.Length - sTemp.IndexOf("(Slot")).Trim();
                                sTemp = thisline[1].ToString().Trim().Split(':')[1].Trim();
                                string WaferID = sTemp.Trim();
                                FileInfo.CarrierID = CarrierID;
                                //FileInfo.WaferID = WaferID;
                                //FileInfo.WaferID = "Wafer." + FileInfo.ImplantID;
                            }
                            else if (line.Trim().StartsWith("ImplantID"))
                            {
                                line = line.Trim();
                                thisline = line.Split(':');
                                string ImplantID = thisline[1].ToString().Trim();
                                FileInfo.ImplantID = ImplantID;
                                //TestImplantID = (Convert.ToInt64(TestImplantID) + 1).ToString();
                                //FileInfo.ImplantID =TestImplantID;
                            }
                            else if (line.Trim().StartsWith("Lot"))
                            {
                                line = line.Trim();
                                thisline = line.Split('-');
                                string LotID = thisline[0].ToString().Trim().Split(':')[1].Trim();
                                FileInfo.LotID = LotID;
                            }
                            else if (line.Trim().StartsWith("Start Event"))
                            {
                                string sTemp = null;
                                line = line.Trim();
                                if (line.Contains("- Resuming implant."))
                                {
                                    sTemp = line.Substring(line.IndexOf("Start Event : ") + "Start Event : ".Length, line.Length - line.Substring(line.IndexOf(" - Resuming implant."), line.Length - line.IndexOf(" - Resuming implant.")).Length - "Start Event : ".Length);
                                    FileInfo.Resumed = true;
                                }
                                else
                                    sTemp = line.Substring(line.IndexOf("Start Event : ") + "Start Event : ".Length, line.Length - line.Substring(line.IndexOf(" - Starting"), line.Length - line.IndexOf(" - Starting")).Length - "Start Event : ".Length);

                                FileInfo.WaferStartTime = DateTime.Parse(sTemp);
                                FileInfo.WaferID = line.Substring(line.IndexOf("Wafer ID") + "Wafer ID".Length, (line.Length - line.Substring(line.IndexOf(", Wafer Lot ID"), line.Length - line.IndexOf(", Wafer Lot ID")).Length) - (line.IndexOf("Wafer ID") + "Wafer ID".Length)).Trim();
                                FileInfo.Slot = line.Substring(line.IndexOf("slot") + "slot".Length, (line.Length - line.Substring(line.IndexOf(",  Port"), line.Length - line.IndexOf(",  Port")).Length) - (line.IndexOf("slot") + "slot".Length)).Trim();
                            }
                            else if (line.Trim().StartsWith("End Event"))
                            {
                                string sTemp = null;
                                line = line.Trim();
                                if (line.Contains("- Wafer partially processed"))
                                {
                                    sTemp = line.Substring(line.IndexOf("End Event : ") + "End Event : ".Length, line.Length - line.Substring(line.IndexOf(" - Wafer partially processed"), line.Length - line.IndexOf(" - Wafer partially processed")).Length - "End Event : ".Length);
                                    FileInfo.Resumed = true;
                                }
                                else
                                    sTemp = line.Substring(line.IndexOf("End Event : ") + "End Event : ".Length, line.Length - line.Substring(line.IndexOf(" - Implant complete."), line.Length - line.IndexOf(" - Implant complete.")).Length - "End Event : ".Length);
                                FileInfo.WaferEndTime = DateTime.Parse(sTemp);
                            }
                            else if (line.Trim().StartsWith("Start Archive Record"))
                            {
                                line = line.Trim();
                                thisline = line.Split(':');

                                FileInfo.StartArchiveRecordNumber = thisline[1].Trim();
                            }
                            else if (line.Trim().StartsWith("Middle Archive Record"))
                            {
                                line = line.Trim();
                                thisline = line.Split(':');

                                FileInfo.MiddleArchiveRecordNumber = thisline[1].Trim();
                            }
                            else if (line.Trim().StartsWith("End Archive Record"))
                            {
                                line = line.Trim();
                                thisline = line.Split(':');

                                FileInfo.EndArchiveRecordNumber = thisline[1].Trim();
                            }
                            else if (line.Trim().StartsWith("Chamber"))
                            {
                                line = line.Trim();
                                thisline = line.Split(':');
                                FileInfo.Chamber = thisline[1].Substring(0, thisline[1].IndexOf("(LoadPort")).Trim();
                                FileInfo.LoadPort = line.Substring(line.IndexOf("LoadPort =") + "LoadPort =".Length, (line.Length - line.Substring(line.IndexOf("(LoadLock ="), line.Length - line.IndexOf("(LoadLock =")).Length) - (line.IndexOf("LoadPort =") + "LoadPort =".Length)).Trim().Trim(')');
                                FileInfo.LoadLock = line.Substring(line.IndexOf("LoadLock =") + "LoadLock =".Length, (line.Length - line.IndexOf("LoadLock =") - "LoadLock =".Length)).Trim().Trim(')');
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }                                         
                }
                sr.Close();
            }
            catch (Exception Ex)
            {
                Log("Exception parsing file " + FileName + " " + Ex.Message);
            }
            return FileInfo;
        }

        private static ArrayList GetFileList(string Destination)
        {
            ArrayList Files = new ArrayList();
            try
            {
                DirectoryInfo di = new DirectoryInfo(Destination);   // folder where the data files exist

               
                FileInfo[] files = di.GetFiles();                   // file info for each entry in the directory

                foreach (FileInfo file in files)
                {
                    //if (file.Name.ToLower().EndsWith(".txt") || file.Name.ToLower().EndsWith(".csv"))       // is it a txt file?
                    if ((file.Name.ToLower().StartsWith("implantreport") && file.Name.ToLower().EndsWith(".txt")) || (file.Name.ToLower().StartsWith("beamsetupreport") && file.Name.ToLower().EndsWith(".txt")))       // is it a txt file?
                    {
                        Files.Add(file);
                    }

                }
                return Files;
               
            }
            catch (Exception Ex)
            {
                Log("Exception in GetFileList" + Ex.Message);
                return null;
            }
        }

        private static ArrayList GetZipFileList(string Destination)
        {
            ArrayList Files = new ArrayList();
            try
            {
                Log("GetZipFileList");
                DirectoryInfo di = new DirectoryInfo(Destination);   // folder where the data files exist

                FileInfo[] files = di.GetFiles();                   // file info for each entry in the directory

                foreach (FileInfo file in files)
                {
                    if (file.Name.ToLower().EndsWith(".zip"))       // is it a zip file?
                    {
                        Files.Add(file);
                    }

                }
                return Files;
            }
            catch (Exception Ex)
            {
                Log("Exception in GetZipFileList" + Ex.Message);
                return null;
            }
        }
 
        private static DataTable GetVCSDirs(DateTime ArchiveStartTime, DateTime ArchiveEndTime, DirectoryInfo ImplantDirs)
        {
            DataRow[] query;
            string functionName = "GetVCSDirs";

            Log(functionName + ": Find Implant folders in time(in UTC) range " + ArchiveStartTime.ToString() + " and " + ArchiveEndTime.ToString());
            
            ArchiveStartTime = ConvertTimeZone(ArchiveStartTime, "LOCAL");
            ArchiveEndTime = ConvertTimeZone(ArchiveEndTime, "LOCAL");
            Log(functionName + ": Find Implant folders in time(in Local) range " + ArchiveStartTime.ToString() + " and " + ArchiveEndTime.ToString());


            DataTable VCSDirs = new DataTable();
            VCSDirs.Columns.Add("DateDir", typeof(string));
            VCSDirs.Columns.Add("TimeDir", typeof(string));
            while (ArchiveStartTime <= ArchiveEndTime)
            {
                DateTime Tmp_endTime = ArchiveStartTime.AddMinutes(1);



                string startTime = ArchiveStartTime.ToString("yyy-MM-dd hh:mm:ss");

                string DateDirectoryName = ArchiveStartTime.ToString("yyy-MM-dd").ToString();
                string TimeDirectoryname = DateTime.Parse(ArchiveStartTime.ToString("yyy-MM-dd HH:mm:ss")).Hour.ToString();
                TimeDirectoryname = (TimeDirectoryname.Length == 1 ? "0" + TimeDirectoryname : TimeDirectoryname);
                if (Directory.Exists(ImplantDirs.FullName + @"\" + DateDirectoryName))
                {
                    if (Directory.Exists(ImplantDirs.FullName + @"\" + DateDirectoryName + @"\" + TimeDirectoryname))
                    {
                        String Select = "DateDir = " + "'" + DateDirectoryName + "'" + " AND " + "TimeDir = " + "'" + TimeDirectoryname + "'";
                        query = VCSDirs.Select(Select, "");
                        if (query.Length == 0)
                        {
                            Log(functionName + ": Found Directory " + ImplantDirs.FullName + @"\" + DateDirectoryName + " " + TimeDirectoryname);
                            VCSDirs.Rows.Add(DateDirectoryName, TimeDirectoryname);
                        }
                    }
                }
                ArchiveStartTime = Tmp_endTime;
            }
            return VCSDirs;
        }
        private static Boolean GetTimeRange(DirectoryInfo ArchiveDirectory, ref DateTime ArchiveStartTime, ref ushort ArchiveStartMilliseconds, ref DateTime ArchiveEndTime, ref ushort ArchiveEndMilliseconds)
        {
            string functionName = "GetTimeRange";
            Log(functionName + ": Start to get GetTimeRange ");
            Boolean status = false;

            FileInfo[] files;
            Object objSignalsRequested = new Object();


            Log(functionName + " Initializing _ArchiveReader");
            ArchiveReaderWrapper _ArchiveReader = new ArchiveReaderWrapper();


            try
            {
                    status = true;
                    if (DataSource == "Live")
                    {
                        Log(functionName + "Calling AttachToOnlineImplanter");
                        _ArchiveReader.AttachToOnlineImplanter(NetworkName, Username, Domain, Password, 0);
                    }
                    else
                    {
                        // find the ArchiveInfo File
                        Log(functionName + " Calling GetFiles in " + ArchiveDirectory.FullName.ToString());
                        files = ArchiveDirectory.GetFiles("*.IanArchiveInfo", SearchOption.AllDirectories);

                        if (files.Length != 0)
                        {
                            // Attach to archives
                            string filename = ArchiveDirectory.FullName.ToString() + files[0].Name.ToString();
                            Log(functionName + "Calling AttachToOfflineArchive for file " + filename);
                            _ArchiveReader.AttachToOfflineArchive(filename);
                        }
                    }
                    Log(functionName + "Calling GetArchiveTimeRange ");
                    object objArchiveRange = new object();
                    _ArchiveReader.GetArchiveTimeRange(ref objArchiveRange); // 
                    Array TimeComponents = (Array)objArchiveRange;
                    string[] SelectedTimeComponent = new string[4];

                    ArchiveStartTime = DateTime.Parse(TimeComponents.GetValue(0, 0).ToString()).AddMinutes(1);
                    ArchiveStartMilliseconds = (ushort)Convert.ToUInt32(TimeComponents.GetValue(1, 1).ToString());
                    ArchiveEndTime = DateTime.Parse(TimeComponents.GetValue(1, 0).ToString());
                    ArchiveEndMilliseconds = (ushort)Convert.ToInt32(TimeComponents.GetValue(0, 1).ToString());

                    Log("Archive Start time(UTC):" + ConvertTimeZone(ArchiveStartTime, "UTC").ToString());
                    Log("Archive End  time(UTC):" + ConvertTimeZone(ArchiveEndTime, "UTC").ToString());

                    Log("Archive Start time(Local):" + ConvertTimeZone(ArchiveStartTime,"LOCAL").ToString());
                    Log("Archive End  time(Local):" + ConvertTimeZone(ArchiveEndTime, "LOCAL").ToString());
            }
            catch (Exception ex)
            {
                Log("Exception: Unable to get GetArchiveTimeRange." + ex.Message);
                status = false;
            }
            _ArchiveReader.Detach();
            return status;
        }
        private static uint[] GetSignals(DirectoryInfo ArchiveDirectory, Dictionary<string, ArrayList> DCPlanDataItems)
        {
            string functionName = "GetSignals";
            Log(functionName + ": Start to get GetSignals ");
            //FileInfo[] files;
            uint[] SelectedSignals = null;

            //Log(functionName + " Initializing _ArchiveReader");
            //ArchiveReaderWrapper _ArchiveReader = new ArchiveReaderWrapper();

            try
            {

                //files = ArchiveDirectory.GetFiles("*.IanArchiveInfo", SearchOption.AllDirectories);

                //// Attach to archives
                //string filename = ArchiveDirectory.FullName.ToString() + files[0].Name.ToString();
                //_ArchiveReader.AttachToOfflineArchive(filename);
                // Get a list of signals
                Object objSignals = new Object();
                try
                {
                    _ArchiveReader.GetSignals(ref objSignals);
                }
                catch (SystemException ex)
                {
                    Log("Exception getting signal information from archives." + ex.Message);

                }
                Array Signals = (Array)objSignals;

                _SensorList = new Dictionary<string, uint>();
                //FileStream MasterSensorList = File.OpenWrite(@SignalDataDir + "_MasterSensorList.csv");
                //FileStream SelectedSignalsList = File.OpenWrite(@SignalDataDir + "_SelectedSignalsList.csv");
                //Byte[] output = new UTF8Encoding(true).GetBytes("ToolSensorName,E3SensorName,SensorID" + "\n");
                //MasterSensorList.Write(output, 0, output.Length);

                //output = new UTF8Encoding(true).GetBytes("ToolSensorName,E3SensorName,SensorID" + "\n");
                //SelectedSignalsList.Write(output, 0, output.Length);

                for (int i = 0; i < Signals.Length / 2; i++)
                {
                    string thisSensor = ConvertToolSignalName(Signals.GetValue(i, 0).ToString());
                    if (!_SensorList.ContainsKey(thisSensor))
                        _SensorList.Add(thisSensor, Convert.ToUInt32(Signals.GetValue(i, 1))); // Sensor Name, SensorID
                    else
                        Log("Found Duplicate sensor name " + thisSensor + " will not be added");
                    //output = new UTF8Encoding(true).GetBytes(Signals.GetValue(i, 0).ToString() + "," + ConvertToolSignalName(Signals.GetValue(i, 0).ToString()) + "," + Signals.GetValue(i, 1) + "\n");
                    //MasterSensorList.Write(output, 0, output.Length);
                }
                //MasterSensorList.Close();
                //SelectedSignalsList.Close();

                ArchiveDataTable = new DataTable();
                // StorageStageNumber,DateTime,and RecordNumber are provided as a header(see GetSignalData).
                ArchiveDataTable.Columns.Add("StorageStageNumber", typeof(Int32));
                ArchiveDataTable.Columns.Add("DateTime", typeof(DateTime));
                ArchiveDataTable.Columns.Add("RecordNumber", typeof(Int32));


                Object objSignalData = new Object();

                //PlanDataItems = new Dictionary<string, ArrayList>();

                DataTable SignalInfoTable = new DataTable();
                SignalInfoTable.Columns.Add("SignalName", typeof(string));
                SignalInfoTable.Columns.Add("SignalID", typeof(Int32));
                // this loop does three things:
                // 1. writes all the signal ID in a temporary table so we can retrieve them later from the archive files. 
                // 2. Stores DCPData items in a dictioany so we will know later which tools's been configured for what
                // 3. Creates the columns in ArchiveDataTable. This table will store all the data extracted from the archive files for ALL sesors that have been configured in E3
                //    regardless of which toolName.
               
                    
                ArrayList ArrayOfSignalNames = new ArrayList();
                foreach (string DCPlanDataItemName in DCPlanDataItems[toolName + "_ArchivedData"])
                {
                    //uint SignalID = GetSignalID(DCPlanDataItemName, _SensorList);
                    uint SignalID = 0;
                    if (_SensorList.ContainsKey(DCPlanDataItemName))
                        SignalID = _SensorList[DCPlanDataItemName];

                    if (SignalID != 0)
                    {
                        // 1:
                        string SelectStatement = "SignalName = " + "'" + DCPlanDataItemName + "'";
                        DataRow[] query = SignalInfoTable.Select(SelectStatement, "");
                        if (query.Length == 0)
                            SignalInfoTable.Rows.Add(DCPlanDataItemName, SignalID);
                        // 2a:
                        ArrayOfSignalNames.Add(DCPlanDataItemName);

                        // 3:
                        if (!ArchiveDataTable.Columns.Contains(DCPlanDataItemName))
                            ArchiveDataTable.Columns.Add(DCPlanDataItemName, typeof(string));


                    }
                }
                    // 2b:
                    //PlanDataItems.Add(toolName, ArrayOfSignalNames);

             

                // Build the requested signal list
                //DataRow[] query1 = SignalInfoTable.Select("", "");
                //SelectedSignals = new uint[query1.Length];
                //int index = 0;
                //foreach (DataRow row in query1)
                //{
                //    SelectedSignals[index++] = Convert.ToUInt32(row[1]);

                //}
                SelectedSignals = new uint[SignalInfoTable.Rows.Count];
                for (int index = 0; index < SignalInfoTable.Rows.Count; index++)
                {
                    SelectedSignals[index] = Convert.ToUInt32(SignalInfoTable.Rows[index][1]);
                }

            }

            catch (Exception ex)
            {
                Log(functionName + "Exception: Unable to get Signals." + ex.Message + "Source of Excpetion " + ex.Source);
            }
            //_ArchiveReader.Detach();
            return SelectedSignals;
        }
        private static void InitializeArchiveReader(DirectoryInfo ArchiveDirectory)
        {
            string functionName = "InitializeArchiveReader";
            Log(functionName + ": Start to InitializeArchiveReader ");

            if (_ArchiveReader != null)// this is  case an error occured in process directory
                _ArchiveReader.Detach();

            FileInfo[] files;

            Log(functionName + " Initializing _ArchiveReader");
            _ArchiveReader = new ArchiveReaderWrapper();

            if (DataSource == "Live")
                _ArchiveReader.AttachToOnlineImplanter(NetworkName, Username, Domain, Password, 0);
            else
            {
                // find the ArchiveInfo File
                files = ArchiveDirectory.GetFiles("*.IanArchiveInfo", SearchOption.AllDirectories);


                // Attach to archives
                string filename = ArchiveDirectory.FullName.ToString() + files[0].Name.ToString();
                if (files.Length != 0)
                {
                    _ArchiveReader.AttachToOfflineArchive(filename);
                }
            }
        }
        private static void ExtractArchiveData(Array SignalData)
        {
            string functionName = "ExtractArchiveData";
            Log(functionName + ": Start to get ExtractArchiveData ");

            try
            {
                //Extract the data.
                //*********************************************************************************************************
                //  4 arrays are returned. 
                //  
                //  The first array contains time and milliseconds( 2 columns(datetime and misslisecond) , M rows where M is number of samples
                //  The seconds array contains vids of signals found( array of N, where is the number of selected signals)
                //  The third array contains values(N columns, M rows. N is the number of selected Signals, M is the now of samples)
                //  The forth array contains StorageStage number and record number(2 columns(Storage State number and record number, M rows whenre M is number of samples)
                //*********************************************************************************************************

                Array PostTimes = (Array)SignalData.GetValue(0);

                DateTime[] TimeStamps = new DateTime[PostTimes.GetLength(0)];
                for (int i = 0; i < PostTimes.GetLength(0); i++) // number of rows
                {
                    TimeStamps[i] = DateTime.Parse(PostTimes.GetValue(i, 0).ToString()).AddMilliseconds(Convert.ToDouble(PostTimes.GetValue(i, 1)));
                }

                Array FoundVIDs = (Array)SignalData.GetValue(1);
                Array DataVals = (Array)SignalData.GetValue(2);
                Array RecordNumbers = (Array)SignalData.GetValue(3);

                int nVals = PostTimes.GetLength(0);

                if (nVals != DataVals.GetLength(0) || nVals < 1)
                {
                    Cursor.Current = Cursors.Default;
                    Log("Error in obtained data.");

                }
                for (int i = 0; i < DataVals.GetLength(0); i++) // number of rows
                {
                    Object[] ArchiveData = new Object[DataVals.GetLength(1) + 3];

                    TimeStamps[i] = ConvertTimeZone(TimeStamps[i], "LOCAL");


                    ArchiveData[0] = RecordNumbers.GetValue(i, 0); //Add Storage Stage number
                    ArchiveData[1] = TimeStamps[i];
                    ArchiveData[2] = RecordNumbers.GetValue(i, 1); //Add record number

                    for (int j = 0; j < DataVals.GetLength(1); j++)  // Number of columns
                    {
                        ArchiveData[j + 3] = DataVals.GetValue(i, j).ToString();
                    }
                    ArchiveDataTable.Rows.Add(ArchiveData);
                }
            }
            catch (Exception Ex)
            {
                Log("Exception Extracting Archive Data: " + Ex.Message);
            }
            Log(functionName + ": Done extracting archive data ");
        }
        private static string GetSignalData(DirectoryInfo ArchiveDirectory, uint[] SelectedSignals, DateTime startTime, ushort startMilliseconds, DateTime endTime, ushort endMilliseconds)
        {
            string functionName = "GetSignalData";
            Log(functionName + ": Start to get GetSignalData from " + ConvertTimeZone(startTime, "LOCAL").ToString() + " to " + ConvertTimeZone(endTime, "LOCAL").ToString());
            string Status = "FAILED";

            

            ArchiveDataTable.Clear();
            //Round up the difference implant start to end span to next integer value
            int DiffInMinutes = Convert.ToInt16((endTime - startTime).TotalMinutes.ToString().Split('.')[0]) + 1;

            // Compute the number iterations to fetch data from the archive in 5-minute chunks
            int increment = 0;
            int iterations = 0;
            if (DiffInMinutes > 5)
            {
                iterations = (DiffInMinutes / 5) + 1;
                increment = 5;
            }

            try
            {
                if (iterations == 0)
                {
                    Object objSignalResults = new Object();
                    
                        try
                        {
                            Boolean GetSignaDataStatus = false; ;
                            Exception GetSignalDataException1 = new Exception();
                            for (int Retry = 0; Retry < 3; Retry++)
                            {
                                try
                                {
                                    Log(functionName + ": Getting GetSignalData from " + ConvertTimeZone(startTime, "LOCAL").ToString() + " to " + ConvertTimeZone(endTime, "LOCAL").ToString());
                                    _ArchiveReader.GetSignalData(startTime, startMilliseconds, endTime, endMilliseconds, SelectedSignals, ref objSignalResults);
                                    Log(functionName + ": Got GetSignalData from " + ConvertTimeZone(startTime, "LOCAL").ToString() + " to " + ConvertTimeZone(endTime, "LOCAL").ToString());
                                    Retry = 3;
                                    GetSignaDataStatus = true;
                                }
                                catch (Exception GetSignalDataException)
                                {
                                    Thread.Sleep(5000);
                                    GetSignalDataException1 = GetSignalDataException;
                                    //InitializeArchiveReader(ArchivedDir);
                                }
                            }
                            if (!GetSignaDataStatus)
                                throw (GetSignalDataException1);


                        }
                        catch (Exception GetSignalDataException)
                        {
                            Log(functionName + ": Exception getting GetSignalData " + GetSignalDataException.Message);
                            Status = "Exception getting GetSignalData " + GetSignalDataException.Message;
                            InitializeArchiveReader(ArchivedDir);
                            return Status;
                        }
                    Array SignalData = (Array)objSignalResults;
                    ExtractArchiveData(SignalData);
                }
                else
                {
                    DateTime tmp_StartTime = startTime;
                    for (int i = 0; i < iterations; i++ )
                    {
                        Object objSignalResults = new Object();
                        try
                        {
                            DateTime tmp_EndTime = tmp_StartTime.AddMinutes(increment);
                            ushort tmp_endMilliseconds = 0;
                            if (DateTime.Compare(tmp_EndTime, ArchiveEndTime) > 0)
                            {
                                tmp_EndTime = ArchiveEndTime;
                                tmp_endMilliseconds = ArchiveEndMilliseconds;
                                i = iterations;
                            }
                            Boolean GetSignaDataStatus = false; ;
                            Exception GetSignalDataException1 = new Exception();
                            for (int Retry = 0; Retry < 3; Retry++)
                            {
                                try
                                {
                                    Log(functionName + ": Getting GetSignalData from " + ConvertTimeZone(tmp_StartTime, "LOCAL").ToString() + " to " + ConvertTimeZone(tmp_EndTime, "LOCAL").ToString());
                                    _ArchiveReader.GetSignalData(tmp_StartTime, startMilliseconds, tmp_EndTime, tmp_endMilliseconds, SelectedSignals, ref objSignalResults);
                                    Log(functionName + ": Got GetSignalData from " + ConvertTimeZone(tmp_StartTime, "LOCAL").ToString() + " to " + ConvertTimeZone(tmp_EndTime, "LOCAL").ToString());
                                    startMilliseconds = 0; // Set to 0 after the 1st chunck. This will give us the EXACT start record number.
                                    Retry = 3;
                                    GetSignaDataStatus = true;
                                }
                                catch (Exception GetSignalDataException)
                                {
                                    Thread.Sleep(5000);
                                    GetSignalDataException1 = GetSignalDataException;
                                    InitializeArchiveReader(ArchivedDir);
                                }
                            }
                            if (!GetSignaDataStatus)
                                throw (GetSignalDataException1);


                        }
                        catch (Exception GetSignalDataException)
                        {
                            Log(functionName + ": Exception getting GetSignalData " + GetSignalDataException.Message);
                            Status = "Exception getting GetSignalData " + GetSignalDataException.Message;
                            InitializeArchiveReader(ArchivedDir);
                            return Status;
                        }
                        tmp_StartTime = tmp_StartTime.AddMinutes(increment);
                        Array SignalData = (Array)objSignalResults;
                        ExtractArchiveData(SignalData);
                    }
                }
            }
            catch (SystemException ex)
            {
                Log("Unable to get signal data from archives. _ArchiveReader.GetSignalData() failed.  {0}" + ex.Message);

            }
            Log(functionName + ": Done GetSignalData ");
            if (ArchiveDataTable.Rows.Count > 0)
                Status = "OK";
            return Status;
        }
        private static void DetachArchiveReader()
        {
            string functionName = "DetachArchiveReader";
            Log(functionName + ": Start to DetachArchiveReader ");

            try
            {
                _ArchiveReader.Detach();
            }
            catch (Exception Ex)
            {
                Log("Exception Detaching the archive Reader " + Ex.Message);
            }
            Log(functionName + ": Done DetachArchiveReader ");
        }

        private static string ConvertToolSignalName(String ToolSignalName)
        {
            // This regex will replace all non-alphanumeric characters with a blank 
            string thisSignal = Regex.Replace(ToolSignalName, "[^a-zA-Z0-9 -]", "");
            // This regex will replace all space caracters with blank
            thisSignal = Regex.Replace(thisSignal, @"\s+", "");
            // This is to fix sensors that begin with a numeric character(i.e. 3Sigma gets converted to N_3Sigma).
            if (Regex.IsMatch(ToolSignalName, @"^\d"))
                thisSignal = String.Concat("N", thisSignal);

            return thisSignal;
        }

        private static uint GetSignalID(string DCPlanDataItemName, Dictionary<string, UInt32> Sensors)
        {
            uint SignalID = 0;


            foreach (string Sensor in Sensors.Keys)
            {
                string thisSensor = ConvertToolSignalName(Sensor);
                if (thisSensor == DCPlanDataItemName)
                {
                    SignalID = Sensors[Sensor];
                    break;
                }
            }
            return SignalID;
        }
        private static Dictionary<string, string> ReadDataFile(DataRow row,string FileType)
        {
            Dictionary<string, string> ParameterNameValueTable = new Dictionary<string, string>();
            StreamReader sr = null;
            //HeaderArray.Clear();
            Stopwatch RunProcessingTimer = new Stopwatch();
            ArrayList oProcessEventArrayList = new ArrayList();

            //string FileName = row[11].ToString();
            string FileName = row["FileName"].ToString();
            RunProcessingTimer.Start();
            try
            {
                Log(String.Concat("Opening file ", FileName));

                if (File.Exists(FileName))
                {
                    FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                    sr = new StreamReader(fs);
                    string line = "";
                    string[] thisline;
                    while (sr.Peek() != -1)
                    {
                        line = sr.ReadLine();
                        if (line.Contains("Signal Statistics Report") && FileType == "Implant")
                        {
                            while (sr.Peek() != -1)   // Read the Data section of the file
                            {
                                line = sr.ReadLine();
                                if (line.Contains("Parameter") && line.Contains("Min") && line.Contains("Mean") && line.Contains("Max") && line.Contains("Std Dev"))
                                {
                                    line = line.Replace("Std Dev", "Std_Dev");
                                    RegexOptions options = RegexOptions.None;
                                    Regex regex = new Regex(@"[ ]{1,}", options);//this regex replaces one or more blank chars with a single char
                                    string temp = line.Trim();
                                    temp = regex.Replace(temp, ",");
                                    thisline = temp.Split(',');
                                    int loc = 0;
                                    int MeanLoc = 0;
                                    int StdDevLoc = 0;
                                    int ParameterLoc = 0;
                                    foreach (string Header in thisline)
                                    {
                                        if (Header == "Mean")
                                            MeanLoc = loc;
                                        else if(Header == "Std_Dev")
                                            StdDevLoc = loc;
                                        else if (Header == "Parameter")
                                            ParameterLoc = loc;
                                        loc += 1;
                                    }
                                    line = sr.ReadLine();
                                    Log("****************************");
                                    while (sr.Peek() != -1)   // Read the Data section of the file
                                    {

                                        line = sr.ReadLine();
                                        temp = line.Trim();
                                        options = RegexOptions.None;
                                        regex = new Regex(@"[-]{2,}", options);
                                        temp = regex.Replace(temp, "EndOfSection");
                                        if (temp != "EndOfSection")
                                        {
                                            regex = new Regex(@"[ ]{2,}", options);//this regex replaces more than two chars with a single char
                                            temp = regex.Replace(temp, ",");

                                            thisline = temp.Split(',');
                                            string ParamName = ConvertToolSignalName(thisline[ParameterLoc]) + "_Mean";
                                            string ParamValue = thisline[MeanLoc].Trim().Split(' ')[0];
                                            if (!ParameterNameValueTable.ContainsKey(ParamName))
                                            {
                                                ParameterNameValueTable.Add(ParamName, ParamValue);
                                            }

                                            ParamName = ConvertToolSignalName(thisline[ParameterLoc]) + "_StdDev";
                                            ParamValue = thisline[StdDevLoc].Trim().Split(' ')[0];
                                            if (!ParameterNameValueTable.ContainsKey(ParamName))
                                            {
                                                ParameterNameValueTable.Add(ParamName, ParamValue);
                                            }

                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                    }
                    else if (line.Contains("Parameter") && line.Contains("Start") && line.Contains("End") && FileType == "BeamSetup")
                    {
                        RegexOptions options = RegexOptions.None;
                        Regex regex = new Regex(@"[ ]{1,}", options);//this regex replaces one or more blank chars with a single char
                        string temp = line.Trim();
                        temp = regex.Replace(temp, ",");
                        thisline = temp.Split(',');
                        int loc = 0;
                        int StartLoc = 0;
                        int EndLoc = 0;
                        int ParameterLoc = 0;
                        foreach (string Header in thisline)
                        {
                            if (Header == "Start")
                                StartLoc = loc;
                            else if (Header == "End")
                                EndLoc = loc;
                            else if (Header == "Parameter")
                                ParameterLoc = loc;
                            loc += 1;
                        }

                        line = sr.ReadLine();

                        while (sr.Peek() != -1)   // Read the Data section of the file
                        {

                            line = sr.ReadLine();
                            temp = line.Trim();
                            options = RegexOptions.None;
                            regex = new Regex(@"[-]{2,}", options);
                            temp = regex.Replace(temp, "EndOfSection");
                            if (temp != "EndOfSection")
                            {
                                regex = new Regex(@"[ ]{2,}", options);//this regex replaces more than two chars with a single char
                                temp = regex.Replace(temp, ",");

                                thisline = temp.Split(',');
                                string ParamName = ConvertToolSignalName(thisline[ParameterLoc]);


                                string ParamValue = thisline[EndLoc].Trim().Split(' ')[0];
                                    if (!ParameterNameValueTable.ContainsKey(ParamName))
                                    {
                                        ParameterNameValueTable.Add(ParamName, ParamValue);
                                    }


                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    }// end while(sr.Peek() != -1)
                    LogStats("Time(miliseconds) to Read the file " + FileName + ": " + RunProcessingTimer.ElapsedMilliseconds);
                    RunProcessingTimer.Stop();
                    sr.Close();
                }
                else
                    Log(String.Concat("File " + FileName + " not found"));
            }
            catch (Exception Ex3)
            {
                Log(String.Concat("Error in ReadDataFile ", Ex3.Message));
                sr.Close();
            }
            return ParameterNameValueTable;
        }

        private static void SendDataLoaderReport(String EqpName, string toolName, DataTable DataLoaderReport)
        {

            string functionName = "SendDataLoaderReport";
            Log(functionName + "Sending data loader report");

            try
            {
                string[] selectedColumns = (string[])DCPlanDataItems[toolName].ToArray(typeof(string));

                DataRow[] query = DataLoaderReport.Select("", "");



                if (query.Length > 0)
                {

                    int rowcount = 0;
                    int QueryLength = query.Length;
                    IBatchAlarmEventData data = new BatchAlarmEventData();
                    string[][] values = new string[QueryLength][];
                    DateTime[] ts = new DateTime[QueryLength];
                    foreach (DataRow thisRow in query)
                    {
                        ts[rowcount] = new DateTime();
                        values[rowcount] = new string[selectedColumns.Length];
                        ts[rowcount] = (DateTime)thisRow[DataLoaderReport.Columns["DateTime"].Ordinal];
                        int vidcount = 0;
                        foreach (string SelectedColumn in selectedColumns)
                        {
                            values[rowcount][vidcount] = thisRow[DataLoaderReport.Columns[SelectedColumn].Ordinal].ToString();
                            vidcount++;
                        }
                        data.VariableID_Names = selectedColumns;
                        data.Values = values;
                        data.TimeStamps = ts;
                        rowcount++;
                    }
                    Log(functionName + ": Sending Data...");
                    _adapterSDK.SendBatchAlarmEventData(EqpName, toolName, data);
                }
            }
            catch (Exception Ex)
            {
                Log(String.Concat("Error in Send DataLoaderReport ", Ex.Message));
            }

        }
        private static Boolean SendArchiveData(String EqpName, string toolName, Int64 StartArchiveRecordNumber, Int64 EndArchiveRecordNumber, DateTime WaferImplantStartTime, DateTime WaferImplantEndTime, Dictionary<string, string> ContextVariableNameValue, DirectoryInfo Datedir)
        {
            Boolean RetStatus = false;
            string functionName = "SendArchiveData";

            if ((CreateCSVOnly || CreateCSVAndLoad) && !LoadOnly)
                fSignalData = File.OpenWrite(SignalDataDir + "SignalData_" + Datedir.Parent.Name + "_" + Datedir.Name + "_" + toolName + "_" + ContextVariableNameValue["WaferID"] + ".csv");

            try
            {
                Log(functionName + ":Searching archive data from record number " + StartArchiveRecordNumber.ToString() + " to " + EndArchiveRecordNumber.ToString());
                string[] tmp_selectedColumns = (string[])DCPlanDataItems[toolName].ToArray(typeof(string));
                ArrayList ReconciledselectedColumns = new ArrayList();
                foreach (string ColumnName in tmp_selectedColumns)
                {
                    if (ArchiveDataTable.Columns.Contains(ColumnName))
                        ReconciledselectedColumns.Add(ColumnName);
                }

                string[] selectedColumns = (string[])ReconciledselectedColumns.ToArray(typeof(string));
                DataTable ToolNameArchiveData = new DataView(ArchiveDataTable).ToTable(false, selectedColumns);

                string selectionCriteria = "RecordNumber >=  " + StartArchiveRecordNumber + " and " + "RecordNumber <= " + EndArchiveRecordNumber;

                DataRow[] query = ToolNameArchiveData.Select(selectionCriteria, "");



                if (query.Length > 0)
                {
                    if ((CreateCSVOnly || CreateCSVAndLoad) && !LoadOnly)
                    {
                        string Header = "ToolID,PPID,WaferID,ImplantID,LotID,CarrierID,Chamber,LoadPort,LoadLock,";
                        foreach (string SelectedColumn in selectedColumns)
                        {
                            Header += SelectedColumn + ",";
                        }
                        writeSignalData(Header.Trim(','));
                    }

                    string ContextValues = "";
                    foreach (string Context in ContextVariableNameValue.Keys)
                    {
                        ContextValues += ContextVariableNameValue[Context] + ",";
                    }

                    Log(functionName + ":Found archive data from record number " + StartArchiveRecordNumber.ToString() + " to " + EndArchiveRecordNumber.ToString());

                    RetStatus = true;
                    if (!CreateCSVOnly)
                        SendStartOfScan(EqpName, toolName, DCPlans, WaferImplantStartTime.AddMilliseconds(-10), ContextVariableNameValue);

                    int rowcount = 0;
                    int QueryLength = query.Length;

                    IBatchAlarmEventData data = new BatchAlarmEventData();
                    string[][] values = new string[QueryLength][];
                    DateTime[] ts = new DateTime[QueryLength];
                    foreach (DataRow thisRow in query)
                    {
                        ts[rowcount] = new DateTime();
                        values[rowcount] = new string[selectedColumns.Length];
                        ts[rowcount] = (DateTime)thisRow[ToolNameArchiveData.Columns["DateTime"].Ordinal];



                        int vidcount = 0;

                        foreach (string SelectedColumn in selectedColumns)
                        {
                            switch (SelectedColumn)
                            {
                                case "CarrierID":
                                case "PPID":
                                case "WaferID":
                                case "ImplantID":
                                case "ToolID":
                                case "Chamber":
                                case "LoadPort":
                                case "LoadLock":
                                case "Slot":
                                    values[rowcount][vidcount] = ContextVariableNameValue[SelectedColumn];
                                    break;
                                case "DateTime":
                                    values[rowcount][vidcount] = ts[rowcount].ToString("yyyy/mm/dd hh:mm:ss.ffffff");
                                    break;
                                default:
                                    values[rowcount][vidcount] = thisRow[ToolNameArchiveData.Columns[SelectedColumn].Ordinal].ToString();
                                    break;
                            }

                            vidcount++;


                        }
                        data.VariableID_Names = selectedColumns;
                        data.Values = values;
                        data.TimeStamps = ts;
                        rowcount++;
                    }
                    Log(functionName + ": Sending " + rowcount.ToString() + "records to E3 ...");
                    if (!CreateCSVOnly)
                    {
                        _adapterSDK.SendBatchAlarmEventData(EqpName, toolName, data);
                        SendEndOfScan(EqpName, toolName, DCPlans, WaferImplantEndTime.AddMilliseconds(+10), ContextVariableNameValue);
                        Thread.Sleep(MaxDelayBetweenWafers);
                    }

                    if ((CreateCSVOnly || CreateCSVAndLoad) && !LoadOnly)
                    {
                        for (int row = 0; row < data.Values.Length; row++)
                        {
                            string line = ContextValues;
                            for (int column = 0; column < data.Values[0].Length; column++)
                            {
                                line += data.Values[row][column] + ",";
                            }
                            writeSignalData(line.TrimEnd(','));

                        }
                    }
                }
                else
                {
                    Log("No data found is the given record range: " + "RecordNumber >=  " + StartArchiveRecordNumber + " and " + "RecordNumber <= " + EndArchiveRecordNumber);
                }
            }
            catch (Exception Ex)
            {
                Log(String.Concat("Error in SendArchiveData ", Ex.Message, " Source of exception: ", Ex.Source));
            }

            if ((CreateCSVOnly || CreateCSVAndLoad) && !LoadOnly)
                fSignalData.Close();
            return RetStatus;
        }
        private static Boolean SendData(String EqpName, string toolName,  Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans,DateTime TimeStamp,Dictionary<string, string> ContextVariableNameValue,Dictionary<string, string> ParameterNameValue)
        {
            Boolean RetStatus = false;
            string functionName = "SendData";

            try
            {
                string[] tmp_selectedColumns = (string[])DCPlanDataItems[toolName].ToArray(typeof(string));
                ArrayList ReconciledselectedColumns = new ArrayList();
                foreach (string ColumnName in tmp_selectedColumns)
                {
                    if (ParameterNameValue.ContainsKey(ColumnName))
                        ReconciledselectedColumns.Add(ColumnName);
                }

                string[] selectedColumns = (string[])ReconciledselectedColumns.ToArray(typeof(string));

                

                IBatchAlarmEventData data = new BatchAlarmEventData();
                string[][] values = new string[1][];
                DateTime[] ts = new DateTime[1];
                ts[0] = TimeStamp.AddSeconds(1);
                values[0] = new string[selectedColumns.Length];

                

                int vidcount = 0;

                foreach (string SelectedColumn in selectedColumns)
                {
                    switch (SelectedColumn)
                    {
                        case "PPID":
                        case "CarrierID":
                        case "Chamber":
                        case "LoadLock":
                        case "LoadPort":
                        case "WaferID":
                        case "ImplantID":
                        case "Slot":
                        case "BeamSetupID":
                        case "SetupResult":
                        case "BeamSetupType":
                        case "ToolID":
                        case "Specie":
                            values[0][vidcount] = ContextVariableNameValue[SelectedColumn];
                            break;
                        case "DateTime":
                            values[0][vidcount] = ts[1].ToString("yyyy/mm/dd hh:mm:ss.ffffff");
                            break;
                        default:
                            values[0][vidcount] = ParameterNameValue[SelectedColumn].ToString();
                            break;
                    }
                    vidcount++;
                }
                data.VariableID_Names = selectedColumns;
                data.Values = values;
                data.TimeStamps = ts;
                Log(functionName + ": Sending Data...");
                _adapterSDK.SendBatchAlarmEventData(EqpName, toolName, data);
            }
            catch (Exception Ex)
            {
                Log(String.Concat("Error in SendArchiveData ", Ex.Message, " Source of exception: ", Ex.Source));
            }
            return RetStatus;
        }
        private static void SendStartOfReport(string EqpName, string ToolName, DateTime ReportStartTime, Dictionary<string, string> ContextVariableNameValue)
        {
            string functionName = "SendStartOfReport";
            Log(functionName + ": Sending StartOfReport");
            try
            {
                string StartReportEventID = "";
                string[] StartTriggerVIDList = new string[1];

                GetStartRunInfo(ToolName, DCPlans, ref  StartReportEventID, ref StartTriggerVIDList);

                IBatchAlarmEventData data = new BatchAlarmEventData();
                data.Events = new IAlarmEvents[1];
                data.Events[0] = new AlarmEvents();
                data.Events[0].TimeStamp = ReportStartTime;

                data.Events[0].AlarmEventID_Name = StartReportEventID;


                data.Events[0].VariableID_NameValues = new Dictionary<string, string>();

                foreach (string StartTriggerVID in StartTriggerVIDList)
                {
                    data.Events[0].VariableID_NameValues.Add(StartTriggerVID, ContextVariableNameValue[StartTriggerVID]);
                }


                _adapterSDK.SendBatchAlarmEventData(EqpName, ToolName, data);
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " : " + Ex.Message);
            }



        }
        private static void SendDataLoaderIdle(string EqpName, string ToolName, DateTime ReportEndTime)
        {
            string functionName = "SendDataLoaderIdle";
            Log(functionName + ": Sending SendDataLoaderIdle");

            try
            {
                string[] DataLoaderIdleVIDList = new string[1];


                IBatchAlarmEventData data = new BatchAlarmEventData();
                data.Events = new IAlarmEvents[1];
                data.Events[0] = new AlarmEvents();
                data.Events[0].TimeStamp = ReportEndTime;

                data.Events[0].AlarmEventID_Name = "88";


                data.Events[0].VariableID_NameValues = new Dictionary<string, string>();

                _adapterSDK.SendBatchAlarmEventData(EqpName, ToolName, data);
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " : " + Ex.Message);
            }

        }

        private static void SendEndOfReport(string EqpName, string ToolName, DateTime ReportEndTime, Dictionary<string, string> ContextVariableNameValue)
        {
            string functionName = "SendEndOfReport";
            Log(functionName + ": Sending EndOfReport");

            try
            {
                string EndReportEventID = "";
                string[] EndTriggerVIDList = new string[1];

                GetEndRunInfo(ToolName, DCPlans, ref  EndReportEventID, ref EndTriggerVIDList);

                IBatchAlarmEventData data = new BatchAlarmEventData();
                data.Events = new IAlarmEvents[1];
                data.Events[0] = new AlarmEvents();
                data.Events[0].TimeStamp = ReportEndTime;

                data.Events[0].AlarmEventID_Name = EndReportEventID;


                data.Events[0].VariableID_NameValues = new Dictionary<string, string>();
                foreach (string EndTriggerVID in EndTriggerVIDList)
                {
                    data.Events[0].VariableID_NameValues.Add(EndTriggerVID, ContextVariableNameValue[EndTriggerVID]);
                }

                _adapterSDK.SendBatchAlarmEventData(EqpName, ToolName, data);
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " : " + Ex.Message);
            }

        }

        private static void SendStartOfScan(string EqpName, string ToolName, Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans, DateTime WaferImplantStartTime, Dictionary<string, string> ContextVariableNameValue)
        {
            string functionName = "SendStartOfScan";
            Log(functionName + ": Sending StartofScan");
            try
            {
                string StartScanEventID = "";
                string[] StartTriggerVIDList = new string[1];

                GetStartRunInfo(ToolName, DCPlans, ref  StartScanEventID, ref StartTriggerVIDList);
                Log(functionName + ": StartScanEventID = " + StartScanEventID);
                Log(functionName + ": StartTriggerVIDList count = " + StartTriggerVIDList.Length.ToString());
                Log(functionName + ": ContextVariableNameValue count = " + ContextVariableNameValue.Count.ToString());

                IBatchAlarmEventData data = new BatchAlarmEventData();
                data.Events = new IAlarmEvents[1];
                data.Events[0] = new AlarmEvents();
                data.Events[0].TimeStamp = WaferImplantStartTime;

                data.Events[0].AlarmEventID_Name = StartScanEventID;


                data.Events[0].VariableID_NameValues = new Dictionary<string, string>();

                foreach (string StartTriggerVID in StartTriggerVIDList)
                {
                    if (ContextVariableNameValue.ContainsKey(StartTriggerVID))
                        data.Events[0].VariableID_NameValues.Add(StartTriggerVID, ContextVariableNameValue[StartTriggerVID]);
                }


                _adapterSDK.SendBatchAlarmEventData(EqpName, ToolName, data);
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " : " + Ex.Message);
            }


        }

        private static void SendEndOfScan(string EqpName, string ToolName, Dictionary<string, List<EqDataCollectionPlanInfo>> DCPlans, DateTime WaferImplantEndTime, Dictionary<string, string> ContextVariableNameValue)
        {
            string functionName = "SendEndOfScan";
            Log(functionName + ": Sending EndOfScan");

            try
            {
                string EndScanEventID = "";
                string[] EndTriggerVIDList = new string[1];

                GetEndRunInfo(ToolName, DCPlans, ref  EndScanEventID, ref EndTriggerVIDList);

                IBatchAlarmEventData data = new BatchAlarmEventData();
                data.Events = new IAlarmEvents[1];
                data.Events[0] = new AlarmEvents();
                data.Events[0].TimeStamp = WaferImplantEndTime;

                data.Events[0].AlarmEventID_Name = EndScanEventID;


                data.Events[0].VariableID_NameValues = new Dictionary<string, string>();
                foreach (string EndTriggerVID in EndTriggerVIDList)
                {
                    if (ContextVariableNameValue.ContainsKey(EndTriggerVID))
                        data.Events[0].VariableID_NameValues.Add(EndTriggerVID, ContextVariableNameValue[EndTriggerVID]);
                }

                _adapterSDK.SendBatchAlarmEventData(EqpName, ToolName, data);
            }
            catch (Exception Ex)
            {
                Log("Exception in " + functionName + " : " + Ex.Message);
            }



        }



        private static void AddProcessEvent(string vsEventName, string vsProcessEventXmlData, DateTime TimeStamp, ref ArrayList roProcessEventArrayList)
        {
            try
            {
                //Log("Attempting to add event: " + vsEventName);

                ProcessEvent oEvent = new ProcessEvent();
                oEvent.EventName = vsEventName;
                oEvent.EventTimeStamp = TimeStamp;
                oEvent.ProcessEventXmlData = vsProcessEventXmlData;

                roProcessEventArrayList.Add(oEvent);

                //Log("Successfully added event");
            }
            catch (Exception ex)
            {
                Log("Exception in AddProcessEvent method: " + ex.Message);
            }
        }

        private static void AddElementWithBlanks(XmlDocument doc, XmlNode eventNode, string ElementName, string ChildElementName, int count)
        {
            XmlParser.AddElement(doc, eventNode, ElementName, null, null);
            XmlNode node = doc.GetElementsByTagName(ElementName).Item(0);
            XmlElement stringele = doc.CreateElement("string");
            XmlNode child = node.AppendChild(stringele);

            for (int i = 0; i < count; i++)
            {
                XmlElement ele = doc.CreateElement(ChildElementName);
                //node.AppendChild(ele);
                child.AppendChild(ele);
            }
        }

        private static string GetXMLBlanks(string nodename, int count)
        {
            string retString = "";

            for (int i = 0; i < count; i++)
            {
                retString = retString + "<" + nodename + " />";
            }

            return retString;
        }

        private static void UpdateParameter(XmlDocument doc, string ParameterName, int WaferPosition, string DataValue)
        {
            //Log("Starting UpdateParameter method for [" + ParameterName + "]...");
            XmlNodeList oParameterElementNodeList = doc.GetElementsByTagName(ParameterName);
            if (oParameterElementNodeList.Count == 1)
            {
                XmlNode oParameterElement = oParameterElementNodeList[0];
                XmlNode oParameterElement2 = oParameterElement.FirstChild;
                //Log("nameis: " + oParameterElement2.Name);
                //Log("values: " + oParameterElement2.InnerXml);
                //oParameterElement.ChildNodes[WaferPosition].InnerXml = DataValue;
                oParameterElement2.ChildNodes[WaferPosition].InnerXml = DataValue;
            }
            else
            {
                Log("Parameter not found");
            }
        }
    }
}





//        private static void CheckDataFileFolder(string dataFileFolder)
//        {
//            try
//            {
//                string[] files = GetAllFilesInFolder(dataFileFolder);
//                if (files != null && files.Length > 0)
//                {
//                    foreach (String file in files)
//                    {
//                        if (File.Exists(file))
//                        {
//                            FileInfo fi = new FileInfo(file);
//                            if (fi.Extension.Equals(".csv"))
//                            {

//                            }
//                        }
//                    }
//                }
//            }

//            catch (Exception ex)
//            {
//                Log("Exception check " + dataFileFolder + ":" + ex.Message);
//            }
//        }


//// *** Get All Filenames in Folder ***
//private static string[] GetAllFilesInFolder(string path)
//{
//    string[] filenames = null;

//    if (Directory.Exists(path))
//    {
//        filenames = Directory.GetFileSystemEntries(path);
//    }
//    return filenames;
//}

