var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

class Consultation{
    ShowConsultations(timeout=10000,pause=1000){
   		testLib.ClickAction('#btnFastForward','', timeout, pause)
	}

    NewConsultation(timeout=50000,pause=1000){
        testLib.ClickAction('#btnNewConsultation','#Bruttolohn', timeout,pause)
	}

     SetBruttoLohn(value,timeout=10000){
        testLib.SearchElement('#Bruttolohn',value,timeout)	
    }

   
    AddConsultation(bruttolohn)
    {

        this.NewConsultation();
        testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\Beratung\\Beratung.xml');
        testLib.Navigate2Selector('Eigenbeteiligung');
        testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\Beratung\\Eigenbeteiligung.xml');
        testLib.OnlyClickAction('#btnNavNext');
    }
    


}
module.exports = Consultation;






