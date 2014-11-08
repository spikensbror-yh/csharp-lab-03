using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IssueApp.Core
{
    public static class IssueMetrics
    {
        #region Properties

        public static IEnumerable<Issue> Subjects { get; set; }

        #endregion

        #region Methods

        public static int Count()
        {
            return Subjects.Count();
        }

        public static int CountCompleted()
        {
            return (
                    from subject in Subjects
                    where subject.CompletedAt != null
                    select subject
                ).Count();
        }

        public static int CountActive()
        {
            return (
                    from subject in Subjects
                    where subject.CompletedAt == null
                    select subject
                ).Count();
        }

        public static int AverageCompletionTimeInHours()
        {
            if (CountCompleted() == 0)
                return 0;

            return (int)Math.Round((
                    from subject in Subjects
                    where subject.CompletedAt != null
                    select subject.CompletionTime().Value.TotalHours
                ).Average());
        }

        public static int LongestCompletionTimeInHours()
        {
            if (CountCompleted() == 0)
                return 0;

            return (int)Math.Round((
                    from subject in Subjects
                    where subject.CompletedAt != null
                    select subject.CompletionTime().Value.TotalHours
                ).Max());
        }

        #endregion
    }
}
