var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Tarif = require('../Lib/Tarif.js')
const tarif = new Tarif()

var _btnNewVn = '#btnNewVn';
var _gridSelector = '#tableList';
var _searchSelector = '#Search';
var _vnNode = 'VN';

class VN {

    ShowVNs(timeout = 20000) {
        testLib.ClickElement(_btnNewVn,'', timeout, 1000)
    }

    SearchVN(searchValue, timeout = 2000) {
        testLib.SaveScreenShot();
        testLib.SetValue(_searchSelector, searchValue)
        testLib.SaveScreenShot();
    }

    AddVN(testVNName, checkTarif = false, navnext=false) {
        this.SearchVN(testVNName);

        try{
            testLib._WaitUntilEnabled(testLib.BtnNavNext,2000);
        }catch(ex){}

        if (!testLib._CheckisEnabled(testLib.BtnNavNext,3000)) {
            this.AddChapter(testVNName);
        }
        else {
            if (checkTarif) {
                testLib.Navigate2Site(tarif.TarifTitle);
                if (!testLib._CheckisEnabled(testLib.BtnNavNext,3000)) {
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
        testLib._AddChapter(_vnNode, _btnNewVn, '', this.AddTarif);
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






