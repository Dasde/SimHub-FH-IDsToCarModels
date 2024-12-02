using GameReaderCommon;
using Microsoft.VisualBasic.FileIO;
using SimHub.Plugins;
using System.Collections.Generic;
using System.IO;

namespace DaZD.FH.IDsToCarModels
{
    [PluginDescription("This plugin load the Car ids lookup file for Forza Horizon 4 and 5 and Forza Motorsport 7 (use the same as Forza Motorsport")]
    [PluginAuthor("DaZD")]
    [PluginName("Forza IDs to Car Models")]
    public class DataPluginIDsToCarNames : SimHub.Plugins.IPlugin, IDataPlugin
    {
        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }
        private string currentGame;
        private string currentCarModel;
        private Dictionary<int, string> currentGameLookups ;
        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            // Define the value of our property (declared in init)
            if (data.GameRunning)
            {
                if (this.currentGame != data.GameName)
                {
                    this.currentGame = data.GameName;
                    if (data.GameName == "FH5" || data.GameName == "FH4" || data.GameName == "FM7")
                    {
                        SimHub.Logging.Current.Info("Forza Game detected (" + data.GameName + ")");
                        if (currentGameLookups != null)
                            currentGameLookups.Clear();
                        currentGameLookups = LoadCarNamesCSV(data.GameName);
                        int carId = GetCarId(data.NewData.CarId);
                        if (currentGameLookups.ContainsKey(carId))
                            currentCarModel = currentGameLookups[carId];
                        else
                            currentCarModel = null;
                        pluginManager.SetPropertyValue("FHCarModel", this.GetType(), currentCarModel);
                        return;
                    }
                }
                if (data.GameName == "FH5" || data.GameName == "FH4" || data.GameName == "FM7")
                {
                    if (data.OldData != null && data.NewData != null)
                    {
                        if (data.OldData.CarId != data.NewData.CarId)
                        {
                            int carId = GetCarId(data.NewData.CarId);
                            if (currentGameLookups.ContainsKey(carId))
                                currentCarModel = currentGameLookups[carId];
                            else
                                currentCarModel = null;
          
                            pluginManager.SetPropertyValue("FHCarModel", this.GetType(), currentCarModel);
                        }
                    }
                }
            } else {
                this.currentGame = "";
            }
        }

        /// <summary>
        /// Load lookup into a dictionnary
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        private Dictionary<int,string> LoadCarNamesCSV(string gameName)
        {
            // use the same file for every forza game.
            var path = @".\LookupTables\FM8.CarNames.csv";

            Dictionary<int,string> lookupCarNames = new Dictionary<int, string>();
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "\t" });
                csvParser.HasFieldsEnclosedInQuotes = true;

                while (!csvParser.EndOfData)
                {
                    string[] fields = csvParser.ReadFields();
                    lookupCarNames[int.Parse(fields[0])] = fields[1];
                }
                csvParser.Close();
            }
            return lookupCarNames;
        }

        /// <summary>
        /// Get the car ID from the carName returned by simbhub
        /// </summary>
        /// <param name="carIdLabel"></param>
        /// <returns></returns>
        private int GetCarId(string carIdLabel)
        {
            return int.Parse(carIdLabel.Split('_')[1]);
        }

        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            if (currentGameLookups != null)
                currentGameLookups.Clear();
        }

        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return null;
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            SimHub.Logging.Current.Info("Starting plugin Forza IDs to Car Models");
            pluginManager.AddProperty("FHCarModel", this.GetType(), this.currentCarModel);
        }
    }
}