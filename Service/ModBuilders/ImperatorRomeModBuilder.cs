using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders
{
    public sealed class ImperatorRomeModBuilder : ModBuilder
    {
        IDictionary<string, IDictionary<string, Localisation>> localisations;
            
        readonly ILocalisationFetcher localisationFetcher;

        public ImperatorRomeModBuilder(
            ILocalisationFetcher localisationFetcher,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            IRepository<TitleEntity> titleRepository,
            Settings settings)
            : base(languageRepository, locationRepository, titleRepository, settings)
        {
            this.localisationFetcher = localisationFetcher;
        }

        protected override void LoadData()
        {
            ConcurrentDictionary<string, IDictionary<string, Localisation>> concurrentLocalisations =
                new ConcurrentDictionary<string, IDictionary<string, Localisation>>();

            Parallel.ForEach(locationGameIds, locationGameId =>
            {
                IDictionary<string, Localisation> locationLocalisations = localisationFetcher
                    .GetGameLocationLocalisations(locationGameId.Id, Settings.Mod.Game)
                    .ToDictionary(x => x.LanguageGameId, x => x);

                concurrentLocalisations.TryAdd(locationGameId.Id, locationLocalisations);
            });

            localisations = concurrentLocalisations.ToDictionary(x => x.Key, x => x.Value);
        }

        protected override void GenerateFiles()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
            string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization");
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string provinceNamesDirectoryPath = Path.Combine(commonDirectoryPath, "province_names");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(commonDirectoryPath);
            Directory.CreateDirectory(localisationDirectoryPath);
            Directory.CreateDirectory(provinceNamesDirectoryPath);

            LoadData();
            CreateDataFiles(provinceNamesDirectoryPath);
            CreateLocalisationFiles(localisationDirectoryPath);
            CreateDescriptorFiles();
        }

        void CreateDataFiles(string provinceNamesDirectoryPath)
        {
            foreach (GameId languageGameId in languageGameIds)
            {
                string path = Path.Combine(provinceNamesDirectoryPath, $"{languageGameId.Id.ToLower()}.txt");
                string content = $"{languageGameId.Id} = {{" + Environment.NewLine;

                foreach (string provinceId in localisations.Keys.OrderBy(x => int.Parse(x)))
                {
                    if (!localisations[provinceId].ContainsKey(languageGameId.Id))
                    {
                        continue;
                    }

                    Localisation localisation = localisations[provinceId][languageGameId.Id];

                    content += $"    {localisation.GameId} = PROV{localisation.GameId}_{languageGameId.Id} # {localisation.Name}";

                    if (Settings.Output.AreVerboseCommentsEnabled)
                    {
                        content += $" # Language={localisation.LanguageId}";
                    }

                    if (!string.IsNullOrWhiteSpace(localisation.Comment))
                    {
                        content += $" # {localisation.Comment}";
                    }

                    content += Environment.NewLine;
                }

                content += "}";

                File.WriteAllText(path, content);
            }
        }

        void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            CreateLocalisationFile(localisationDirectoryPath, "english");
            CreateLocalisationFile(localisationDirectoryPath, "french");
            CreateLocalisationFile(localisationDirectoryPath, "german");
            CreateLocalisationFile(localisationDirectoryPath, "spanish");
        }

        void CreateLocalisationFile(string localisationDirectoryPath, string language)
        {
            string fileContent = GenerateLocalisationFileContent(language);
            string fileName = $"{Settings.Mod.Id}_provincenames_l_{language}.yml";
            string filePath = Path.Combine(localisationDirectoryPath, fileName);

            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
        }

        void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateInnerDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{Settings.Mod.Id}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id, $"descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        string GenerateLocalisationFileContent(string language)
        {
            string content = $"l_{language}:{Environment.NewLine}";

            foreach (string provinceId in localisations.Keys.OrderBy(x => int.Parse(x)))
            {
                foreach (string culture in localisations[provinceId].Keys.OrderBy(x => x))
                {
                    Localisation localisation = localisations[provinceId][culture];

                    string provinceLocalisationDefinition = $" PROV{provinceId}_{localisation.LanguageGameId}:0 \"{localisation.Name}\"";

                    if (Settings.Output.AreVerboseCommentsEnabled)
                    {
                        provinceLocalisationDefinition += $" # Language={localisation.LanguageId}";
                    }

                    if (!string.IsNullOrWhiteSpace(localisation.Comment))
                    {
                        provinceLocalisationDefinition += $" # {localisation.Comment}";
                    }

                    content += $"{provinceLocalisationDefinition}{Environment.NewLine}";
                }
            }

            return content;
        }

        string GenerateMainDescriptorContent()
        {
            return GenerateInnerDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{Settings.Mod.Id}\"";
        }

        string GenerateInnerDescriptorContent()
        {
            return
                $"# Version {Settings.Mod.Version} ({DateTime.Now})" + Environment.NewLine +
                $"name=\"{Settings.Mod.Name}\"" + Environment.NewLine +
                $"version=\"{Settings.Mod.Version}\"" + Environment.NewLine +
                $"supported_version=\"{Settings.Mod.GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"}}";
        }
    }
}