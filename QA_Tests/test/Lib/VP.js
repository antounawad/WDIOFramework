var TestLib = require('../Lib/ClassLib.js')
 var assert = require('assert');
const testLib = new TestLib();
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation();

var _newVp = false;    
var _btnNewVp = '#btnNewVp';
var _gridSelector = '#tableList';


class VP{
    
	ShowVps(timeout)
	{
		testLib.ClickAction(consultation.BtnFastForwardConsultation, _btnNewVp ,timeout)
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
            var text = browser.getText(_gridSelector);
            var index = text.indexOf(testVPName);
            if(index == -1)
            {
                testLib.OnlyClickAction(_btnNewVp);
                this.AddVP(testVPName);
            }
            else
            {
                //testLib.OnlyClickAction('#btnBlurredOverlay');
                testLib.OnlyClickAction(_gridSelector);
            }
		}		
    }

    AddVP(testVNName)
    {
       testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\manual\\Arbeitnehmer\\Stammdaten.xml');
       this._newVp = true;
       
	   testLib.OnlyClickAction('#btnNavNext');
    }	
}
module.exports = VP;






