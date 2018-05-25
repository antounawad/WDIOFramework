using System.ComponentModel;
using Eulg.Utilities.Console;
using xbAV.Utilities.Kkzb.Filters;

namespace Tools.KkzbGrabber.Filters
{
    [Description("Match provider names against database")]
    class InteractiveDatabaseFilter : DatabaseFilter
    {
        public InteractiveDatabaseFilter() : base(Prompt.SqlServerConnectionString("(LocalDb)\\v11.0", "eulg"))
        {
        }

        protected override bool OnRelaxedNameEquality(string name1, string name2)
        {
            return Prompt.YesNo($"The names '{name1}' and '{name2}' seem similar. Do they refer to the same insurance provider?");
        }
    }
}
