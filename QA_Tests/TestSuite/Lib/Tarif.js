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
var _saveErrorCheck = '[class="swal2-confirm md-button md-raised md-accent"]';

var _ErrorList = [1000];
var _ErrorCounter = 0;

var _crlf = '\r\n';


let _sVersicherer = '';
let _sDurchfWeg = '';
let _sType = '';
let _sTarif = '';

class Tarif {

	get TarifTitle() { return _tarifTitle };
	get ResultArr() { return _ResultArr };
	get ResultCounter() { return _ResultCounter++ };
	get TarifCancelBtn() { return _TarifCancelBtn };

	ShowTarif(timeout = 10000, pause = 3000) {
		testLib.ClickElement(testLib.BtnNavNext, _beratungTarifSelector, timeout, pause)
	}

	CancelTarif() {
		if (testLib.CheckIsVisible(_TarifCancelBtn)) {
			testLib.ClickElementSimple(_TarifCancelBtn);
			testLib.PauseAction(1000);
		}
		testLib.RefreshBrowser(_addTarifBtnSelector);
	}


	RemoveExistTariffs() {
		testLib._WaitUntilVisible(_addTarifBtnSelector);

		while (browser.isExisting(_deleteTarifSelector)) {
			testLib.ClickElementSimple(_deleteTarifSelector);

			testLib._WaitUntilVisible(_deleteTarifBtnSelector);

			testLib.ClickElement(_deleteTarifBtnSelector);

			testLib._WaitUntilVisible(_addTarifBtnSelector);

			testLib.PauseAction(500);

		}
	}

	Jump2TarifSite() {
		testLib.Jump2Chapter(testLib.NavChapterTarif, _versorgunswerkSelector);
	}

	AddTarif() {
		testLib.ClickElement(_addTarifBtnSelector);
	}

	GetAllVersicherer() {
		try {
			_It_Selector = '#' + testLib.TarifSelectoren[0]["Value"][0];
			_It_List = $(_It_Selector);
			_It_Values = _It_List.getAttribute(_ngoption, _value, true);
			_It_Values = this.ExtractExcludeIds(_It_Values);

			return _It_Values;
		} catch (ex) {
			throw new Error(ex);
		}

	}

	GetAllDurchfWege() {
		try {
			_It_Selector = '#' + testLib.TarifSelectoren[1]["Value"][0];
			_It_List = $(_It_Selector);
			_It_Values = _It_List.getAttribute(_ngoption, _value, true);

			return _It_Values;
		} catch (ex) {
			throw new Error(ex);
		}
	}

	GetAllTarife() {
		try {
			_It_Selector = '#' + testLib.TarifSelectoren[3]["Value"][0];
			_It_List = $(_It_Selector);
			_It_Values = _It_List.getAttribute(_ngoption, _value, true);
			if (!testLib.AllTarife || testLib.SmokeTest || testLib.TarifSmoke) {
				if ((testLib.SmokeTest || testLib.TarifSmoke) && _It_Values.length > 0) {
					var values = [1];
					values[0] = _It_Values[0];
					_It_Values = values;
					return _It_Values;
				}
				_It_Values = this.ExtractExcludeTariffs(_It_Values);
			}
			return _It_Values;
		} catch (ex) {
			throw new Error(ex);
		}
	}

	GetAllTypes() {
		try {
			_It_Selector = '#' + testLib.TarifSelectoren[2]["Value"][0];
			_It_List = $(_It_Selector);
			_It_Values = _It_List.getAttribute(_ngoption, _value, true);
			if (!testLib.AllTypes) {
				_It_Values = this.ExtractExcludeTypes(_It_Values);
			}

			if (testLib.SmokeTest && _It_Values.length > 0)
				return _It_Values[0];

			return _It_Values;
		} catch (ex) {
			throw new Error(ex);
		}


	}

	ExtractExcludeTypes(types) {
		var simpletypes = [testLib.Types.length];
		var stypes = [testLib.Types.length];
		testLib.Types.forEach(function (value, index) {
			stypes[index] = value.Id[0];
		});

		if (types.length == 1) {
			var c = stypes.indexOf(types);
			if (c == -1) {
				return null;
			}
			else {
				return types;
			}
		}

		var counter = 0;
		types.forEach(function (value, index) {
			var ind = stypes.indexOf(value);
			if (ind >= 0) {
				simpletypes[counter++] = value;
			}
		});

		return simpletypes;
	}

	ExtractExcludeTariffs(tariffs) {
		var simpletariffs = [testLib.Tarife.length];
		var stariffs = [testLib.Tarife.length];
		testLib.Tarife.forEach(function (value, index) {
			stariffs[index] = value.Id[0];
		});

		if (tariffs.length == 1) {
			var c = stariffs.indexOf(tariffs);
			if (c == -1) {
				return null;
			}
			else {
				return tariffs;
			}
		}

		var counter = 0;
		tariffs.forEach(function (value, index) {
			var ind = stariffs.indexOf(value);
			if (ind >= 0) {
				simpletariffs[counter++] = value;
			}
		});

		return simpletariffs;
	}

	ExtractExcludeIds(versichererIds) {
		try {
			var online = [versichererIds.length - testLib.ExcludeVersicherer.length];
			var offline = [testLib.ExcludeVersicherer.length];
			testLib.ExcludeVersicherer.forEach(function (value, index) {
				offline[index] = value.Id[0];
			});
			var counter = 0;
			versichererIds.forEach(function (value, index) {
				var ind = offline.indexOf(value);
				var ind2 = _ResultArr.indexOf(value);
				if (ind == -1 && ind2 == -1) {
					online[counter++] = value;
				}
			});

			return online;
		} catch (ex) {
			throw new Error(ex);
		}
	}

	GetDurchfWegArray() {
		try {
			if (!testLib.AllDurchfWege || testLib.SmokeTest) {
				var durchfwegArr = [testLib.DurchfWege.length];
				testLib.DurchfWege.forEach(function (value, index) {
					durchfwegArr[index] = value['Id'][0];
				});

				if (testLib.SmokeTest && durchfwegArr.length > 0) {
					return durchfwegArr[0];
				}
				return durchfwegArr;
			}

			return this.GetAllDurchfWege();
		} catch (ex) {
			throw new Error(ex);
		}
	}



	Init() {
		_selector = null;
		_list = null;
		_values = null;
		_ids = null;
	}



	SetListBoxSelector() {
		_list = $(_selector);
		_values = _list.getAttribute(_ngoption, _value, true);
		_ids = _list.getAttribute(_ngoption, _id, true);
	}

	CheckSelectorIsDisabled() {
		if (_ids.length > 1 && browser.getAttribute(_selector, "disabled") == null) {
			testLib.ClickElement('#' + _ids[0]);
		}

	}

	CheckDfwIsEnabled(checkIsEnabled, dwFound, durchfWegeArr) {
		if (checkIsEnabled == null) {
			var durchfWegLength = durchfWegeArr.length;

			this.SetListBoxSelector();

			testLib.ClickElementSimple(_selector);

			var x1 = _values.indexOf(dwFound)
			var selector = '#' + _ids[x1];
			testLib.ClickElement(selector);
			_sDurchfWeg = browser.getText(selector);
			return durchfWegLength;
		}

		return 1;
	}

	CheckIsEnabled(checkIsEnabled, found, arr, logstring) {
		if (checkIsEnabled == null) {
			this.SetListBoxSelector();
			testLib.ClickElementSimple(_selector);
			var length = arr.length;
			var x1 = _values.indexOf(found)
			var selector = '#' + _ids[x1];
			testLib.ClickElement(selector);
			if (logstring == _sType)
				_sType = browser.getText(selector);
			else if (logstring == _sTarif)
				_sTarif = browser.getText(selector);
			return length;
		}

		return 1;
	}

	SelectAndClick() {
		try {
			this.SetListBoxSelector();
			testLib.ClickElementSimple(_selector);
			this.CheckSelectorIsDisabled();
			return true;

		} catch (ex) {
			return false;
		}
	}


	CreateListTarif(versicherer, newTarif = true, callback = false) {
		this.Init();

		var durchfSelCnt = 0;
		var tarifSelCnt = 0;
		var typeSelCnt = 0;
		var pre_durchfSelCnt = 0;
		var pre_tarifSelCnt = 0;
		var pre_typeSelCnt = 0;

		while (true) {

			_sVersicherer = '';
			_sType = '';
			_sDurchfWeg = '';
			_sTarif = '';


			try {


				_selector = '#' + testLib.TarifSelectoren[0]["Value"][0];

				this.SetListBoxSelector();

				testLib.ClickElementSimple(_selector);
				var selector = '#' + _ids[_values.indexOf(versicherer)];
				testLib.ClickElementSimple(selector);
				_sVersicherer = browser.getText(selector);

				console.log(_sVersicherer);



				_selector = '#' + testLib.TarifSelectoren[1]["Value"][0];
				var checkIsEnabled = browser.getAttribute(_selector, "disabled");
				_sDurchfWeg = browser.getText(_selector);

				var durchfWegLength = 1;

				this.SetListBoxSelector();

				var durchfWegeArr = null;

				durchfWegeArr = this.GetDurchfWegArray();

				var dwFound = "";
				if (!testLib.AllDurchfWege) {
					dwFound = durchfWegeArr[durchfSelCnt];
					if (_values.indexOf(dwFound) == -1) {
						if (newTarif) {
							testLib.RefreshBrowser(_addTarifBtnSelector, newTarif);
						}
						pre_durchfSelCnt = durchfSelCnt;
						durchfSelCnt++;
						if (durchfSelCnt > durchfWegeArr.length - 1 || checkIsEnabled === 'true') {
							console.log('Nothing to do at choosed combination...')
							break;
						}
						continue;
					}
				}

				dwFound = durchfWegeArr[durchfSelCnt];

				durchfWegLength = this.CheckDfwIsEnabled(checkIsEnabled, dwFound, durchfWegeArr);


				console.log(_sDurchfWeg);



				// type
				var typeArr = null;
				typeArr = this.GetAllTypes();

				var checkTypeIsDisabled = null;
				var typeLength = 0;
				if (typeArr != null) {
					typeLength = typeArr.length;
				}
				else {
					testLib.ClickElementSimple(_TarifCancelBtn, 500);

					pre_typeSelCnt = 0;
					typeSelCnt = 0;
					pre_tarifSelCnt = 0;
					tarifSelCnt = 0;
					pre_durchfSelCnt = durchfSelCnt;
					durchfSelCnt++;
					testLib.RefreshBrowser(_addTarifBtnSelector, newTarif);
					if (durchfSelCnt > durchfWegLength - 1) {
						console.log('Nothing to do at choosed combination...')
						break;
					}

					continue;

				}




				_selector = '#' + testLib.TarifSelectoren[2]["Value"][0];
				_sType = browser.getText(_selector);
				checkTypeIsDisabled = browser.getAttribute(_selector, "disabled");

				if (testLib.TarifSelectoren[2]["CheckVisible"][0] == "true") {
					var CheckVisible = browser.isVisible(_selector);
					if (!CheckVisible) {
						continue;
					}
				}

				var typeFound = typeArr[typeSelCnt];



				typeLength = this.CheckIsEnabled(checkTypeIsDisabled, typeFound, typeArr, _sType);



				console.log(_sType);
				// end Type

				// begin Tarif

				var tarifArr = null;
				tarifArr = this.GetAllTarife();

				var checkTarifIsDisabled = null;
				var tarifLength = tarifArr.length;

				_selector = '#' + testLib.TarifSelectoren[3]["Value"][0];
				_sTarif = browser.getText(_selector);
				checkTarifIsDisabled = browser.getAttribute(_selector, "disabled");

				var tarifFound = tarifArr[tarifSelCnt];

				tarifLength = this.CheckIsEnabled(checkTarifIsDisabled, tarifFound, tarifArr, _sTarif);

				console.log(_sTarif);


				// end Tarif
				for (var tarifSel = 4; tarifSel <= testLib.TarifSelectoren.length - 1; tarifSel++) {
					_selector = '#' + testLib.TarifSelectoren[tarifSel]["Value"][0];
					if (testLib.TarifSelectoren[tarifSel]["CheckVisible"][0] == "true") {
						var CheckVisible = browser.isVisible(_selector);
						if (!CheckVisible) {
							continue;
						}
					}

					this.SelectAndClick();
					console.log(browser.getText(_selector));
				}
				pre_tarifSelCnt = tarifSelCnt;
				tarifSelCnt++;




				testLib.ClickElement(_tarifSaveBtn);

				if (callback)
					break;


				if (testLib.CheckIsVisible(_saveErrorCheck)) {
					pre_tarifSelCnt = tarifSelCnt;
					tarifSelCnt--;
					throw new Error("Tarif Save Error...")
				}



				if (tarifSelCnt > tarifLength - 1 || checkTarifIsDisabled === 'true' || testLib.TarifSmoke) {
					pre_tarifSelCnt = 0;
					tarifSelCnt = 0;
					pre_typeSelCnt = typeSelCnt;
					typeSelCnt++;

					if (typeSelCnt > typeLength - 1 || checkTypeIsDisabled === 'true' || testLib.TypeSmoke) {
						pre_typeSelCnt = 0;
						typeSelCnt = 0;
						pre_tarifSelCnt = 0;
						tarifSelCnt = 0;
						pre_durchfSelCnt = durchfSelCnt;
						durchfSelCnt++;
					}

				}







				if (!newTarif) {
					newTarif = true;
					if (durchfSelCnt >= durchfWegLength && (tarifSelCnt == 0 || typeSelCnt == 0)) {
						newTarif = false;

					}
				}
				//console.log("counter: " + _counter++);
				//testLib.LogTime("Vor RK Test");



				this.CheckAngebot(newTarif, testLib.OnlyTarifCheck);
				console.log(_crlf);

				if (testLib.SmokeTest)
					break;

				//testLib.LogTime("Nach RK Test");



				if (durchfSelCnt > durchfWegLength - 1) {
					break;
				}



			} catch (ex) {

				var message = 'Versicherer: ' + _sVersicherer + _crlf + 'Durchf.Weg: ' + _sDurchfWeg + _crlf + 'Typ: ' + _sType + _crlf + 'Tarif: ' + _sTarif + _crlf + ex.message;

				var rkError = (ex.message.indexOf('Fehler bei Angebotserstellung') >= 0 || ex.message.indexOf('Fehler bei der Dokumentegenerierung') >= 0);

				if (rkError) {
					this.ErrorFunction(message);
					this.DeleteAllTarife(true, newTarif);
					if (testLib.SmokeTest)
						break;

					continue;

				}

				_ErrorCounter++;

				if (testLib.IsDebug) {
					console.log('InnerLoop Error: ' + ex.message);

				}

				if (_ErrorCounter > 10) {
					throw new Error('Zu viele Fehler Iterationen')
				}


				testLib.ClickElementSimple(_TarifCancelBtn, 500);
				testLib.RefreshBrowser(_addTarifBtnSelector, newTarif);
				if (testLib.SmokeTest)
					break;
				continue;
			}


		}

	}

	ErrorFunction(message) {
		let dt = testLib.LogTime();
		_ErrorList[_ErrorCounter] = message + ' Bild: ' + String(_ErrorCounter + dt + '.png');
		testLib._TakeErrorShot(String(_ErrorCounter) + dt + '.png');
		_ErrorCounter++;
		console.log(message);

		if (testLib.BreakAtError) {
			console.log("BreakAtError = false; Fehler:")
			assert.equal(1, 0, message);
		}
	}

	DeleteAllTarife(newTarif = false, jump = true) {
		if (jump) {
			this.Jump2TarifSite();
		}
		this.RemoveExistTariffs();
		if (newTarif) {
			this.AddTarif();
		}
	}

	CheckAngebot(newTarif = true, short = false) {
		if (short) {
			testLib.RefreshBrowser(_addTarifBtnSelector);
			this.DeleteAllTarife(newTarif);
			return;
		}

		if (testLib.IsDebug) {
			console.log('newTarif: ' + String(newTarif));
			console.log('short: ' + String(short));
		}


		if (testLib.IsDebug) {
			console.log('Beratungsübersicht pre')
		}

		testLib.Navigate2Site('Beratungsübersicht');

		if (testLib.IsDebug) {
			console.log('Beratungsübersicht post')
		}

		consultation.AddConsultation();

		if (testLib.IsDebug) {
			console.log('AddConsultation post')
		}

		var failSite = testLib.StatusSiteTitle + ':' + testLib.NavChapterAngebot + ':' + testLib.LinkAngebotKurzUebersicht;
		testLib.Navigate2Site('Angebot – Kurzübersicht', failSite);

		if (testLib.IsDebug) {
			console.log('Angebot - Kurzübersicht post')
		}

		this.CheckRKResult();

		if (testLib.IsDebug) {
			console.log('CheckRKResult post')
		}

		document.GenerateDocuments();

		if (testLib.IsDebug) {
			console.log('GenerateDocuments post')
		}

		this.DeleteAllTarife(newTarif);

		if (testLib.IsDebug) {
			console.log('DeleteAllTarife post')
		}
	}


	CheckRKResult() {
		testLib._WaitUntilVisible(testLib.BtnNavNext, 100000);

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






