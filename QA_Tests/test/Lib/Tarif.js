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
var _TarifCancelBtn = '#modalContainer_btnAbbrechen';
var _tarifSaveBtn = '#modalContainer_btnSpeichern';


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

var _selector = null;
var _list = null;
var _values = null;
var _ids = null;



class Tarif{

	get TarifTitle(){return _tarifTitle};
	get ResultArr(){return _ResultArr};
	get ResultCounter(){return _ResultCounter++};
	get TarifCancelBtn(){return _TarifCancelBtn};

    ShowTarif(timeout=10000,pause=3000){
   		testLib.ClickAction(testLib.BtnNavNext,_beratungTarifSelector, timeout, pause)
	}

	CancelTarif()
	{
		if(testLib.CheckIsVisible(_TarifCancelBtn))
		{
			testLib.OnlyClickAction(_TarifCancelBtn);
			testLib.PauseAction(1000);
		}
		testLib.RefreshBrowser(_addTarifBtnSelector);
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

	GetAllTarife()
	{
		_It_Selector = '#'+testLib.TarifSelectoren[3]["Value"][0];
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

	GetTarifArray()
	{
		if(!testLib.AllTarife)
		{
			var tarifArr = [testLib.Tarife.length];
			testLib.Tarife.forEach(function(value, index) 
			{
				tarifArr[index] = value['Id'][0];
			});			

			return tarifArr;
		}

		return this.GetAllTarife();
	}	

	Init()
	{
		_selector = null;
		_list = null;
		_values = null;
		_ids = null;
	}
	
	
	

	CreateSmokeTarif(versicherer)
	{
		this.Init();
		
		for (var tarifSel = 0; tarifSel <= testLib.TarifSelectoren.length-1; tarifSel++)
		{
				_selector = '#'+testLib.TarifSelectoren[tarifSel]["Value"][0];
				if(testLib.TarifSelectoren[tarifSel]["CheckVisible"][0] == "true")
				{
					var CheckVisible = browser.isVisible(_selector); 
					if(!CheckVisible)
					{
						continue;
					}
				}

				this.SetListBoxSelector();

				try
				{
					testLib.OnlyClickAction(_selector);

					if(tarifSel == 0)
					{
						var selector = '#'+_ids[_values.indexOf(versicherer)];
						testLib.ClickAction(selector);

					}
					else
					{
						this.CheckSelectorIsDisabled();
					}						
				}catch(ex)
				{
					console.log("Error: CreateSmokeTarif: "+ex.message);
					throw new Error(ex);
				}
		
		}
		
		testLib.ClickAction(_tarifSaveBtn);
	}

	SetListBoxSelector()
	{
		_list = $(_selector);
		_values = _list.getAttribute(_ngoption, _value,true);
		_ids = _list.getAttribute(_ngoption, _id,true);
	}

	CheckSelectorIsDisabled()
	{
		if(_ids.length > 1 && browser.getAttribute(_selector, "disabled") == null)
		{
			testLib.ClickAction('#'+_ids[0]);	
		}

	}

	CheckDfwIsEnabled(checkIsEnabled,dwFound,durchfWegeArr)
	{
		if(checkIsEnabled == null)
		{
			var durchfWegLength = durchfWegeArr.length;

			this.SetListBoxSelector();

			testLib.OnlyClickAction(_selector);	
			
			var x1 = _values.indexOf(dwFound)
			var selector = '#'+_ids[x1];
			testLib.ClickAction(selector);
			return durchfWegLength;
		}

		return 1;
	}

	CheckTarifIsEnabled(checkIsEnabled,tarifFound,tarifArr)
	{
		if(checkIsEnabled == null)
		{
			this.SetListBoxSelector();
			testLib.OnlyClickAction(_selector);	
			var tarifLength = tarifArr.length;
			var x1 = _values.indexOf(tarifFound)
			var selector = '#'+_ids[x1];
			testLib.ClickAction(selector);
			return tarifLength;
		}

		return 1;
	}

	SelectAndClick()
	{
		
			
		try
		{
			this.SetListBoxSelector();
			testLib.OnlyClickAction(_selector);
			this.CheckSelectorIsDisabled();

		}catch(ex)
		{
			console.log("Error: SelectAndClick: "+ex.message);
			throw new Error(ex);
		}		
	}


	CreateListTarif(versicherer, newTarif=true)
	{
		this.Init();

		try
		{	
			var durchfSel = 0;
			var tarifSell = 0;
			while(true)
			{
				
				_selector = '#'+testLib.TarifSelectoren[0]["Value"][0];

				this.SetListBoxSelector();

				testLib.OnlyClickAction(_selector);	
				var selector = '#'+_ids[_values.indexOf(versicherer)];
				testLib.OnlyClickAction(selector);
			

				_selector = '#'+testLib.TarifSelectoren[1]["Value"][0];
				var checkIsEnabled =	browser.getAttribute(_selector, "disabled");

				var durchfWegLength = 1;

				this.SetListBoxSelector();

				var durchfWegeArr = this.GetDurchfWegArray();
				var dwFound = "";
				if(!testLib.AllDurchfWege)
				{
					dwFound = durchfWegeArr[durchfSel];
					if(_values.indexOf(dwFound) == -1)
					{
						if(newTarif)
						{
							testLib.RefreshBrowser(_addTarifBtnSelector);
							this.AddTarif();
						}
						durchfSel++;
						if(durchfSel > durchfWegeArr.length-1 || checkIsEnabled === 'true')
						{
							break;
						}
						continue;
					}
				}

				dwFound = durchfWegeArr[durchfSel];
				
				durchfWegLength = this.CheckDfwIsEnabled(checkIsEnabled,dwFound,durchfWegeArr);

				console.log("Versicherer: "+String(versicherer)+" Durchführungsweg: "+String(dwFound)+"...");


				// type

				_selector = '#'+testLib.TarifSelectoren[2]["Value"][0];
				if(testLib.TarifSelectoren[2]["CheckVisible"][0] == "true")
				{
					var CheckVisible = browser.isVisible(_selector); 
					if(!CheckVisible)
					{
						continue;
					}
				}

				this.SelectAndClick();
				// end Type

				// begin Tarif

				var tarifArr = this.GetAllTarife();
				var checkTarifIsDisabled = null;
				var tarifLength = tarifArr.length;
				if(testLib.TarifSmoke)
				{
					tarifLength = 1;
				}
				for(var t = tarifSell;t <= tarifLength -1;t++)
				{

					_selector = '#'+testLib.TarifSelectoren[3]["Value"][0];
					checkTarifIsDisabled =	browser.getAttribute(_selector, "disabled");
	
					if(testLib.TarifSelectoren[3]["CheckVisible"][0] == "true")
					{
						var CheckVisible = browser.isVisible(_selector); 
						if(!CheckVisible)
						{
							continue;
						}
					}

					var tarifFound = tarifArr[tarifSell];
	
					var tarifLength = this.CheckTarifIsEnabled(checkTarifIsDisabled,tarifFound,tarifArr);

					console.log("Versicher: "+String(versicherer)+" Durchführungsweg: "+String(dwFound)+" Tarif: "+String(tarifFound)+"...");
	
					// end Tarif
					for (var tarifSel = 4; tarifSel <= testLib.TarifSelectoren.length-1; tarifSel++)
					{
						_selector = '#'+testLib.TarifSelectoren[tarifSel]["Value"][0];
						if(testLib.TarifSelectoren[tarifSel]["CheckVisible"][0] == "true")
						{
							var CheckVisible = browser.isVisible(_selector); 
							if(!CheckVisible)
							{
								continue;
							}
						}
			
						this.SelectAndClick();
					}
					tarifSell++;
					break;
				
				}

				testLib.ClickAction(_tarifSaveBtn);
				if(tarifSell > tarifLength -1 || checkTarifIsDisabled === 'true' || testLib.TarifSmoke)
				{
					tarifSell = 0;
					durchfSel++;
				}


				if(!newTarif)
				{
					newTarif = true;
					if(durchfSel >= durchfWegLength-1 && tarifSell == 0)
					{
						newTarif = false;
						
					}
				}
				this.CheckAngebot(newTarif,testLib.OnlyTarifCheck);
				


				if(durchfSel > durchfWegLength-1)
				{
					break;
				}					



				
				
			
			}
			

	}catch(ex)
	{
		console.log("Error: CreateListTarif: "+ex.message);
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
			testLib.RefreshBrowser(_addTarifBtnSelector);
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






