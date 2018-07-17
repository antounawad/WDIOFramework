var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _newVn = false;    
var _btnMainAgency = '#btnXbavMainAgency';
var _btnNewVn = '#btnNewVn';
var _gridSelector = '#tableList';
var _searchSelector = '#Search';

class VN{

    

    get NewVn(){ return this._newVn};

    ShowVNs(timeout=20000)
    {
        testLib.ClickAction(_btnMainAgency, _btnNewVn,timeout,1000)
    }

    SearchVN(searchValue,timeout=200000){
        this.ShowVNs(timeout)
        testLib.SearchElement(_searchSelector,searchValue)
    }
    
    CheckVN(testVNName)
    {
        this.SearchVN(testVNName);
        testLib.PauseAction(1000);
		if(testLib.SmokeTest)
		{
            var text = browser.getText(_gridSelector);
            var index = text.indexOf(testVNName);
            if(index == -1)
            {
                testLib.OnlyClickAction(_btnNewVn);
                this.AddVN(testVNName);
            }
            else
            {
                //testLib.OnlyClickAction('#btnBlurredOverlay');
                testLib.OnlyClickAction(_gridSelector);
            }
		}		
    }

    AddVN(testVNName)
    {
       testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\manual\\Arbeitgeber\\Stammdaten.xml');
       this._newVn = true;
    }

    AddZahlungsart()
    {
        testLib.Navigate2Site('Zahlungsart / GwG');
        testLib.CheckSiteFields(testLib.ExecutablePath+'test\\config\\sites\\manual\\Arbeitgeber\\Zahlungsart.xml');
        this._newVn = false;
    }



}
module.exports = VN;






