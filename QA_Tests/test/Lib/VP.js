var TestLib = require('../Lib/ClassLib.js')
 var assert = require('assert');
const testLib = new TestLib();

var _newVp = false;    


class VP{
    
	ShowVps(timeout)
	{
		testLib.ClickAction('#btnFastForwardConsultation', '#btnNewVp',timeout)
	}

	SearchVP(searchValue,timeout=2000,pause=500){
        //this.ShowVps(timeout)
		testLib.SearchElement('#Search',searchValue)
	}

	CheckVP(testVPName)
    {
        this.SearchVP(testVPName);
        testLib.PauseAction(1000);
		if(testLib.SmokeTest)
		{
            var text = browser.getText('#tableList');
            var index = text.indexOf(testVPName);
            if(index == -1)
            {
                testLib.OnlyClickAction('#btnNewVp');
                this.AddVP(testVPName);
            }
            else
            {
                //testLib.OnlyClickAction('#btnBlurredOverlay');
                testLib.OnlyClickAction('#tableList');
            }
		}		
    }

    AddVP(testVNName)
    {
       testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\Arbeitnehmer\\Stammdaten.xml');
	   this._newVp = true;
	   testLib.OnlyClickAction('#btnNavNext');
    }	
}
module.exports = VP;






