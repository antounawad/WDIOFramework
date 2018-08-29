var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _btnNewConsultation = '#btnNewConsultation';
var _bruttoLohn = '#Bruttolohn';
var _btnFastForwardConsultation = '#btnFastForwardConsultation';
var _consultation = 'Consultation';
var _consultationDeleteSelector = '.md-font.mdi.mdi-24px.mdi-delete';
var _consultationConfirmDelete = '#modalDeleteConsultation_btnLöschen';


class Consultation{

    get BtnFastForwardConsultation(){return _btnFastForwardConsultation};

    ShowConsultations(timeout=10000,pause=1000){
   		testLib.ClickElement(testLib.BtnFastForward,'', timeout, pause)
	}

    SetBruttoLohn(value,timeout=10000){
        testLib.SetValue(_bruttoLohn,value)	
    }

    AddConsultation(deleteConsultations=true)
    {
        if(testLib.IsVisible(_btnNewConsultation,5000))
        {
            if(deleteConsultations)
            {
                this.RemoveExistConsultations();
            }
            testLib.AddChapter(_consultation, _btnNewConsultation, _bruttoLohn);
        }
    }

    RemoveExistConsultations() {

		while (browser.isExisting(_consultationDeleteSelector)) {
			testLib.ClickElementSimple(_consultationDeleteSelector);

			testLib.IsVisible(_consultationConfirmDelete);

			testLib.ClickElement(_consultationConfirmDelete);

			testLib.PauseAction(500);

		}
	}

}
module.exports = Consultation;






