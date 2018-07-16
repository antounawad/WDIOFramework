var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _newVn = false;    

class VN{

    

    get NewVn(){ return this._newVn};

    ShowVNs(timeout=20000)
    {
        testLib.ClickAction('#btnXbavMainAgency', '#btnNewVn',timeout,1000)
    }

    SearchVN(searchValue,timeout=200000){
        this.ShowVNs(timeout)
        testLib.SearchElement('#Search',searchValue)
    }
    
    CheckVN(testVNName)
    {
        this.SearchVN(testVNName);
        testLib.PauseAction(1000);
		if(testLib.SmokeTest)
		{
            var text = browser.getText('#tableList');
            var index = text.indexOf(testVNName);
            if(index == -1)
            {
                testLib.OnlyClickAction('#btnNewVn');
                this.AddVN(testVNName);
            }
            else
            {
                //testLib.OnlyClickAction('#btnBlurredOverlay');
                testLib.OnlyClickAction('#tableList');
            }
		}		
    }

    AddVN(testVNName)
    {
       testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\Arbeitgeber\\Stammdaten.xml');
       this._newVn = true;
    }

    AddZahlungsart()
    {
        testLib.Navigate2Selector('Zahlungsart / GwG');
        testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\Arbeitgeber\\Zahlungsart.xml');
    }



}
module.exports = VN;






