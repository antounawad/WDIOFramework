using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Controls;

namespace Eulg.Tools.QueryPad.Controls
{
    public partial class ResultsControl : UserControl
    {
        public EditorControl Editor { get; set; }

        public ResultsControl()
        {
            InitializeComponent();
        }

        public DataSet Data { get; private set; }
        public void SetData(DataSet dataSet)
        {
            Data = dataSet;
            GridViewOutput.ItemsSource = Data.Tables[0];
        }

    }
}
