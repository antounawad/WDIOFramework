namespace Eulg.Setup
{
    public class FractionalProgressChangedEventArgs : System.EventArgs
    {
        public double Progress { get; }

        public bool ItemCountsUpdated => CurrentItem != null;

        public long CompletedItems { get; }

        public long TotalItems { get; }

        public string CurrentItem { get; }

        public FractionalProgressChangedEventArgs(long complete, long total)
        {
            if (total < 0) total = 0;
            if (complete < 0) complete = 0;
            if (complete > total) complete = total;

            Progress = (double)complete / total;
        }

        public FractionalProgressChangedEventArgs(long complete, long total, long completeItems, long totalItems, string currentItem)
        {
            if (total < 0) total = 0;
            if (complete < 0) complete = 0;
            if (complete > total) complete = total;
            if (totalItems < 0) totalItems = 0;
            if (completeItems < 0) completeItems = 0;
            if (completeItems > totalItems) completeItems = totalItems;

            Progress = (double)complete / total;
            CompletedItems = completeItems;
            TotalItems = totalItems;
            CurrentItem = currentItem;
        }
    }
}
