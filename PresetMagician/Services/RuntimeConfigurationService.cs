using System;
using System.IO;
using Catel;
using Catel.IoC;
using Catel.IO;
using Catel.Logging;
using Newtonsoft.Json;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Path = Catel.IO.Path;

namespace PresetMagician.Services
{
    public class RuntimeConfigurationService : IRuntimeConfigurationService
    {
        private static readonly string _defaultLocalConfigFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming),
                "configuration.json");

        private static readonly string _defaultLocalLayoutFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming), "layout.xml");

        private readonly JsonSerializer _jsonSerializer;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceLocator _serviceLocator;
        private readonly IVstService _vstService;

        private LayoutRoot _originalLayout;

        public RuntimeConfigurationService(IServiceLocator serviceLocator, IVstService vstService)
        {
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => vstService);

            RuntimeConfiguration = new RuntimeConfiguration();
            ApplicationState = new ApplicationState();

            _vstService = vstService;
            _serviceLocator = serviceLocator;
            _jsonSerializer = new JsonSerializer {Formatting = Formatting.Indented};
        }

        public RuntimeConfiguration RuntimeConfiguration { get; private set; }
        public RuntimeConfiguration EditableConfiguration { get; private set; }
        public ApplicationState ApplicationState { get; private set; }

        public void Load()
        {
            LoadConfiguration();
        }

        public void CreateEditableConfiguration()
        {
            string output = JsonConvert.SerializeObject(RuntimeConfiguration);

            EditableConfiguration = JsonConvert.DeserializeObject<RuntimeConfiguration>(output);
        }

        public void ApplyEditableConfiguration()
        {
            string output = JsonConvert.SerializeObject(EditableConfiguration);

            JsonConvert.PopulateObject(output, RuntimeConfiguration);
            SaveConfiguration();
        }

        public void LoadConfiguration()
        {
            _logger.Debug("Loading configuration");
            if (!File.Exists(_defaultLocalConfigFilePath))
            {
                _logger.Info("No configuration found.");
                return;
            }

            try
            {
                using (var rd = new StreamReader(_defaultLocalConfigFilePath))
                using (JsonReader jsonReader = new JsonTextReader(rd))
                {
                    using (RuntimeConfiguration.SuspendValidations()) {
                        _jsonSerializer.Populate(jsonReader,RuntimeConfiguration);
                    }
                    _vstService.Plugins.AddItems(RuntimeConfiguration.CachedPlugins);
                }
                _logger.Debug("Configuration loaded");
            }
            catch (Exception e)
            {
                _logger.Error("Unable to load configuration file, probably corrupt. Error:" + e.Message);
            }
        }

        public void LoadLayout()
        {
            return;
            // Disabled because loading the layout causes no documents to be active

            _originalLayout = GetDockingManager().Layout;

            if (File.Exists(_defaultLocalLayoutFilePath))
                try
                {
                    GetLayoutSerializer().Deserialize(_defaultLocalLayoutFilePath);
                }
                catch (Exception)
                {
                    // Probably something wrong with the file, ignore
                }
        }

        public void ResetLayout()
        {
            var dockingManager = GetDockingManager();

            dockingManager.Layout = _originalLayout;
        }

        public void Save(bool includeCaching = false)
        {
            SaveConfiguration(includeCaching);
            SaveLayout();
        }

        public void SaveConfiguration(bool includeCaching = false)
        {
            using (var sw = new StreamWriter(_defaultLocalConfigFilePath))
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                if (includeCaching)
                {
                    RuntimeConfiguration.CachedPlugins = _vstService.Plugins;
                }

                _jsonSerializer.Serialize(jsonWriter, RuntimeConfiguration);
            }
        }

        public void SaveLayout()
        {
            GetLayoutSerializer().Serialize(_defaultLocalLayoutFilePath);
        }

        private XmlLayoutSerializer GetLayoutSerializer()
        {
            return new XmlLayoutSerializer(GetDockingManager());
        }

        private DockingManager GetDockingManager()
        {
            return _serviceLocator.ResolveType<DockingManager>();
        }

        public bool IsConfigurationValueEqual (object left, object right)
        {
            var leftJson = JsonConvert.SerializeObject(left);
            var rightJson = JsonConvert.SerializeObject(right);

            return leftJson.Equals(rightJson);
        }
    }
}