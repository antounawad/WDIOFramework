using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace Tools.FixSolution
{
    public class Solution
    {
        public List<MyProject> Projects { get; } = new List<MyProject>();
        //public ProjectCollection ProjectCollection => ProjectCollection.GlobalProjectCollection;

        public SolutionFile SolutionFile { get; }

        public Solution(SolutionFile solutionFile)
        {
            SolutionFile = solutionFile;
        }

        public static Solution ReadSlnFile(string slnFile)
        {
            //ProjectCollection.GlobalProjectCollection.DefaultToolsVersion = "15.0";

            var solutionFile = SolutionFile.Parse(slnFile);
            var sln = new Solution(solutionFile);

            sln.Projects.Clear();
            foreach (var prj in sln.SolutionFile.ProjectsInOrder.Where(t => t.ProjectType != SolutionProjectType.SolutionFolder))
            {
                Project project = null;
                //try
                //{
                project = ProjectCollection.GlobalProjectCollection.LoadProject(prj.AbsolutePath); //new Project(prj.AbsolutePath, null, "14.0");
                //}
                //catch (Exception e)
                //{
                //    Debug.Print($"Projekt '{prj.ProjectName}' nicht geladen: {e}");
                //}
                var p = new MyProject(project)
                {
                    Id = Guid.Parse(prj.ProjectGuid),
                    ParentProject = prj.ParentProjectGuid != null ? (Guid?)Guid.Parse(prj.ParentProjectGuid) : null,
                    Name = prj.ProjectName,
                    Type = prj.ProjectType,
                    AbsolutePath = prj.AbsolutePath,
                    RelativePath = prj.RelativePath,
                };
                p.Dependencies.AddRange(prj.Dependencies.Select(Guid.Parse));
                sln.Projects.Add(p);

            }
            return sln;
        }

        public void CheckDependencies()
        {
            const string projectReferenceTagName = "ProjectReference";
            const string referenceTagName = "Reference";
            var allowedHintPaths = new[] { "packages", "Externallibraries", "lib", @"Web\lib" };

            foreach (var project in Projects)
            {
                if (project.Project == null) continue;
                var references = project.Project.Items.Where(w => w.ItemType.Equals(referenceTagName)).OrderBy(o => o.EvaluatedInclude);
                var projectReferences = project.Project.Items.Where(w => w.ItemType.Equals(projectReferenceTagName)).OrderBy(o => o.EvaluatedInclude);
                foreach (var dbl in references.GroupBy(g => g.EvaluatedInclude).Where(w => w.Count() > 1))
                    Debug.WriteLine($"{project.Name}: {dbl} doppelt");
                foreach (var dbl in projectReferences.GroupBy(g => g.EvaluatedInclude).Where(w => w.Count() > 1))
                    Debug.WriteLine($"{project.Name}: {dbl} doppelt");

                //var q = project.Project.Items.GroupBy(g => g.ItemType).Select(s => s.Key);

                foreach (var dbl in project.Project.Items.Where(w => new[]{ "Compile", "Content", "None", "Resource", "Page", "EmbeddedResource", "AppDesigner", "PublishFile", "ApplicationManifest" }.Contains( w.ItemType)).GroupBy(g => g.EvaluatedInclude).Where(w => w.Count() > 1))
                    Debug.WriteLine($"{project.Name}: {dbl} doppelt");

                foreach (var r in references)
                {
                    var hintPath = r.DirectMetadata.SingleOrDefault(a => a.Name.Equals("HintPath"))?.EvaluatedValue;
                    if (hintPath == null) continue;
                    if (!hintPath.Contains("..\\") || !allowedHintPaths.Any(a => hintPath.Contains(a)))
                    {
                        Debug.WriteLine($"{project.Name}: Illegal Hint Path: {hintPath}");
                    }
                }
                foreach (var pr in projectReferences)
                {
                    var include = pr.DirectMetadata.SingleOrDefault(a => a.Name.Equals("Project"));
                    if(include==null) continue;
                    var includeName = pr.DirectMetadata.SingleOrDefault(a => a.Name.Equals("Name"))?.EvaluatedValue;
                    var includeGuid = Guid.Parse(include.EvaluatedValue); //Projects.Where(w => w.RelativePath.Equals(include)).FirstOrDefault(f => f.Id);
                    if (project.Dependencies.All(a => a != includeGuid))
                    {
                        Debug.WriteLine($"{project.Name}: Missing Dependency: {includeName}");
                    }
                }
            }
        }

        public static void Test1()
        {
            var s = Solution.ReadSlnFile("C:\\Git-Web\\EULG.sln");
            s.CheckDependencies();
        }
    }

    public class MyProject
    {
        public Guid Id { get; set; }
        public Guid? ParentProject { get; set; }
        public string Name { get; set; }
        public SolutionProjectType Type { get; set; }
        public string AbsolutePath { get; set; }
        public string RelativePath { get; set; }
        public List<Guid> Dependencies { get; } = new List<Guid>();

        public Project Project { get; }

        public MyProject(Project project)
        {
            Project = project;
        }
    }
}
