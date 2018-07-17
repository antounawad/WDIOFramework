var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var _deleteTarifSelector = '.ng-scope.md-font.mdi.mdi-24px.mdi-delete';
var _deleteTarifBtnSelector = '#modalDeleteAreYouSure_btnLöschen';
var _addTarifBtnSelector = '#btnNewTariffConfig';
var _versorgunswerkSelector = '#navViewLink_VnVnVersorgungswerk';
var _navchapter = '#navChapterLink_1';


class Tarif{
    ShowTarif(timeout=10000,pause=3000){
   		testLib.ClickAction('#btnNavNext','#navViewLink_BeratungBeratungTarifauswahl', timeout, pause)
	}


	RemoveExistTariffs()
	{
		testLib.WaitUntil(_addTarifBtnSelector);

		while(browser.isExisting(_deleteTarifSelector))
		{
			testLib.OnlyClickAction(_deleteTarifSelector);
			
			testLib.WaitUntil(_deleteTarifBtnSelector);
			
			testLib.ClickAction(_deleteTarifBtnSelector);

			testLib.WaitUntil(_addTarifBtnSelector);

			testLib.PauseAction(500);
			
		}		
	}
	
	Jump2TarifSite()
	{
		testLib.ClickAction(testLib.MenueMinMax);

		testLib.ClickAction(_navchapter,_versorgunswerkSelector);

		testLib.ClickAction(_versorgunswerkSelector);

	}

	AddTarif()
	{
		testLib.ClickAction(_addTarifBtnSelector);
	}
	
}


module.exports = Tarif;






