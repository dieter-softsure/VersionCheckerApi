using VersionCheckerApi.Analysing.RepoGetter.Devops;
using VersionCheckerApi.Persistence.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using File = VersionCheckerApi.Analysing.RepoGetter.File;

namespace VersionCheckerApi.Analysing.Pipelines
{
    public class PipelineAnalyzer
    {
        private readonly IDeserializer _deserializer;

        public PipelineAnalyzer()
        {
            _deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        public async Task<List<Pipeline>> GetOrUpdatePipelineData(string projectId, Repository repo, List<Pipeline>? oldPipelines)
        {
            oldPipelines ??= new List<Pipeline>();
            var getter = (DevopsRepositoryGetter)repo.Getter;
            var pipelines = await getter.GetPipelines(projectId, repo.Id);

            var ymlFilePaths = repo.Paths.Where(f => pipelines.Any(p => f.EndsWith(p.file))).ToList();
            var ymlFiles = await getter.GetFiles(repo, ymlFilePaths);

            for (var i = 0; i < ymlFiles.Count; i++)
            {
                var oldPipeline = oldPipelines.FirstOrDefault(o => o.Name == pipelines[i].name);
                var pipeline = await CreatePipelineFromFile(getter, pipelines[i].name, ymlFiles[i], oldPipeline);
                if (pipeline != null)
                    oldPipelines.Add(pipeline);
            }

            oldPipelines = oldPipelines.Where(o => pipelines.Any(p => p.name == o.Name)).ToList();

            return oldPipelines;
        }

        private async Task<Pipeline?> CreatePipelineFromFile(DevopsRepositoryGetter getter, string name, File file, Pipeline? oldPipeline)
        {
            PipelineYaml newPipeline;
            try
            {
                newPipeline = _deserializer.Deserialize<PipelineYaml>(file.Content);
            }
            catch
            {
                return null; // ignore when file is unreadable
            }

            oldPipeline ??= new Pipeline
            {
                Name = name
            };

            oldPipeline.Agent = newPipeline.Pool?.Name ?? newPipeline.Queue?.Name ?? "default";
            if (oldPipeline.Agent != "default") return oldPipeline;

            oldPipeline.VmImage = newPipeline.Jobs?[0].Pool?.VmImage ??
                                  newPipeline.Jobs?[0].Pool?.Vmimage ??
                                  newPipeline.Pool?.VmImage ??
                                  await GetVmImageFromTemplate(getter, file);

            return oldPipeline;
        }

        private async Task<string> GetVmImageFromTemplate(DevopsRepositoryGetter getter, File file)
        {
            PipelineYamlFull yaml;
            try
            {
                yaml = _deserializer.Deserialize<PipelineYamlFull>(file.Content);
            }
            catch
            {
                return "unknown"; // ignore when file is unreadable
            }

            var repo = await getter.GetRepositoryByName(yaml.Resources!.Repositories![0].Repository, yaml.Resources!.Repositories![0].Ref);
            var fileEnd = yaml.Jobs![0].Template!.Split('@')[0];
            var ymlFilePath = repo.Paths.First(p => p.EndsWith(fileEnd));
            var f =  await getter.GetFile(repo, ymlFilePath);
            var pipeline = _deserializer.Deserialize<PipelineYaml>(f.Content);
            return pipeline.Jobs?[0].Pool?.VmImage ?? pipeline.Pool!.VmImage ?? "Unknown";
        }
    }
}
