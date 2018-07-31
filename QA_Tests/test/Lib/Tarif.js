var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation();
var Document = require('../Lib/Document.js')
const document = new Document();

var _deleteTarifSelector = '.ng-scope.md-font.mdi.mdi-24px.mdi-delete';
var _deleteTarifBtnSelector = '#modalDeleteAreYouSure_btnLöschen';
var _addTarifBtnSelector = '#btnNewTariffConfig';
var _versorgunswerkSelector = '#navViewLink_VnVnVersorgungswerk';


var _ngoption = 'md-option[ng-repeat]';
var _value = 'value';
var _id = 'id';
var _beratungTarifSelector = '#navViewLink_BeratungBeratungTarifauswahl';
var _tarifTitle = 'Arbeitgeber – Tarifvorgabe'
var _It_Selector = null;
var _It_List = null;
var _It_Values = null;
var _It_Selector = null;
var _ResultArr = [100];
var _ResultCounter = 0;



class Tarif{

	get TarifTitle(){return _tarifTitle};
	get ResultArr(){return _ResultArr};
	get ResultCounter(){return _ResultCounter++};

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

	GetAllVersicherer()
	{
		_It_Selector = '#'+testLib.TarifSelectoren[0]["Value"][0];
		_It_List = $(_It_Selector);
		_It_Values = _It_List.getAttribute(_ngoption, _value,true);
		_It_Values = this.ExtractExcludeIds(_It_Values);

		return _It_Values;
	}

	GetAllDurchfWege()
	{
		_It_Selector = '#'+testLib.TarifSelectoren[1]["Value"][0];
		_It_List = $(_It_Selector);
		_It_Values = _It_List.getAttribute(_ngoption, _value,true);

		return _It_Values;
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
			var ind2 = _ResultArr.indexOf(value);
			if(ind == -1 && ind2 == -1)
            {
                online[counter++] = value;
            }
        });

        return online;
	}

	GetDurchfWegArray()
	{
		if(!testLib.AllDurchfWege)
		{
			var durchfwegArr = [testLib.DurchfWege.length];
			testLib.DurchfWege.forEach(function(value, index) 
			{
				durchfwegArr[index] = value['Id'][0];
			});			

			return durchfwegArr;
		}

		return this.GetAllDurchfWege();
	}
	
	
	

	CreateSmokeTarif(versicherer)
	{
		var Selector = null;
		var List = null;
		var Values = null;
		var Ids = null;
	
		for (var tarifSel = 0; tarifSel <= testLib.TarifSelectoren.length-1; tarifSel++)
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


	CreateListTarif(versicherer, newTarif=true)
	{
		var Selector = null;
		var List = null;
		var Values = null;
		var Ids = null;

		try
		{	
			var durchfSel = 0;
			while(true)
			{
				
				Selector = '#'+testLib.TarifSelectoren[0]["Value"][0];
				List = $(Selector);
				Values = List.getAttribute(_ngoption, _value,true);
				Ids = List.getAttribute(_ngoption, _id,true);

				testLib.OnlyClickAction(Selector);	
				var selector = '#'+Ids[Values.indexOf(versicherer)];
				testLib.OnlyClickAction(selector);
			

				Selector = '#'+testLib.TarifSelectoren[1]["Value"][0];
				var checkIsEnabled =	browser.getAttribute(Selector, "disabled");

				var durchfWegLength = 1;

				List = $(Selector);
				Values = List.getAttribute(_ngoption, _value,true);
				Ids = List.getAttribute(_ngoption, _id,true);

				var durchfWegeArr = this.GetDurchfWegArray();
				

				if(!testLib.AllDurchfWege)
				{
					var found = false;
					var x0 = durchfWegeArr[durchfSel];
					if(Values.indexOf(x0) == -1)
					{
						if(newTarif)
						{
							browser.refresh();
							testLib.WaitUntilVisible(_addTarifBtnSelector)
							this.AddTarif();
							durchfSel++;
							if(durchfSel > durchfWegeArr.length)
							{
								break;
							}
							continue;
								
						}
												
					}
					// for(var check = 0;check <= durchfWegeArr.length-1; check++)
					// {
					// 	found = Values.indexOf(durchfWegeArr[check]) >= 0;
					// 	if(found)
					// 	{
					// 		break;
					// 	}
					// }
					// if(!found)
					// {
					// 	testLib.ClickAction('#modalContainer_btnAbbrechen');
					// 	if(newTarif)
					// 	{
					// 		browser.refresh();
					// 		testLib.WaitUntilVisible(_addTarifBtnSelector)
					// 		this.AddTarif();
					// 	}
					// 	break;	
					// }
				}

				if(checkIsEnabled == null)
				{
					var durchfWegeArr = this.GetDurchfWegArray();
					durchfWegLength = durchfWegeArr.length;
				
					
					List = $(Selector);
					Values = List.getAttribute(_ngoption, _value,true);
					Ids = List.getAttribute(_ngoption, _id,true);

					testLib.OnlyClickAction(Selector);	
					var x0 = durchfWegeArr[durchfSel];
					var x1 = Values.indexOf(x0)
					var selector = '#'+Ids[x1];
					testLib.ClickAction(selector);
				}

					for (var tarifSel = 2; tarifSel <= testLib.TarifSelectoren.length-1; tarifSel++)
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
			
									var checkIsEnabled =	browser.getAttribute(Selector, "disabled");
									if(Ids.length > 1 && checkIsEnabled == null)
									{
										testLib.ClickAction('#'+Ids[0]);	
									}
														
							}catch(ex)
							{
								throw new Error(ex);
							}
					
					}
					testLib.ClickAction('#modalContainer_btnSpeichern');
					this.CheckAngebot(newTarif,true);
					durchfSel++;
					if(durchfSel > durchfWegLength-1)
					{
						break;
					}
			}
			

	}catch(ex)
	{
		throw new Error(ex);
	}
		
		
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

	CheckAngebot(newTarif=true,short=false) {
		if(short)
		{
			browser.refresh();
			testLib.WaitUntilVisible(_addTarifBtnSelector)
			this.DeleteAllTarife(newTarif);
			return;
		}		
		



		testLib.Navigate2Site('Beratungsübersicht');

	   consultation.AddConsultation();

		var failSite = testLib.StatusSiteTitle+':'+testLib.NavChapterAngebot+':'+testLib.LinkAngebotKurzUebersicht;
		testLib.Navigate2Site('Angebot – Kurzübersicht',failSite);

		this.CheckRKResult();

		document.GenerateDocuments();

		this.DeleteAllTarife(newTarif);
	}	


	CheckRKResult() {
		testLib.WaitUntilVisible(testLib.BtnNavNext, 100000);

		var errorBlock = $("md-card[ng-show='HasErrorMessages']");

		if (errorBlock !== undefined) {
			assert.notEqual(errorBlock.getAttribute('class').indexOf('ng-hide'), -1, 'Fehler bei Angebotserstellung für Tarif: ' + browser.getText("span[class='label-tarif']") + browser.getText("div[class='label-details']"));
		}
		else {
			assert.equal(0, 1, 'Rechenkernseite prüfen');
		}

	}	

	
	
	
}


module.exports = Tarif;






