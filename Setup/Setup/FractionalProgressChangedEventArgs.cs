using System;

namespace Eulg.Setup
{
    public class FractionalProgressChangedEventArgs : System.EventArgs
    {
        public double Progress { get; private set; }

        public bool ItemCountsUpdated { get { return CurrentItem != null; } }

        public long CompletedItems { get; private set; }

        public long TotalItems { get; private set; }

        public string CurrentItem { get; private set; }

        public FractionalProgressChangedEventArgs(long complete, long total)
        {
            if(total < 0 || complete < 0 || complete > total)
            {
                throw new ArgumentOutOfRangeException();
            }

            Progress = (double)complete / total;
        }

        public FractionalProgressChangedEventArgs(long complete, long total, long completeItems, long totalItems, string currentItem)
        {
            if (total < 0 || complete < 0 || complete > total || totalItems < 0 || completeItems < 0 || completeItems > totalItems)
            {
                throw new ArgumentOutOfRangeException();
            }

            Progress = (double)complete / total;
            CompletedItems = completeItems;
            TotalItems = totalItems;
            CurrentItem = currentItem;
        }
    }
}
