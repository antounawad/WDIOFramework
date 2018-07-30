var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var _deleteTarifSelector = '.ng-scope.md-font.mdi.mdi-24px.mdi-delete';
var _deleteTarifBtnSelector = '#modalDeleteAreYouSure_btnLöschen';
var _addTarifBtnSelector = '#btnNewTariffConfig';
var _versorgunswerkSelector = '#navViewLink_VnVnVersorgungswerk';


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
		testLib.WaitUntilVisible(_addTarifBtnSelector);

		while(browser.isExisting(_deleteTarifSelector))
		{
			testLib.OnlyClickAction(_deleteTarifSelector);
			
			testLib.WaitUntilVisible(_deleteTarifBtnSelector);
			
			testLib.ClickAction(_deleteTarifBtnSelector);

			testLib.WaitUntilVisible(_addTarifBtnSelector);

			testLib.PauseAction(500);
			
		}		
	}
	
	Jump2TarifSite()
	{
		testLib.Jump2Chapter(testLib.NavChapterTarif, _versorgunswerkSelector);
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
		Values = this.ExtractExcludeIds(Values);

		return Values;
	}

	ExtractExcludeIds(versichererIds)
	{
		var online = [versichererIds.length-testLib.ExcludeVersicherer.length];
		var offline = [testLib.ExcludeVersicherer.length];
		testLib.ExcludeVersicherer.forEach(function(value, index) {
			offline[index] = value.Id[0];
		});
		var counter = 0;
        versichererIds.forEach(function(value, index) 
        {
			var ind = offline.indexOf(value);
			if(ind == -1)
            {
                online[counter++] = value;
            }
        });

        return online;
	}

	CreateTarif(versicherer)
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

				try
				{
					testLib.OnlyClickAction(Selector);

					if(tarifSel == 0)
					{
						var selector = '#'+Ids[Values.indexOf(versicherer)];
						testLib.ClickAction(selector);

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
					throw new Error(ex);
				}
		
		}
		
		testLib.ClickAction('#modalContainer_btnSpeichern');
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






