var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var _deleteTarifSelector = '.ng-scope.md-font.mdi.mdi-24px.mdi-delete';
var _deleteTarifBtnSelector = '#modalDeleteAreYouSure_btnLöschen';
var _addTarifBtnSelector = '#btnNewTariffConfig';
var _versorgunswerkSelector = '#navViewLink_VnVnVersorgungswerk';
var _navchapter = '#navChapterLink_1';
var _ngoption = 'md-option[ng-repeat]';
var _value = 'value';
var _id = 'id';
var _beratungTarifSelector = '#navViewLink_BeratungBeratungTarifauswahl';
var _tarifTitle = 'Arbeitgeber – Tarifvorgabe'


class Tarif{

	get TarifTitle(){return _tarifTitle};

    ShowTarif(timeout=10000,pause=3000){
   		testLib.ClickAction(testLib.BtnNavNext,_beratungTarifSelector, timeout, pause)
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

	GetAllTarife()
	{
		var Selector = null;
		var List = null;
		var Values = null;
		Selector = '#'+testLib.TarifSelectoren[0]["Value"][0];
		List = $(Selector);
		Values = List.getAttribute(_ngoption, _value,true);
		Values = this.FilterOnline(Values);


	}

	FilterOnline(versichererIds)
	{
		var offline = [4];
		offline[0] = '1090'; // Swisslife
		offline[1] = '1062'; // LV
		offline[2] = '9995'; // Generali
		offline[3] = '1150'; // Saarland

		var online = [versichererIds.length-offline.length];
        versichererIds.forEach(function(value, index) 
        {
			var f = offline.find(value);
			if(true)
            {
                online[index] = element;
            }
        });

        return offline;
	}

	CreateTarif(versicherer, allArr)
	{
		var Selector = null;
		var List = null;
		var Values = null;
		var Ids = null;
	
		for (var tarifSel = 0; tarifSel < testLib.TarifSelectoren.length; tarifSel++)
		{
				Selector = '#'+testLib.TarifSelectoren[tarifSel]["Value"][0];
				if(testLib.TarifSelectoren[tarifSel]["CheckVisible"][0] == "true")
				{
					var CheckVisible = browser.isVisible(Selector); 
					if(!CheckVisible)
					{
						continue;
					}
				}

				
				List = $(Selector);
				Values = List.getAttribute(_ngoption, _value,true);
				Ids = List.getAttribute(_ngoption, _id,true);

				// var number = versicherer['Id'][0];

				// var index = Values[number];
				// if(index == null || index < 0)
				// {
				// 	testLib.ClickAction('#modalContainer_btnAbbrechen');
				// 	return;
				// }

				try
				{
					testLib.OnlyClickAction(Selector);

					if(tarifSel == 0)
					{
						testLib.ClickAction('#'+Ids[Values.indexOf(versicherer['Id'][0])]);

					}
					else
					{
						var checkIsEnabled =	browser.getAttribute(Selector, "disabled");
						if(Ids.length > 1 && checkIsEnabled == null)
						{
							testLib.ClickAction('#'+Ids[0]);
						}
					}						
				}catch(ex)
				{
					var x = "y";
				}
		
		}
		
		testLib.ClickAction('#modalContainer_btnSpeichern');
		

		// var index = -1;
		// if(browser.isExisting('.warningMessage'))
		// {
		// 	var text = browser.getText('.warningMessage');
		// 	index = text.indexOf('Tarifkonfiguration nicht verfügbar');
		
		
		// }
		// else
		// {
			
		// }
		// return true;// index == -1;


		
	}

	DeleteAllTarife(newTarif=false, jump=true)
	{
		if(jump)
		{
			this.Jump2TarifSite();
		}
		this.RemoveExistTariffs();
		if(newTarif)
		{
			this.AddTarif();
		}
	}
	
}


module.exports = Tarif;






