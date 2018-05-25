var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

class Consultation{
    ShowConsultations(timeout=10000,pause=3000){
   		testLib.ClickAction('#btnFastForwardConsultation','', timeout, pause)
	}

    NewConsultation(timeout=10000,pause=3000){
        this.ShowConsultations()
        testLib.ClickAction('#btnNewConsultation','#Bruttolohn', timeout,pause)
	}

     SetBruttoLohn(value,timeout=10000){
        testLib.SearchElement('#Bruttolohn',value,timeout)	
    }

}
module.exports = Consultation;






