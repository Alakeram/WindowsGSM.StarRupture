using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace WindowsGSM.Plugins
{
    public class StarRupture : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.StarRupture", // WindowsGSM.XXXX
            author = "Alakeram",
            description = "WindowsGSM plugin for StarRupture",
            version = "1.6",
            url = "https://github.com/Alakeram/WindowsGSM.StarRupture", // Github repository link (Best practice)
            color = "#035c00" // Color Hex
        };

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "3809400"; // Game server appId Steam

        // - Standard Constructor and properties
        public StarRupture(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;

        // - Game server Fixed variables
        public override string StartPath => @"StarRupture\Binaries\Win64\StarRuptureServerEOS-Win64-Shipping.exe"; // Game server start path
        public string FullName = "StarRupture Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()

        // - Game server default values
        public string Port = "7777"; // Default port
        public string QueryPort = "27015"; // Default query port. This is the port specified in the Server Manager in the client UI to establish a server connection.
		public string Defaultmap = "Default";
		public string Maxplayers = "4"; // Default maxplayers
		public string Additional = "-log"; // Additional server start parameter

        // - This will create a config after install and check within the server start to update the values in case it is existing or missing.
		public Task<bool> CreateServerCFG()
		{
			try
			{
				string exeDir = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID);
				
				// Build SaveGames directory path
				string saveGamesDir = Path.Combine(exeDir,"StarRupture","Saved","SaveGames");
				
				if (!Directory.Exists(exeDir))
					return Task.FromResult(false);

				if (string.IsNullOrWhiteSpace(exeDir))
					return Task.FromResult(false);
				
				string cfgPath = Path.Combine(exeDir, "DSSettings.txt");
				Notice = $"Writing DSSettings.txt to: {cfgPath}";
				
				// Create Password.json
				string passwordPath = Path.Combine(exeDir, "Password.json");
				if (!File.Exists(passwordPath))
				{
					string passwordJson =
				@"{
				  ""Password"": """"
				}";
					File.WriteAllText(passwordPath, passwordJson, new UTF8Encoding(false));
				}

				// Create PlayerPassword.json
				string playerPasswordPath = Path.Combine(exeDir, "PlayerPassword.json");
				if (!File.Exists(playerPasswordPath))
				{
					string playerPasswordJson =
				@"{
				  ""Password"": """"
				}";
					File.WriteAllText(playerPasswordPath, playerPasswordJson, new UTF8Encoding(false));
				}
				
				string baseName = string.IsNullOrWhiteSpace(_serverData.ServerName) ? $"StarRuptureDedicated_{_serverData.ServerID}" : _serverData.ServerName;
				
				// Remove the spaces in the server name.
				baseName = baseName.Replace(" ","") ;
				
				//Makes sure there is no special characters that can cause an issue with the creation of the session.
				baseName = Regex.Replace(baseName, @"[^a-zA-Z0-9_]", "");
				
				//If the hostname exceeds the 20 character limit, stop it at 20 characters.
					if(baseName.Length > 20)
						baseName= baseName.Substring(0, 20);
				
				string newSessionName = baseName;
				
				// Default first-run config
				string defaultJson =
		$@"{{
		  ""SessionName"": ""{newSessionName}"",
		  ""SaveGameInterval"": ""300"",
		  ""StartNewGame"": ""true"",
		  ""LoadSavedGame"": ""false"",
		  ""SaveGameName"": ""AutoSave0.sav""
		}}".Trim();

				// If file does not exist create with StartNewGame=true
				if (!File.Exists(cfgPath))
				{
					File.WriteAllText(cfgPath, defaultJson, new UTF8Encoding(false));
					return Task.FromResult(true);
				}
				
				//Check if the save game directory exists.
				if (!Directory.Exists(saveGamesDir))
				{
					Notice = "No SaveGames directory yet. Keeping StartNewGame=true.";
					return Task.FromResult(true);
				}
				
				string json = File.ReadAllText(cfgPath);
				bool alreadyModified = json.Contains(@"""StartNewGame"": ""false""") && json.Contains(@"""LoadSavedGame"": ""true""");
				
				if (alreadyModified)
				{
					Notice = "DSSettings.txt already set to load existing save. No changes needed.";
					return Task.FromResult(true);
				}
				
				// Not already modified + save directory exist, flip values
				Notice = "SaveGames directory found. Switching DSSettings to load mode.";
				json = json.Replace(@"""StartNewGame"": ""true""",  @"""StartNewGame"": ""false""");
				json = json.Replace(@"""LoadSavedGame"": ""false""", @"""LoadSavedGame"": ""true""");
				File.WriteAllText(cfgPath, json, new UTF8Encoding(false));
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Error = ex.Message;
				return Task.FromResult(false);
			}
		}

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string shipExePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(shipExePath))
            {
                Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                return null;
            }
			
			bool ok = await CreateServerCFG();
			if(!ok)
			{
				Error = "Failed to create DSSettings.txt";
				return null;
			}
            // Prepare start parameter
            string param = "-log"; //Possibly not needed, keeping here in case.
            param += $" {_serverData.ServerParam}";
            param += string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $" -Port={_serverData.ServerPort}"; 
            param += string.IsNullOrWhiteSpace(_serverData.ServerQueryPort) ? string.Empty : $" -ServerQueryPort={_serverData.ServerQueryPort}";
            param += string.IsNullOrWhiteSpace(_serverData.ServerMaxPlayer) ? string.Empty : $" -MaxPlayers={_serverData.ServerMaxPlayer}";
            param += string.IsNullOrWhiteSpace(_serverData.ServerIP) ? string.Empty : $" -Multihome={_serverData.ServerIP}";
			param += string.IsNullOrWhiteSpace(_serverData.ServerName) ? string.Empty : $" -ServerName={_serverData.ServerName}";

            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = shipExePath,
                    Arguments = param,
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = false,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
            }

            // Start Process
            try
            {
                p.Start();
                if (AllowsEmbedConsole)
                {
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                }
                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }
        }


// - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                Functions.ServerConsole.SendWaitToMainWindow("^c");
                p.WaitForExit(20000);
            });
        }

// fixes WinGSM bug, https://github.com/WindowsGSM/WindowsGSM/issues/57#issuecomment-983924499
        public async Task<Process> Update(bool validate = false, string custom = null)
        {
            var (p, error) = await Installer.SteamCMD.UpdateEx(serverData.ServerID, AppId, validate, custom: custom, loginAnonymous: loginAnonymous);
            Error = error;
            await Task.Run(() => { p.WaitForExit(); });
            return p;
        }

    }
}
