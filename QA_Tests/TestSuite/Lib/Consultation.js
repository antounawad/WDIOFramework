var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _btnNewConsultation = '#btnNewConsultation';
var _bruttoLohn = '#Bruttolohn';
var _btnFastForwardConsultation = '#btnFastForwardConsultation';
var _consultation = 'Consultation';
var _consultationDeleteSelector = '.md-font.mdi.mdi-24px.mdi-delete';
var _consultationConfirmDelete = '#modalDeleteConsultation_btnLöschen';
var _siteTitle = 'Beratungsübersicht';


class Consultation{

    get BtnFastForwardConsultation(){return _btnFastForwardConsultation};

    ShowConsultations(timeout=10000,pause=1000){
   		testLib.ClickElement(testLib.BtnFastForward,'', timeout, pause)
	}

    SetBruttoLohn(value,timeout=10000){
        testLib.SetValue(_bruttoLohn,value)	
    }

    AddConsultation(deleteConsultations=true,navigate=false)
    {
        if(navigate)
        {
            testLib.Navigate2Site(_siteTitle);
        }

        if(testLib.CheckIsVisible(_btnNewConsultation))
        {
            if(deleteConsultations)
            {
                this.RemoveExistConsultations();
            }
            testLib._AddChapter(_consultation, _btnNewConsultation, _bruttoLohn);
        }
    }


    RemoveExistConsultations() 
	{

		while (browser.isExisting(_consultationDeleteSelector)) {
			testLib.ClickElement(_consultationDeleteSelector);

			testLib.CheckIsVisible(_consultationConfirmDelete);

			testLib.ClickElement(_consultationConfirmDelete);

			testLib.PauseAction(500);

		}
	}
}
module.exports = Consultation;






