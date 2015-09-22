using System;

namespace Eulg.Setup.Pages
{
    public interface ISetupPageBase
    {
        string PageTitle { get; set; }

        Type PrevPage { get; set; }

        bool HasNext { get; set; }
        bool HasPrev { get; set; }

        string NextButtonText { get; set; }

        void OnLoad();
        void OnLoadComplete();
        void OnNext();
        bool OnPrev();
        bool OnClose();

    }
}
