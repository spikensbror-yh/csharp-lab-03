using IssueApp.Core;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace IssueApp.Frontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Construct

        public MainWindow()
        {
            IssueManager.Initialize((string)Properties.Settings.Default["Database"]);

            InitializeComponent();
            IssueList.ItemsSource = IssueManager.Issues;
            PriorityBox.ItemsSource = Enum.GetNames(typeof(IssuePriority));
            SetupPriorityFilterBox();

            ClearIssueForm();
            Refresh();
        }

        #endregion

        #region Methods

        private bool PopulateIssue(Issue issue)
        {
            if (issue == null)
            {
                MessageBox.Show("Cannot populate a non-existant issue.");
                return false;
            }

            string title = TitleBox.Text;
            string description = DescriptionBox.Text;

            try
            {
                if (title == "")
                    throw new Exception("No title supplied!");

                if (description == "")
                    throw new Exception("No description supplied!");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            issue.Title = title;
            issue.Description = description;
            issue.Action = ActionBox.Text;
            issue.Priority = (IssuePriority)PriorityBox.SelectedIndex;

            DateTime reported, completed;
            if (DateTime.TryParse(ReportedAtBox.Text, out reported))
                issue.ReportedAt = reported;
            else if (ReportedAtBox.Text == "")
                issue.ReportedAt = null;

            if (DateTime.TryParse(CompletedAtBox.Text, out completed))
                issue.CompletedAt = completed;
            else if (CompletedAtBox.Text == "")
                issue.CompletedAt = null;

            return true;
        }

        private void PopulateIssueForm(Issue issue)
        {
            if (issue == null)
            {
                ClearIssueForm();
                return;
            }

            TitleBox.Text = issue.Title;
            DescriptionBox.Text = issue.Description;
            ActionBox.Text = issue.Action;
            PriorityBox.SelectedIndex = (int)issue.Priority;

            if (issue.ReportedAt.HasValue)
                ReportedAtBox.Text = issue.ReportedAt.Value.ToString();
            else
                ReportedAtBox.Text = null;

            if (issue.CompletedAt.HasValue)
                CompletedAtBox.Text = issue.CompletedAt.Value.ToString();
            else
                CompletedAtBox.Text = null;
        }

        private void ClearIssueForm()
        {
            TitleBox.Text = "";
            DescriptionBox.Text = "";
            ActionBox.Text = "";
            ReportedAtBox.Text = null;
            CompletedAtBox.Text = null;
            PriorityBox.SelectedIndex = 0;
        }

        private void Refresh()
        {
            IssueList.Items.Refresh();

            IssueMetrics.Subjects = IssueList.ItemsSource.Cast<Issue>();
            IssueTotalLabel.Content = IssueMetrics.Count();
            IssueCompletedLabel.Content = IssueMetrics.CountCompleted();
            IssueActiveLabel.Content = IssueMetrics.CountActive();
            IssueAverageLabel.Content = IssueMetrics.AverageCompletionTimeInHours() + " hour(s)";
            IssueLongestLabel.Content = IssueMetrics.LongestCompletionTimeInHours() + " hour(s)";
        }

        #endregion

        #region Window Events

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IssueManager.Deinitialize();
        }

        #endregion

        #region Button Events

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Issue issue = (Issue)IssueList.SelectedItem;
            if (PopulateIssue(issue))
            {
                IssueManager.Update(issue);
                Refresh();
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Issue issue = new Issue();
            if (PopulateIssue(issue))
            {
                issue.ReportedAt = issue.ReportedAt ?? DateTime.Now;

                IssueManager.Insert(issue);
                Refresh();
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "CSV File|*.csv";

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                IssueManager.ImportFromCSV(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Refresh();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "CSV File|*.csv";

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            IssueManager.ExportToCSV(dialog.FileName);
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            CompletedAtBox.Text = DateTime.Now.ToString();
        }

        private void DeselectButton_Click(object sender, RoutedEventArgs e)
        {
            IssueList.SelectedIndex = -1;
        }

        #endregion

        #region List Events

        private void IssueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateIssueForm((Issue)IssueList.SelectedItem);
        }

        #endregion

        #region Filter Events

        private void SetupPriorityFilterBox()
        {
            string[] enumItems = Enum.GetNames(typeof(IssuePriority));
            string[] filterItems = new string[1 + enumItems.Length];
            filterItems[0] = "Select a priority...";
            Array.Copy(enumItems, 0, filterItems, 1, enumItems.Length);

            PriorityFilterBox.ItemsSource = filterItems;
            PriorityFilterBox.SelectedIndex = 0;
        }

        private void PriorityFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = PriorityFilterBox.SelectedIndex - 1;
            if (selected < 0)
            {
                IssueList.ItemsSource = IssueManager.Issues;
                return;
            }

            IssuePriority priority = (IssuePriority)selected;
            IssueList.ItemsSource = from issue in IssueManager.Issues
                                    where issue.Priority == priority
                                    select issue;

            Refresh();
        }

        private void TitleFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            IssueList.ItemsSource = from Issue issue in IssueList.ItemsSource
                                    where issue.Title.ToLower().Contains(TitleFilterBox.Text.ToLower())
                                    select issue;

            Refresh();
        }

        #endregion

        #region Sorting Events

        private enum SortParameter
        {
            ReportedAt,
            Priority
        }

        private SortParameter sortParameter = SortParameter.ReportedAt;
        private bool sortAscending = false;

        private void ReportedAtSortButton_Click(object sender, RoutedEventArgs e)
        {
            if (sortParameter == SortParameter.ReportedAt)
                sortAscending = !sortAscending;
            else
                sortAscending = true;

            sortParameter = SortParameter.ReportedAt;
            if (sortAscending)
                IssueList.ItemsSource = from Issue issue in IssueList.ItemsSource
                                        orderby issue.ReportedAt ascending
                                        select issue;
            else
                IssueList.ItemsSource = from Issue issue in IssueList.ItemsSource
                                        orderby issue.ReportedAt descending
                                        select issue;
        }

        private void PrioritySortButton_Click(object sender, RoutedEventArgs e)
        {
            if (sortParameter == SortParameter.Priority)
                sortAscending = !sortAscending;
            else
                sortAscending = true;

            sortParameter = SortParameter.Priority;
            if (sortAscending)
                IssueList.ItemsSource = from Issue issue in IssueList.ItemsSource
                                        orderby issue.Priority ascending
                                        select issue;
            else
                IssueList.ItemsSource = from Issue issue in IssueList.ItemsSource
                                        orderby issue.Priority descending
                                        select issue;
        }

        #endregion
    }
}
