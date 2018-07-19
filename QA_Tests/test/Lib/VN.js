var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Tarif = require('../Lib/Tarif.js')
const tarif = new Tarif()

var _btnMainAgency = '#btnXbavMainAgency';
var _btnNewVn = '#btnNewVn';
var _gridSelector = '#tableList';
var _searchSelector = '#Search';
var _vnNode = 'VN';

class VN{

    ShowVNs(timeout=20000)
    {
        testLib.ClickAction(_btnMainAgency, _btnNewVn,timeout,1000)
    }

    SearchVN(searchValue,timeout=200000){
        this.ShowVNs(timeout)
        testLib.SearchElement(_searchSelector,searchValue)
    }
    
    CheckVN(testVNName, checkTarif = false)
    {
        this.SearchVN(testVNName);
        testLib.PauseAction(1000);
		if(testLib.SmokeTest)
		{
            var text = browser.getText(_gridSelector);
            var index = text.indexOf(testVNName);
            if(index == -1)
            {
                this.AddVN(testVNName);
            }
            else
            {
                if(checkTarif)
                {
                    testLib.Navigate2Site(tarif.TarifTitle);
                    if(!testLib.CheckisEnabled(testLib.BtnNavNext))
                    {
                        this.AddTarif(testLib);
                    }

                }
                else
                {
                    testLib.OnlyClickAction(_gridSelector);
                }
            }
		}		
    }

    AddVN(testVNName)
    {
       testLib.AddChapter(_vnNode, _btnNewVn,'',this.AddTarif);
    }

    AddTarif(element)
    {
        tarif.AddTarif();
        tarif.CreateTarif(element['Versicherer'][0]);
    }
}
module.exports = VN;






