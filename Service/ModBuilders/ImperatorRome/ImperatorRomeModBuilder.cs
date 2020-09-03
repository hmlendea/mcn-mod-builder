using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.ImperatorRome
{
    public sealed class ImperatorRomeModBuilder : ModBuilder, IImperatorRomeModBuilder
    {
        const string GameVersion = "1.4.*";

        public override string Game => "ImperatorRome";

        public ImperatorRomeModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
        {
        }

        protected override void BuildMod()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.ImperatorRomeModId);
            string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization");
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string provinceNamesDirectoryPath = Path.Combine(commonDirectoryPath, "province_names");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(localisationDirectoryPath);
            Directory.CreateDirectory(provinceNamesDirectoryPath);

            Directory.CreateDirectory(commonDirectoryPath);

            CreateDataFiles(provinceNamesDirectoryPath);
            CreateLocalisationFiles(localisationDirectoryPath);
            CreateDescriptorFiles();
        }

        void CreateDataFiles(string provinceNamesDirectoryPath)
        {
            List<Localisation> localisations = GetLocalisations();

            foreach (string culture in localisations.Select(x => x.LanguageId).Distinct())
            {
                string path = Path.Combine(provinceNamesDirectoryPath, $"{culture.ToLower()}.txt");
                string content = $"{culture} = {{" + Environment.NewLine;

                foreach (Localisation localisation in localisations.Where(x => x.LanguageId == culture))
                {
                    content +=
                        $"    {localisation.LocationId} = PROV{localisation.LocationId}_{culture}" +
                        $" # {localisation.Name}" + Environment.NewLine;
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
            string fileName = $"{outputSettings.ImperatorRomeModId}_provincenames_l_{language}.yml";
            string filePath = Path.Combine(localisationDirectoryPath, fileName);

            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
        }

        void CreateDescriptorFiles()
        {
            string fileContent = GenerateDescriptorFileContent();

            string descriptorFile1Path = Path.Combine(OutputDirectoryPath, $"{outputSettings.ImperatorRomeModId}.mod");
            string descriptorFile2Path = Path.Combine(OutputDirectoryPath, outputSettings.ImperatorRomeModId, "descriptor.mod");

            File.WriteAllText(descriptorFile1Path, fileContent);
            File.WriteAllText(descriptorFile2Path, fileContent);
        }

        string GenerateLocalisationFileContent(string language)
        {
            List<Localisation> localisations = GetLocalisations();
            string content = $"l_{language}:{Environment.NewLine}";

            foreach(Localisation localisation in localisations)
            {
                content += $" PROV{localisation.LocationId}_{localisation.LanguageId}:0 \"{localisation.Name}\"{Environment.NewLine}";
            }

            return content;
        }

        string GenerateDescriptorFileContent()
        {
            return
                $"version=\"{GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"}}" + Environment.NewLine +
                $"name=\"{outputSettings.ImperatorRomeModName}\"" + Environment.NewLine +
                $"path=\"mod/{outputSettings.ImperatorRomeModId}\"";
        }
    }
}
