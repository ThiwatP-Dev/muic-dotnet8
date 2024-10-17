using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Schedules;

namespace KeystoneLibrary.Providers
{
    public class ScheduleGeneratorProvider
    {
        public List<SemesterCourseViewModel> CourseSections { get; set; }
        public List<SectionViewModel[]> GeneratedSchedules { get; set; }
        public List<string> ErrorList { get; set; }

        public ScheduleGeneratorProvider(List<SemesterCourseViewModel> courses)
        {
            GeneratedSchedules = new List<SectionViewModel[]>();
            this.CourseSections = courses;
            ErrorList = new List<string>();
        }

        public List<SectionViewModel[]> Generate()
        {
            DFS_Traverse(0, 0, new List<SectionViewModel>());
            return GeneratedSchedules;
        }

        private void DFS_Traverse(int i, int j, List<SectionViewModel> sectionViewModels)
        {
            int totalScheduleGenerated = 50;

            if (GeneratedSchedules.Count < totalScheduleGenerated)
            {
                if (i == CourseSections.Count)
                {
                    var section = new SectionViewModel[sectionViewModels.Count];

                    var index = 0;
                    
                    foreach (var item in sectionViewModels)
                    {
                        section[index++] = item;
                    }

                    GeneratedSchedules.Add(section);
                }
                else
                {
                    for (var index = 0; index < CourseSections[i].Sections.Count; ++index)
                    {
                        if (CourseSections[i].Sections[index].Number != "999")
                        {
                            if (sectionViewModels.Count == 0)
                            {
                                sectionViewModels.Add(CourseSections[i].Sections[index]);
                                DFS_Traverse(i + 1, index, sectionViewModels);
                                sectionViewModels.RemoveAt(sectionViewModels.Count - 1);
                            }
                            else
                            {
                                var hasValue = true;
                                for (var m = 0; m < sectionViewModels.Count; ++m)
                                {
                                    if (HasClassTimeConflict(sectionViewModels[m], CourseSections[i].Sections[index].ClassSchedules))
                                    {
                                        hasValue = false;
                                        break;
                                    }
                                }

                                if (hasValue)
                                {
                                    sectionViewModels.Add(CourseSections[i].Sections[index]);
                                    DFS_Traverse(i + 1, index, sectionViewModels);
                                    sectionViewModels.RemoveAt(sectionViewModels.Count - 1);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool HasClassTimeConflict(SectionViewModel current, IEnumerable<ClassScheduleTimeViewModel> next)
        {
            var conflictResult = current.CheckClassTimeConflict(next.ToList());
            if (conflictResult.Item1)
            {
                ErrorList.Add(conflictResult.Item2);
            }
            
            return conflictResult.Item1; //no comflict
        }
    }
}