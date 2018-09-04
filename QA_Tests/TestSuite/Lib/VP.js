var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation();

var _btnNewVp = '#btnNewVp';
var _gridSelector = '#tableList';
var _vpNode = 'VP'
var _siteTitle = 'Arbeitnehmer – Auswahl';


class VP {

    ShowVps(timeout) {
        testLib.ClickElement(consultation.BtnFastForwardConsultation, _btnNewVp, timeout)
    }

    SearchVP(searchValue, timeout = 2000, pause = 500) {
        //this.ShowVps(timeout)
        testLib.SetValue('#Search', searchValue)
    }

    AddVP(testVPName, navnext=true) {
        testLib.Navigate2Site(_siteTitle);
        this.SearchVP(testVPName);
        testLib.PauseAction(1000);
        try{
            testLib._WaitUntilEnabled(testLib.BtnNavNext,2000);
        }catch(ex){}

        if (!testLib._CheckisEnabled(testLib.BtnNavNext,3000)) {
            this.AddChapter(testVPName);
        }
        else {
            if (testLib.CheckIsVisible(testLib.BtnBlurredOverlay)) {
                testLib.ClickElementSimple(testLib.BtnBlurredOverlay);
            }
            testLib.ClickElementSimple(_gridSelector);
			if(navnext)
            {
                testLib.Next();
            }
        }
    }

      AddChapter(testVNName,navnext=true) {
        testLib._AddChapter(_vpNode, _btnNewVp);
        if(navnext)
        {
            testLib.Next();
        }
    }
}
module.exports = VP;






