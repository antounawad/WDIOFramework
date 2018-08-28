var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _btnNewConsultation = '#btnNewConsultation';
var _bruttoLohn = '#Bruttolohn';
var _btnFastForwardConsultation = '#btnFastForwardConsultation';
var _consultation = 'Consultation';


class Consultation{

    get BtnFastForwardConsultation(){return _btnFastForwardConsultation};

    ShowConsultations(timeout=10000,pause=1000){
   		testLib.ClickElement(testLib.BtnFastForward,'', timeout, pause)
	}

    SetBruttoLohn(value,timeout=10000){
        testLib.SetValue(_bruttoLohn,value)	
    }

    AddConsultation()
    {
        testLib.AddChapter(_consultation, _btnNewConsultation, _bruttoLohn);
    }
}
module.exports = Consultation;






