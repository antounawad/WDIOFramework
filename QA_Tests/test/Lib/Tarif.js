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
var _counter = 0;



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
		try{
			_It_Selector = '#'+testLib.TarifSelectoren[0]["Value"][0];
			_It_List = $(_It_Selector);
			_It_Values = _It_List.getAttribute(_ngoption, _value,true);
			_It_Values = this.ExtractExcludeIds(_It_Values);

			return _It_Values;
		}catch(ex)
		{
			throw new Error(ex);
		}

	}

	GetAllDurchfWege()
	{
		try
		{
		_It_Selector = '#'+testLib.TarifSelectoren[1]["Value"][0];
		_It_List = $(_It_Selector);
		_It_Values = _It_List.getAttribute(_ngoption, _value,true);

		return _It_Values;
	}catch(ex)
	{
		throw new Error(ex);
	}		
	}

	GetAllTarife()
	{
		try
		{
		_It_Selector = '#'+testLib.TarifSelectoren[3]["Value"][0];
		_It_List = $(_It_Selector);
		_It_Values = _It_List.getAttribute(_ngoption, _value,true);
		return _It_Values;
	}catch(ex)
	{
		throw new Error(ex);
	}		
	}

	GetAllTypes()
	{
		try{
			_It_Selector = '#'+testLib.TarifSelectoren[2]["Value"][0];
			_It_List = $(_It_Selector);
			_It_Values = _It_List.getAttribute(_ngoption, _value,true);
			if(!testLib.AllTypes)
			{
				_It_Values = this.ExtractExcludeTypes(_It_Values);
			}
			return _It_Values;
		}catch(ex)
		{
			throw new Error(ex);
		}


	}	

	ExtractExcludeTypes(types)
	{
		var simpletypes = [testLib.Types.length];
		var stypes = [testLib.Types.length];
		testLib.Types.forEach(function(value, index) {
			stypes[index] = value.Id[0];
		});

		if(types.length == 1)
		{
		  var c = stypes.indexOf(types);
		  if(c == -1)
		  {
			  return null;
		  }
		}

		var counter = 0;
        types.forEach(function(value, index) 
        {
			var ind = stypes.indexOf(value);
			if(ind >= 0)
            {
                simpletypes[counter++] = value;
            }
        });

        return simpletypes;
	}	

	ExtractExcludeIds(versichererIds)
	{
		try
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
	}catch(ex)
	{
		throw new Error(ex);
	}		
	}

	GetDurchfWegArray()
	{
		try
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
	}catch(ex)
	{
		throw new Error(ex);
	}
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

	CheckIsEnabled(checkIsEnabled,found,arr)
	{
		if(checkIsEnabled == null)
		{
			this.SetListBoxSelector();
			testLib.OnlyClickAction(_selector);	
			var length = arr.length;
			var x1 = _values.indexOf(found)
			var selector = '#'+_ids[x1];
			testLib.ClickAction(selector);
			return length;
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
			return true;

		}catch(ex)
		{
			return false;
		}		
	}


	CreateListTarif(versicherer, newTarif=true)
	{
		this.Init();

		try
		{	
			var durchfSelCnt = 0;
			var tarifSelCnt = 0;
			var typeSelCnt = 0;
			var loopBreaker = false;

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

				var durchfWegeArr = null;

				try
				{
					durchfWegeArr = this.GetDurchfWegArray();
				}
				catch(ex)
				{
					testLib.OnlyClickAction(_TarifCancelBtn,500);
					testLib.RefreshBrowser(_addTarifBtnSelector,true);
					continue;
				}
				
				var dwFound = "";
				if(!testLib.AllDurchfWege)
				{
					dwFound = durchfWegeArr[durchfSelCnt];
					if(_values.indexOf(dwFound) == -1)
					{
						if(newTarif)
						{
							testLib.RefreshBrowser(_addTarifBtnSelector,newTarif);
						}
						durchfSelCnt++;
						if(durchfSelCnt > durchfWegeArr.length-1 || checkIsEnabled === 'true')
						{
							break;
						}
						continue;
					}
				}

				dwFound = durchfWegeArr[durchfSelCnt];
				
				durchfWegLength = this.CheckDfwIsEnabled(checkIsEnabled,dwFound,durchfWegeArr);

				console.log("Versicherer: "+String(versicherer)+" Durchführungsweg: "+String(dwFound)+"...");



				// type
				var typeArr = null;
				try
				{
					typeArr = this.GetAllTypes();
				}
				catch(ex)
				{
					testLib.OnlyClickAction(_TarifCancelBtn,500);
					testLib.RefreshBrowser(_addTarifBtnSelector, true);
					continue;
				}
				
				var checkTypeIsDisabled = null;
				var typeLength = 0;
				if(typeArr != null)
				{
					typeLength = typeArr.length;
				}
				else
				{
					testLib.OnlyClickAction(_TarifCancelBtn,500);
					
					typeSelCnt = 0;
					tarifSelCnt = 0;
					durchfSelCnt++;
					testLib.RefreshBrowser(_addTarifBtnSelector,newTarif);
					if(durchfSelCnt > durchfWegLength-1)
					{
						break;
					}
					
					continue;

				}


				
				while(true)
				{

					_selector = '#'+testLib.TarifSelectoren[2]["Value"][0];
					checkTypeIsDisabled =	browser.getAttribute(_selector, "disabled");
	
					if(testLib.TarifSelectoren[2]["CheckVisible"][0] == "true")
					{
						var CheckVisible = browser.isVisible(_selector); 
						if(!CheckVisible)
						{
							continue;
						}
					}

					var typeFound = typeArr[typeSelCnt];


					try
					{
						typeLength = this.CheckIsEnabled(checkTypeIsDisabled,typeFound,typeArr);
					}
					catch(ex)
					{
						testLib.RefreshBrowser(_addTarifBtnSelector,true);
						loopBreaker = true;
						break;
					}

					

					console.log("Versicher: "+String(versicherer)+" Durchführungsweg: "+String(dwFound)+" Type: "+String(typeFound)+"...");
				// end Type

				// begin Tarif

				var tarifArr = null;
				try
				{
					tarifArr = this.GetAllTarife();
				}
				catch(ex)
				{
					testLib.RefreshBrowser(_addTarifBtnSelector,true);
					loopBreaker = true;
					break;
				}
				
				var checkTarifIsDisabled = null;
				var tarifLength = tarifArr.length;
				if(testLib.TarifSmoke)
				{
					tarifLength = 1;
				}
				
				while(true)
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

					var tarifFound = tarifArr[tarifSelCnt];

					try
					{
	
						tarifLength = this.CheckIsEnabled(checkTarifIsDisabled,tarifFound,tarifArr);
					}
					catch(ex)
					{
						testLib.OnlyClickAction(_TarifCancelBtn,500);
						testLib.RefreshBrowser(_addTarifBtnSelector,true);
						loopBreaker = true;
					}

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
			
						if(!this.SelectAndClick())
						{
							loopBreaker = true;
							testLib.RefreshBrowser(_addTarifBtnSelector,true);
							break;
						}
					}
					if(!loopBreaker)
					{
						tarifSelCnt++;
					}
					break;
				
				}
				break;
			}

			if(loopBreaker == true)
			{
				loopBreaker = false;
				continue;
			}

			testLib.ClickAction(_tarifSaveBtn);

			if(tarifSelCnt > tarifLength -1 || checkTarifIsDisabled === 'true' || testLib.TarifSmoke)
			{
				tarifSelCnt = 0;
				typeSelCnt ++;

				if(typeSelCnt > typeLength -1 || checkTypeIsDisabled === 'true' || testLib.TypeSmoke)
				{
					typeSelCnt = 0;
					tarifSelCnt = 0;
					durchfSelCnt++;
				}
					
			}




				


				if(!newTarif)
				{
					newTarif = true;
					if(durchfSelCnt >= durchfWegLength && (tarifSelCnt == 0 || typeSelCnt == 0))
					{
						newTarif = false;
						
					}
				}
				console.log("counter: "+_counter++);
				testLib.LogTime("Vor RK Test");
				this.CheckAngebot(newTarif,testLib.OnlyTarifCheck);
				testLib.LogTime("Nach RK Test");


				if(durchfSelCnt > durchfWegLength-1)
				{
					break;
				}					



				
				
			
			}
			var x = "y";
			

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






