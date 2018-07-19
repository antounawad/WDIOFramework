var TestLib = require('../Lib/ClassLib.js')
 var assert = require('assert');
const testLib = new TestLib();
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation();

var _btnNewVp = '#btnNewVp';
var _gridSelector = '#tableList';
var _vpNode = 'VP'
var _siteTitle = 'Arbeitnehmer – Auswahl';


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
        testLib.Navigate2Site(_siteTitle);
        this.SearchVP(testVPName);
        testLib.PauseAction(1000);
		if(testLib.SmokeTest)
		{
            var text = browser.getText(_gridSelector);
            var index = text.indexOf(testVPName);
            if(index == -1)
            {
                this.AddVP(testVPName);
            }
            else
            {
                if(testLib.CheckIsVisible(testLib.BtnBlurredOverlay))
                {
                    testLib.OnlyClickAction(testLib.BtnBlurredOverlay);
                }
                testLib.OnlyClickAction(_gridSelector);
            }
		}		
    }

    AddVP(testVNName)
    {
       testLib.AddChapter(_vpNode, _btnNewVp);
       testLib.Next();
    }	
}
module.exports = VP;






