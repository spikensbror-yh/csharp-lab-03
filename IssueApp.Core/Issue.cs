using System;

namespace IssueApp.Core
{
    public enum IssuePriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public class Issue
    {
        #region Properties

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public DateTime? ReportedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public IssuePriority Priority { get; set; }

        #endregion

        #region Methods

        public TimeSpan? CompletionTime()
        {
            return (CompletedAt != null) ? CompletedAt - ReportedAt : null;
        }

        #endregion
    }
}
