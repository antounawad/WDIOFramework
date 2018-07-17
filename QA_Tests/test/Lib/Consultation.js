var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _btnNewConsultation = '#btnNewConsultation';
var _bruttoLohn = '#Bruttolohn';
var _btnFastForwardConsultation = '#btnFastForwardConsultation';


class Consultation{

    get BtnFastForwardConsultation(){return _btnFastForwardConsultation};

    ShowConsultations(timeout=10000,pause=1000){
   		testLib.ClickAction(testLib.BtnFastForward,'', timeout, pause)
	}

    NewConsultation(timeout=50000,pause=1000){
        
        testLib.WaitUntil(_btnNewConsultation,10000);
        testLib.ClickAction(_btnNewConsultation);
        testLib.WaitUntil(_bruttoLohn);
	}

     SetBruttoLohn(value,timeout=10000){
        testLib.SearchElement(_bruttoLohn,value)	
    }

   
    AddConsultation(bruttolohn)
    {

        this.NewConsultation();
        testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\manual\\Beratung\\Beratung.xml');
        testLib.Navigate2Site('Eigenbeteiligung');
        testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\manual\\Beratung\\Eigenbeteiligung.xml');
        testLib.OnlyClickAction(testLib.BtnNavNext);
    }
    


}
module.exports = Consultation;






