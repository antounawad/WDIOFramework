var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Tarif = require('../Lib/Tarif.js')
const tarif = new Tarif()

var _btnMainAgency = '#btnXbavMainAgency';
var _btnNewVn = '#btnNewVn';
var _gridSelector = '#tableList';
var _searchSelector = '#Search';
var _vnNode = 'VN';
var _checkIisEnabled = false;

class VN {

    ShowVNs(timeout = 20000) {
        testLib.ClickElement(_btnNewVn,'', timeout, 1000)
    }

    SearchVN(searchValue, timeout = 200000) {
        testLib.SetValue(_searchSelector, searchValue)
    }

    AddVN(testVNName, checkTarif = false, navnext=false) {
        this.SearchVN(testVNName);
        testLib.PauseAction(1000);


        if (!testLib.IsEnabled(testLib.BtnNavNext,2000)) {
            this.AddChapter(testVNName,navnext);
        }
        else {
            if (checkTarif) {
                testLib.Navigate2Site(tarif.TarifTitle);
                if (!testLib.IsEnabled(testLib.BtnNavNext)) {
                    this.AddTarif(testLib);
                }

            }
            else {
                testLib.ClickElementSimple(_gridSelector);
                if(navnext)
                {
                    testLib.Next();
                }
            }
        }

    }

    AddChapter(testVNName,navnext) {
        testLib.AddChapter(_vnNode, _btnNewVn, '', this.AddTarif);
        if(navnext)
        {
            testLib.Next();
        }
    }

    AddTarif(element) {
        tarif.AddTarif();
        tarif.CreateListTarif(element['Versicherer'][0]['Id'][0], false,true);

    }
}
module.exports = VN;






