var assert = require('assert');
var fs = require('fs'),
    xml2js = require('xml2js')
var date = require('date-and-time');
var path = require('path');

var _UrlTimeOut = 999999999999999999999999999999999999999999;

// Mit Dokumentgenerierung oder nicht
var _Documents = false;
var _debug = false;

// Versicher Liste (falls in config angegeben)
var _Versicherer = null;
var _DurchfWege = null;
var _Tarife = null;
var _ExcludeVersicherer = null;
// Speichert die TarifSelektoren
var _TarifSelector = null;
// Alle Versicherer oder nur bestimmte
var _AllVersicherer = false;

var _VnName = null;
var _VpName = null;
// Smoke Test Ja oder Nein
var _SmokeTest = false;
// Liefert die Felder in Site Config
var _SiteFields = null;
// Config Path
var _executablePath = "C:\\Git\\Shared\\QA_Tests\\";
// Helper um Fehler bei den Iterationen abzufangen
var _Navigate2SiteIterator = 0;
// Helper um Fehler bei Rekursiven Aufrufen zu vermeiden
var _ClickIterator = 0;
var _SearchIterator = 0;
var _ClearElementIterator = 0;

var _WaitUntilSelector = "";

var _TarifSmoke = false;

var _BreakAtError = false;

var _TarifSiteSelector = 'Arbeitgeber – Tarifvorgabe';

var _MenueMinMax = '.fold-toggle.hide.show-gt-sm.md-font.mdi.mdi-24px.mdi-backburger';

var _btnTarifSave = 'modalContainer_btnSpeichern';

var _AllDurchfWege = false;
var _AllTarife = false;

var _AllType = false;
var _Types = null;
var _TypeSmoke = null;

var _btnNavNext = '#btnNavNext';
var _btnNavPrev = '#btnNavBack';

var _leftSiteMenu = '#navbar-left'

var _btnFastForward = '#btnFastForward';

var _NewChapterList = ['New', 'Chapter', '', 'Sites'];

var _btnBlurredOverlay = '#btnBlurredOverlay';

var _gridSelector = '#tableList';

var _btnMainAgency = '#btnXbavMainAgency';

var _btnNewVn = '#btnNewVn';

var _NavchapterTarif = '#navChapterLink_1'; // Arbeitgeber
var _NavchapterAngebot = '#navChapterLink_6' // Angebot
var _NavchapterDokumente = '#navChapterLink_8' // Dokumente
var _StatusSiteTitle = 'Abschluss - Status';
var _Leistungsphase = 'Leistungsphase - Rentenleistung aus bAV';
var _LinkAngebotKurzUebersicht = '#navViewLink_AngebotAngebotVersichererangebot';
var _VnAuswahl = 'Arbeitgeber – Auswahl';

var _OnlyTarifCheck = false;

var _CurrentCheckID = 0;
var _ExMessage = [3];
var _ExMessageCnt = 0;
var _TestFolder = null;
var _TestConfigFolder = null;
var __xpath = null;
var __xpathResult = null;
var _takeScreenShotAllDialogs = false;
var _SplitVersicherer = false;
var _SplitFrom = 0;
var _SplitTo = 0;
var _LoginName = null;
var _LoginPassword = null;
var _WorkflowSelector = null;

var _SelectorIndexArr = [100]
var _SelectorArr = [100]


class TestLib {

    get LoginName() { return _LoginName };
    get LoginPassword() { return _LoginPassword };
    get WorkflowSelector() { return _WorkflowSelector };

    set TakeScreenShotAllDialogs(value) {

        _takeScreenShotAllDialogs = value

        if (value === true) {
            this._DeleteAllFilesFromDirectory(this.ErrorShotPath);
        }

    };
    get VnName() { return _VnName };
    get VpName() { return _VpName };
    get SplitVersicherer() { return _SplitVersicherer };
    get SplitFrom() { return _SplitFrom };
    get SplitTo() { return _SplitTo };
    set SplitTo(value) { _SplitTo = value };

    get IsDebug() { return _debug === 'true' }
    get TypeSmoke() { return _TypeSmoke === 'true' };
    get Types() { return _Types };
    get AllTypes() { return _AllType === 'true' };

    set CurrentID(value) { _CurrentCheckID = value };


    get TarifSmoke() { return _TarifSmoke === 'true' };
    get OnlyTarifCheck() { return _OnlyTarifCheck === 'true' };
    get Tarife() { return _Tarife };

    get AllTarife() { return _AllTarife === 'true' };

    get NavChapterTarif() { return _NavchapterTarif };
    get NavChapterAngebot() { return _NavchapterAngebot };
    get NavChapterDokumente() { return _NavchapterDokumente };

    get StatusSiteTitle() { return _StatusSiteTitle }

    get LeistungsphaseTitle() { return _Leistungsphase };

    get LinkAngebotKurzUebersicht() { return _LinkAngebotKurzUebersicht };

    get UrlTimeOut() { return _UrlTimeOut };

    get BtnMainAgency() { return _btnMainAgency };

    get BreakAtError() { return _BreakAtError === 'true' };

    get AllDurchfWege() { return _AllDurchfWege === 'true' };

    get DurchfWege() { return _DurchfWege };

    get BtnBlurredOverlay() { return _btnBlurredOverlay };
    //Wegen Config Dateien.
    get ExecutablePath() { return _executablePath };

    get ErrorShotPath() { return this.ExecutablePath + 'TestSuite\\' + this.TargetUrl + '\\' + _TestFolder + _TestConfigFolder + 'errorShots\\' };

    // Gibt die VersichererList aus Config Datei zurück
    get Versicherer() { return _Versicherer };

    get ExcludeVersicherer() { return _ExcludeVersicherer };

    // FileStream für Config Dateien
    get Fs() { return fs };

    get ArgVOffset() { return 2 };

    get TestComponent() {
        return this._GetValuefromArg("prd");
    };

    get TestConfigFolder() {
        return this._GetValuefromArg("tcf");
    };

    get ConfigLib() {
        return this._GetValuefromArg("cfg");
    }

    // Übergebenes Projekt --hotfix aus Args
    get TargetUrl() {

        return this._GetValuefromArg("url");
    }

    get TestFolder() {
        return _TestFolder;
    }


    get TargetDom() {
        return this._GetValuefromArg("dom");
    }

    get VersionCompare() {
        return this._GetValuefromArg("ver");
    }

    // Returns Version aus Args
    get Version() {
        let ver = process.argv[5]
        if (ver != null) {
            return ver.substr(2);
        }
        return '';
    }

    // Returns Main Config Pfad
    get MainConfigPath() {
        return this.ExecutablePath + 'TestSuite\\' + this.ConfigLib + '\\' + this.TestComponent + '\\' + this.TestConfigFolder + '\\' + 'Config.xml'
    }

    get CommonConfigPath() {
        return this.ExecutablePath + 'TestSuite\\common\\Config.xml';
    }

    // Einheitliche Rückgabe des Titels
    get BrowserTitle() { return browser.getTitle() }

    // Returns Versicher Liste aus Config falls angegeben
    get Versicherer() { return _Versicherer }

    // Returns Schlter ob alle Versicherer geprüft werden oder nur die aus der List
    // Wandelt um on Boolean
    get AllVersicherer() { return _AllVersicherer === 'true' }

    // Returns Smoke Test ja oder nein
    // Wandelt um in Boolean
    get SmokeTest() { return _SmokeTest === 'true' }

    // Returns TarifSelektoren aus Config
    get TarifSelectoren() { return _TarifSelector }

    // Mit Document Test oder nicht
    get DocumentTest() { return _Documents === 'true' }

    // Loggt den Browser Title und prüft, falls assertString nicht leer ist
    ShowBrowserTitle(assertString = '') {
        console.log("Broser Title: " + this.BrowserTitle)
        if (assertString != '') {
            assert.equal(this.BrowserTitle, assertString);
        }

    }

    set WaitUntilSelector(value) {
        _WaitUntilSelector = value;
    }

    get WaitUntilSelector() {
        return _WaitUntilSelector;
    }

    get MenueMinMax() { return _MenueMinMax };

    get BtnNavNext() { return _btnNavNext };
    get BtnNavPrev() { return _btnNavPrev };

    get BtnFastForward() { return _btnFastForward };


    get TarifSiteSelector() { return _TarifSiteSelector };


    CompareTitle(title) {
        return browser.getTitle().includes(title);
    }
// listbox mit Angular 
    SetListBoxValue(selector, value) {
        var fieldname = this._GetFieldName(selector);

        if (fieldname.includes('[')) {
            var ex = this._GetSelector(fieldname);
            fieldname = '#' + ex.getAttribute('id');
        }

        var exist = this.WaitUntilExist(fieldname);

        if (!exist) {
            throw new Error("Warning: Selector not found...");
        }

        var List = this._GetSelector(fieldname);
        var values = List.getAttribute("md-option[ng-repeat]", "value", true);
        var Ids = List.getAttribute("md-option[ng-repeat]", "id", true);

        var index = values.indexOf(value);

        if (Ids.length > 1) {
            this.ClickElementSimple(fieldname, 1000);
            if (index > -1) {
                this.ClickElement('#' + Ids[index]);
            }
            else {
                this.ClickElement('#' + Ids[0]);
            }
        }
    }

    GetText(selector) {

        var selObj = this._GetSelector(selector);
        if (selObj !== null && selObj.isDisplayed()) {

            return selObj.getText();
        }
        else {
            if (this.IsDebug)
                console.log("Warning: Selector: " + selector + " not found.")
            return "";
        }

    }

    GetValue(selector) {
        var selObj = this._GetSelector(selector);
        if (selObj !== null && selObj.isDisplayed()) {
            var check = selObj.getValue()
            if (check != null) {
                return check;
            }
        }
        else {
            if (this.IsDebug)
                console.log("Warning: Selector " + selector + " not found.")
            return "";
        }

    }

    GetAttributeValue(selector, attribute) {
        var selObj = this._GetSelector(selector);
        if (selObj !== null && selObj.isDisplayed()) {

            var value = selObj.getAttribute(attribute);
            return value;
        }
        else {
            if (this.IsDebug)
                console.log("Warning: Selector " + selector + " not found.")
            return "";
        }

    }


    CompareValue(selector, value) {
        var selObj = this._GetSelector(selector);
        if (selObj !== null && selObj.isDisplayed()) {

            var check = selObj.getValue()
            if (check != null) {
                return check === value;
            }
            else {
                var text = searchSelector.getText();

                return (text.includes(value));
            }
        }
        else {
            if (this.IsDebug)
                console.log("Warning Selector " + selector + " not found.")
            return false;
        }

    }

    // Sucht ein Element (Selector) und ruft die Methode zum Setzen eines Values auf
    // Wird der Selector nicht gefunden, wird abgebrochen
    // Wenn ein Pause value übergeben wird, wird Pausiert
    SetValue(selector, value, pauseTime = 0, checkExist = false) {
        try {
            if (_SearchIterator >= 20) {
                _SearchIterator = 0;
                throw new Error("Zu viele SearchElement Iterationen");
            }
            var searchSelector = this._GetSelector(selector);
            assert.notEqual(searchSelector, null)

            var entryValue = searchSelector.getValue();


            if (checkExist) {
                if (entryValue != null && entryValue != "") {
                    return;
                }
            }

            searchSelector.setValue(value);
            var retValue = searchSelector.getValue();

            if (value != retValue) {
                this.ClickElementSimple(searchSelector.selector);
                this.PauseAction(500);
                searchSelector.addValue(100);

                _SearchIterator += 1;
                this.SetValue(selector, value, 1000);
            }
            this.PauseAction(pauseTime);
        } catch (ex) {
            console.log("Warning: SearchElement: " + ex.message);
            _SearchIterator += 1;
            this.SetValue(selector, value, 1000);

        }

    }



    CheckisEnabled(selector, waitTime = 300) {

        try {

            var selObj = this._GetSelector(selector)
            selObj.waitForEnabled(waitTime);
            if (selObj !== null) {

                this._WaitUntilEnabled(selector, waitTime);
            }

        } catch (ex) {
        } finally {
            return selObj.isDisplayed();
        }
    }



    CheckIsVisible(selector, waitTime = 300) {
        try {
            var retValue = this._GetSelector(selector);
            retValue.waitForDisplayed(waitTime);
        } catch (ex) {
        } finally {
            return retValue.isDisplayed();
        }
    }

    CheckIsVisibleGetSelector(selector, waitTime = 300, click = false) {
        try {
            var retValue = this._GetSelector(selector);
            retValue.waitForDisplayed(waitTime);
            if (click) {
                retValue.click();
            }
        } catch (ex) {
        } finally {
            return retValue;
        }
    }

    CheckExist(waitUntilSelector, waitTime = 1000, message = "") {
        _WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: ' + waitUntilSelector + ' to be different after: ' + waitTime;
        if (message != "") {
            _message = message;
        }

        var res = true;
        var funcSel = this._GetSelector;
        try {
            res = browser.waitUntil(function () {
                var selObj = funcSel(waitUntilSelector);
                if (selObj !== null) {
                    return selObj.isExisting();
                }
                else {
                    return false;
                }
            }, waitTime, _message);
        } catch (ex) {
            res = false;
        }
        finally {
            return res;
        }



    }


    // Navigiert zur Seite des Übergebenen Seitentitels
    Navigate2Site(title, failSite = '') {
        try {

            if (_Navigate2SiteIterator >= 20) {
                _Navigate2SiteIterator = 0;
                throw new Error("Warning: Zu viele Navigate2Site Iterationen");
            }
            while (true) {

                try {

                    this._WaitUntilTitle();
                    if (this.IsDebug) {
                        console.log(this.BrowserTitle);
                    }
                }
                catch (ex) {
                    console.log("Warning: Navigate2Site(WaitUntilTitle): " + ex.message);
                }



                if (String(this.BrowserTitle).includes(title)) {
                    _Navigate2SiteIterator = 0;
                    this.PauseAction(500);
                    break;
                }

                if (failSite != '') {
                    var fSiteArr = String(failSite).split(":");
                    var indexFail = this.BrowserTitle.indexOf(fSiteArr[0]);
                    if (indexFail >= 0) {
                        this.Jump2Chapter(fSiteArr[1], fSiteArr[2]);
                        this.Navigate2Site(title, failSite);
                    }
                }

                this._WaitUntilVisible(this.BtnNavNext);
                this.Next();

                this._CheckSiteFields();
            }
        } catch (ex) {
            _Navigate2SiteIterator += 1;
            var conslog = !ex.message.includes('is not clickable at point') && ex.message.includes('obscures it');
            if (conslog) {
                console.log("Warining: Navigate2Site: " + ex.message);
            }
            this.Navigate2Site(title, failSite);
        } finally {
            this._CheckSiteFields();
        }
    }


    Navigate2SitePrev(title, failSite = '') {
        try {

            if (_Navigate2SiteIterator >= 20) {
                _Navigate2SiteIterator = 0;
                throw new Error("Zu viele Navigate2Site Iterationen");
            }
            while (true) {

                try {

                    this._WaitUntilTitle();
                    if (this.IsDebug) {
                        console.log(this.BrowserTitle);
                    }
                }
                catch (ex) {
                    console.log("Error: Navigate2Site(WaitUntilTitle): " + ex.message);
                }



                if (String(this.BrowserTitle).includes(title)) {
                    _Navigate2SiteIterator = 0;
                    this.PauseAction(500);
                    break;
                }

                if (failSite != '') {
                    var fSiteArr = String(failSite).split(":");
                    var indexFail = this.BrowserTitle.indexOf(fSiteArr[0]);
                    if (indexFail >= 0) {
                        this.Jump2Chapter(fSiteArr[1], fSiteArr[2]);
                        this.Navigate2SitePrev(title, failSite);
                    }
                }

                this._WaitUntilVisible(this.BtnNavPrev);
                this.Prev();

            }
        } catch (ex) {
            _Navigate2SiteIterator += 1;
            var conslog = !ex.message.includes('is not clickable at point') && ex.message.includes('obscures it');
            if (conslog) {
                console.log("Error: Navigate2SitePrev: " + ex.message);
            }
            this.Navigate2SitePrev(title, failSite);
        }
    }

    Jump2FailSite(failSite, title) {
        if (failSite === '') {
            this.Navigate2Site(title);
            return;
        }
        var chapterLink = String(failSite).split(":");
        this.Jump2Chapter(chapterLink[0], chapterLink[1])
    }

    Jump2Chapter(chapter, link) {
        this._SetLeftMenuVisible();
        if (!this.CheckIsVisible(link)) {
            this.ClickElement(chapter, link);
        }

        this.ClickElement(link);
    }




    ClearElementValue(elementName) {
        try {
            if (_ClearElementIterator >= 20) {
                _ClearElementIterator = 0;
                throw new Error("Warning: Zu viele Iterationen ClearElement");
            }
            var element = this._GetSelector(elementName);

            if (_ClearElementIterator > 0) {
                this.PauseAction(1000);
            }

            element.clearElement();

            if (element.getValue() != "") {
                _ClearElementIterator += 1;
                this.ClearElementValue(elementName);
            }

        }
        catch (ex) {
            console.log("Warning: ClearElementValue: " + ex.message);
            _ClearElementIterator += 1;

        }

    }

    SelectHauptAgentur() {
        this._WaitUntilVisibleWithClick(_btnMainAgency, true);
        this._WaitUntilVisible(_btnNewVn, 50000);
        this.SaveScreenShot();

    }


    Next(waitTime = 0) {
        this.PauseAction(waitTime);
        this.ClickElement(_btnNavNext);
    }

    Prev(waitTime = 0) {
        this.PauseAction(waitTime);
        this.ClickElement(_btnNavPrev);
    }

    ClickElementSimple(selector, pauseTime = 0) {
        try {

            var selObj = this._GetSelector(selector);
            if (selObj === null || !selObj.isExisting()) {
                return;
            }
            assert.notEqual(selObj.selector, "");

            selObj.click();
            this._CheckAndClearSelectorArr(selector);

            if (pauseTime > 0) {
                this.PauseAction(pauseTime);
            }
            return selObj;
        } catch (ex) {
            console.log("Warning: ClickElementSimple: " + ex.message);
            if (!this._CheckPopUp(retValue.selector)) {
                throw new Error(ex);
            }
        }
    }

    SaveScreenShot() {
        if (_takeScreenShotAllDialogs === true) {
            this._WaitUntilTitle();
            this._TakeErrorShot(this.BrowserTitle);
        }
    }

    _CheckAndClearSelectorArr(selector) {
        if (selector.includes("next") || selector.includes("prev") ) {
            this._InitSelectorIndex();
        }
    }

    ClickElement(selector, waitforVisibleSelector = '', timeout = 50000, pauseTime = 0, click = false) {

        if (_ClickIterator >= 20) {
            _ClickIterator = 0;
            throw new Error("Warning: Zu viele ClickAction Iterationen");
        }

        var retValue = this._GetSelector(selector);

        this._WaitUntilVisible(retValue.selector)
        this._WaitUntilEnabled(retValue.selector)


        try {
            retValue.click();
            this._CheckAndClearSelectorArr(selector);

            if (retValue.selector == _btnNavPrev || selector == _btnNavNext) {
                this.SaveScreenShot();
            }

        } catch (ex) {
            var conslog = !ex.message.includes('is not clickable at point') && ex.message.includes('obscures it');
            if (conslog) {
                console.log("Warning: ClickAction: " + ex.message);
            }
            _ClickIterator += 1;

            if (!this._CheckPopUp(retValue.selector)) {
                throw new Error(ex);
            }
            else {
                return;
            }

            if (this.CheckIsVisible(_btnTarifSave)) {
                this._GetSelector(_btnTarifSave).click();
            }
            else if (this.CheckIsVisible(_btnNavPrev)) {
                this._GetSelector(_btnNavPrev).click();
                this.SaveScreenShot();
            }

            this.ClickElement(selector, waitforVisibleSelector, timeout, pauseTime, click);
        }

        if (waitforVisibleSelector == '#btnFastForward') {
            if (!this._GetSelector(waitforVisibleSelector).isDisplayed()) {
                waitforVisibleSelector = _btnNavNext;
            }
        }


        if (waitforVisibleSelector != '') {
            var selObj = this._GetSelector(waitforVisibleSelector)
            if (selObj !== null) {
                selObj.waitForDisplayed(timeout)
            }
        }

        if (click) {
            this.ClickElement(waitforVisibleSelector);
        }

        this.PauseAction(pauseTime);

        return retValue;

    }

    PauseAction(pauseTime) {
        if (pauseTime > 0) {
            browser.pause(pauseTime);
        }
    }





    // CheckVersion() {
    //     if (this.Version !== '') {
    //         var t = browser.getText('#container-main');
    //         console.log(t);
    //         assert.notEqual(t.indexOf('Version ' + this.Version), -1, "Fehlerhafte Version ausgliefert.");
    //     }
    // }


    CheckVersion() {
        if (this.VersionCompare !== '') {
            var currentVersion = $('#container-main').getText();
            var propertyVersion = this.VersionCompare;
            return currentVersion.includes("Version " + propertyVersion);
        }
    }

    CheckGivenVersion(versionToCheck = "") {
        if (versionToCheck != "") {
            var currentVersion = $('#container-main').getText();
            return currentVersion.includes("Version " + versionToCheck);


        }
        else {
            console.log("No Version was given to Chaeck")
        }
    }

    CheckText(selector, text) {
        var index = -1;
        if (this.SmokeTest && this.Version != '') {
            var elem = this._GetSelector(selector);
            var text = elem.getText();
            if (text != null && text.length > 0)
                index = text.indexOf(text);

        }
        return index >= 0;
    }




    _ReadXMLFieldValues(xmlFile) {

        _SiteFields = null;
        this._GetXmlParser(xmlFile).parseString(this.Fs.readFileSync(xmlFile), function (err, result) {
            _SiteFields = result['Config']['Fields'][0]['Field'];
        })

        return _SiteFields;

    }

    _WaitUntilVisibleWithClick(waitUntilSelector = _btnNavNext, click = false, waitTime = 10000, message = "") {
        _WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: ' + waitUntilSelector + ' to be different after: ' + waitTime;
        if (message != "") {
            _message = message;
        }

        if (this.CheckIsVisible(_btnBlurredOverlay)) {
            this.ClickElementSimple(_btnBlurredOverlay);
            if (this.CheckIsVisible(_gridSelector)) {
                this.ClickElementSimple(_gridSelector);
            }
        }

        var funcSel = this._GetSelector;
        var result = browser.waitUntil(function () {
            var elem = funcSel(waitUntilSelector);
            if (elem != null) {
                return elem.isDisplayed();
            }
            return false;

        }, waitTime, _message);

        if (result && click) {
            var selObj = funcSel(waitUntilSelector)
            if (selObj !== null) {
                selObj.click();
            }
        }
    }


    _WaitUntilVisible(waitUntilSelector = _btnNavNext, waitTime = 10000, message = "") {
        _WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: ' + waitUntilSelector + ' to be different after: ' + waitTime;
        if (message != "") {
            _message = message;
        }

        if (this.CheckIsVisible(_btnBlurredOverlay)) {
            this.ClickElementSimple(_btnBlurredOverlay);
            if (this.CheckIsVisible(_gridSelector)) {
                this.ClickElementSimple(_gridSelector);
            }
        }

        var funcSel = this._GetSelector;
        var result = browser.waitUntil(function () {
            var elem = funcSel(waitUntilSelector);
            if (elem != null) {
                return elem.isDisplayed();
            }
            return false;

        }, waitTime, _message);
        return result;
    }

    _WaitUntilEnabled(waitUntilSelector = _btnNavNext, waitTime = 50000, message = "") {
        _WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: ' + waitUntilSelector + ' to be different after: ' + waitTime;
        if (message != "") {
            _message = message;
        }

        if (this.CheckIsVisible(_btnBlurredOverlay)) {
            this.ClickElementSimple(_btnBlurredOverlay);
            if (this.CheckIsVisible(_gridSelector)) {
                this.ClickElementSimple(_gridSelector);
            }
        }

        var funcSel = this._GetSelector;

        var result = browser.waitUntil(function () {
            var elem = funcSel(waitUntilSelector)// this._GetSelector(selector)
            if (elem != null) {
                return elem.isDisplayed();
            }
            return false;

        }, waitTime, _message);

    }

    _WaitUntilExist(waitUntilSelector, waitTime = 50000, message = "") {
        _WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: ' + waitUntilSelector + ' to be different after: ' + waitTime;
        if (message != "") {
            _message = message;
        }

        var funcSel = this._GetSelector;

        var result = browser.waitUntil(function () {

            var res = funcSel(_WaitUntilSelector);
            if (res !== null) {
                return res.isExisting();
            }
            return false;
        }, waitTime, _message);

        return result;

    }



    GenerateRandomEmail(emailDomain) {
        var text = "";
        var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        text += possible.charAt(Math.floor(Math.random() * possible.length));

        var finalText = text + Math.floor((Math.random() * 1000000000) + 1) + emailDomain;
        return finalText;
    }


    ScrollToView(attribute, attributeValue, waitForUntilVisible = "") {
        var searchSelector = '[' + attribute + '=' + '"' + attributeValue + '"' + ']'

        if (this._WaitUntilVisible(searchSelector)) {
            var sel = this._GetSelector(searchSelector);
            if (sel != null) {
                sel.scrollIntoView();
                if (waitForUntilVisible != "") {
                    this._WaitUntilVisible(waitForUntilVisible);
                }
            }

        }
    }

    // in some Special Cases where there is no ID or the ID is Dynamic, u can Click any Elem by giving a unique Attribute as a complete String 
    //example : var sel = $('[ng-click="showWageslipCarousel($event)"]') >> attribute = ng-click att-value= show...
    ClickElementByAttribute(attribute, attributeValue, waitForUntilVisible = "") {
        var searchSelector = '[' + attribute + '=' + '"' + attributeValue + '"' + ']'

        if (this._WaitUntilVisible(searchSelector)) {
            var sel = this._GetSelector(searchSelector);
            if (sel != null) {
                sel.click();
                if (waitForUntilVisible != "") {
                    this._WaitUntilVisible(waitForUntilVisible);
                }
            }

        }
    }

    // compare a value of innerText of a given Attribute with a give Text from the Usre and Click the element if match 
    CompareAndClickIfMatch(attribute, attributeValue, compareValue) {


        var searchSelector = '[' + attribute + '=' + '"' + attributeValue + '"' + ']';
        var sel = this._GetSelector(searchSelector);
        if (sel != null) {
            var selectorValue = sel.getText();
            if (selectorValue.trim() === compareValue.trim()) {
                sel.click();
            }
        }


    }

    _WaitUntilTitle(waitTime = 5000, message = "") {
        var _message = 'expected: title to be different after: ' + waitTime;
        if (message != "") {
            _message = message;
        }

        browser.waitUntil(function () {
            var title = browser.getTitle().includes(' | ');
            return title;

        }, waitTime, _message);

    }


    RefreshBrowser(selector = null, click = false) {
        browser.refresh();
        if (selector != null) {
            this._WaitUntilVisible(selector);
            if (click) {
                this.ClickElementSimple(selector);
            }
        }

    }

    LogTime(string = '') {
        if (string != '') {
            console.log(string);
        }
        var dt = this._GetTime();
        console.log(dt);
        return dt;
    }

    _GetTime(withoutSpecChar = false) {
        let dt = date.format(new Date(), 'YYYY:MM:DD HH:mm:ss').toString();
        dt = dt.replace(' ', '__');
        if (withoutSpecChar) {
            let dtNew = '';
            for (var i = 0; i <= dt.length - 1; i++) {
                if (dt[i] != ':' && dt[i] != '_' && dt[i] != ' ') {
                    dtNew += dt[i];
                }
            }
            return dtNew;
        }
        return dt;
    }


    InitBrowserStart(login=null,readxml = false, rktest=false) {
        var url = 'https://' + this.TargetUrl + '.' + this.TargetDom + '.de/' + this.TestComponent+ "/Account/Login?ReturnUrl=%2F"+ this.TestComponent+"%2F";

        browser.url(url);

        this.SaveScreenShot();

        //https://rc.xbav-berater.de/Vermittlerbereich/Account/Login?ReturnUrl=%2FVermittlerbereich%2F

        // Erstmal die Standard configuration auslesen
        // Alle Versicherer oder nur spezielle
        // Alle Kombinationen oder nur spezielle oder nur SmokeTest
        // SmokeTest := Nur erste funtkionierende Kombination
        if (readxml) {
            
            this._ReadXMLMainConfig();
            if(login !== null)
            {
                login.LoginUser(this.LoginName, this.LoginPassword);
            }
        }
        
        if(rktest)
        {
            this._ReadXMLAttributeCommon(true);
        }

    }

    // InitBrowserStart(readxml = true) {
    //     var url = 'https://' + this.TargetUrl + '.' + this.TargetDom + '.de' + '/Beratung/Account/Login?ReturnUrl=%2FBeratung%2F';
    //     if (String(this.TargetUrl).toUpperCase() == 'BERATUNG') {
    //         url = 'http://beratung.xbav-berater.de/Account/Login?ReturnUrl=%2F';
    //     }
    //     browser.url(url);

    //     this.SaveScreenShot();



    //     // Erstmal die Standard configuration auslesen
    //     // Alle Versicherer oder nur spezielle
    //     // Alle Kombinationen oder nur spezielle oder nur SmokeTest
    //     // SmokeTest := Nur erste funtkionierende Kombination
    //     if (readxml) {
    //         this._ReadXMLAttributeCommon(true);
    //     }

    // }

    SetListBoxValue(selector, value) {
        var fieldname = this._GetFieldName(selector);

        if (fieldname.includes('[')) {
            var ex = this._GetSelector(fieldname);
            fieldname = '#' + ex.getAttribute('id');
        }

        var exist = this._WaitUntilExist(fieldname);

        if (!exist) {
            throw new Error("Selector not found...");
        }

        var List = this._GetSelector(fieldname);
        var values = List.getAttribute("md-option[ng-repeat]", "value", true);
        var Ids = List.getAttribute("md-option[ng-repeat]", "id", true);

        var index = values.indexOf(value);

        if (Ids.length > 1) {
            this.ClickElementSimple(fieldname, 1000);
            if (index > -1) {
                this.ClickElement('#' + Ids[index]);
            }
            else {
                this.ClickElement('#' + Ids[0]);
            }
        }
    }

    SetSimpleListBoxValue(selector, value) {

        var exist = this._WaitUntilExist(selector);

        if (!exist) {
            throw new Error("Selector not found...");
        }

        var List = this._GetSelector(selector).getValue();
        //List.click();
        //List.click();
        List.setValue('Lastschrift');
    }

    GetText(selector) {
        var selObj = this._GetSelector(selector);
        if (selObj !== null && selObj.isDisplayed()) {

            return selObj.getText();

        }
        else {
            if (this.IsDebug)
                console.log("Selector: " + selector + " not found.")
            return false;
        }

    }


    AddChapter(vn = null, vp = null, consultation = null) {
        try {
            if (vn != null) {
                vn.AddVN(this.VnName, true, true);
            }
        } catch (ex) {
            this.Navigate2SitePrev(_VnAuswahl);
            vn.AddVN(this._GetTime(true), true, true);
        }

        try {
            if (vp != null) {
                vp.AddVP(this.VpName);
            }
        } catch (ex) {
            this.Navigate2SitePrev(this._VpAuswahl);
            vn.AddVP(this._GetTime(true));
        }

        try {
            if (consultation != null) {
                consultation.AddConsultation(true, true);
            }
        } catch (ex) {
            this.Navigate2SitePrev(this._ConsultationAuswahl);
            consultation.AddConsultation(true);
        }
    }

    Compare2Values(value1, value2) {
        var result = value1 === value2;
        return result;
    }


    _TakeErrorShot(message) {
        // Todo verbessern :-)

        var path = this.ErrorShotPath + message + '.png';
        console.log("path: " + path);

        let newMessage = '';
        if (message != null) {
            for (var i = 0; i <= message.length - 1; i++) {
                if (message[i] != ':' && message[i] != '_' && message[i] != ' ' && message[i] != '|' && message[i] != '–') {
                    newMessage += message[i];
                }
            }
        }

        if (_takeScreenShotAllDialogs === true) {
            this.PauseAction(2000);
        }

        browser.saveScreenshot(this.ErrorShotPath + newMessage + '.png')
    }


    _GetXmlConfigPath(pathFile) {
        var title = this.BrowserTitle;

        var index = title.indexOf('|');
        if (index > 0) {
            title = title.substr(0, index - 1);
        }

        // if (String(title).includes('Investmentauswahl')) {
        //     var x = "Y";
        // }

        var path = this.ExecutablePath + 'TestSuite\\' + this.Target + '\\' + _TestFolder + _TestConfigFolder + 'sites\\mandatory\\' + title + '.xml';

        if (pathFile != null) {
            path = pathFile;
        }

        return path;
    }


    _GetFieldName(element) {
        var fieldname = element;
        if (fieldname.substr(0, 1) != '.' && fieldname.substr(0, 1) != '[' && !fieldname.includes('Common:')) {
            fieldname = '#' + element;
        }

        return fieldname;
    }


    _CheckExMessage(message) {
        if (_ExMessageCnt > 2) {
            this._CleanUpExMessage();
            console.log(message);
            return;
        }

        if (_ExMessageCnt == 0 || _ExMessage.indexOf(message) >= 0) {
            _ExMessage[_ExMessageCnt++] = message;
        }
        else if (_ExMessage.indexOf(message) == -1) {
            this._CleanUpExMessage();
            this._CheckExMessage(message);
        }
    }

    _CleanUpExMessage() {
        for (var i = 0; i <= 2; i++) {
            _ExMessage[i] = '';
        }
        _ExMessageCnt = 0;
    }

    // Methode zum Automatisierten Füllen von Pflichtfeldern
    // Die Methode wird während des Navigierens aufgerufen (kann auch separat aufgerufen werden)
    // Wenn pathFile nicht angegeben wird, ermittelt sich der Name aus dem Titel der aktuellen Seite 
    // Ansonsten aus der Übergebenen Variablen
    // Falls eine Seite gefunden wird, werden die Felder extrahiert und ggfs. die Values gesetzt
    // Noch ein bisschen dirty aber schon funktionsfähig
    _CheckSiteFields(pathFile) {

        var configFile = this._GetXmlConfigPath(pathFile);

        if (fs.existsSync(configFile)) {

            var fields = this._ReadXMLFieldValues(configFile);
            for (var element = 0; element <= fields.length - 1; element++) {

                var __siteFieldName = null;
                var __siteFieldValue = null;
                var __siteFieldList = null;
                var __siteFieldExist = null;
                var __siteFieldClear = null;
                var __siteFieldCheck = null;
                var __siteFieldAdd = null;
                var __siteFieldCheckExist = null;
                var __siteFieldFieldValueArr = null;
                var __siteFieldFieldNameArr = null;
                var __siteFieldWarning = null;
                var checkBefore = null;

                __siteFieldName = this._GetFieldName(fields[element]['Name'][0]);
                if (__siteFieldName.includes("Common:")) {
                    __siteFieldFieldNameArr = this._GetCommonConfigFile(String(__siteFieldName).split(':')[1], false);

                    for (var fna = 0; fna <= __siteFieldFieldNameArr.length - 1; fna++) {
                        try {
                            var fn = __siteFieldFieldNameArr[fna];
                            if (!String(fn).includes('[')) {
                                fn = '#' + fn;
                            }
                            this._WaitUntilExist(fn, 1000);
                            __siteFieldName = fn;
                            if (this.IsDebug) {
                                console.log('SiteFieldName: ' + __siteFieldName);
                            }
                            break;
                        }
                        catch (ex) {

                        }

                    };

                    if (__siteFieldName.includes("Common:")) {
                        continue;
                    }


                }

                __siteFieldValue = fields[element]['Value'][0];
                if (__siteFieldValue.includes("Common")) {
                    __siteFieldFieldValueArr = this._GetCommonConfigFile(String(__siteFieldValue).split(':')[1]);
                }

                try {

                    __siteFieldCheckExist = this._CheckFieldAttribute('CheckExist', fields[element]);

                    if (__siteFieldCheckExist != null && String(__siteFieldCheckExist) !== _CurrentCheckID) {
                        continue;
                    }

                    __siteFieldList = this._CheckFieldAttribute('ListBox', fields[element]);
                    __siteFieldClear = this._CheckFieldAttribute('Clear', fields[element]);
                    __siteFieldCheck = this._CheckFieldAttribute('Check', fields[element]);
                    __siteFieldAdd = this._CheckFieldAttribute('Add', fields[element]);
                    __siteFieldWarning = this._CheckFieldAttribute('WarningField', fields[element]);

                    if (__siteFieldWarning != null) {
                        this.PauseAction(3000);
                        var warningBlock = this._GetSelector(__siteFieldWarning);
                        if (warningBlock != null) {
                            var text = browser.getText(warningBlock.selector);
                            if (text.includes(__siteFieldValue)) {
                                var exfield = this._CheckFieldAttribute('ExceptionField', fields[element]);
                                var exValue = this._CheckFieldAttribute('ExceptionValue', fields[element]);
                                if (exfield != null && exValue != null) {
                                    __siteFieldName = this._GetFieldName(exfield);
                                    __siteFieldName = this._GetSelector(__siteFieldName);
                                    browser.click(__siteFieldName.selector);
                                    this._WaitUntilEnabled();
                                    this.PauseAction(5000);
                                    this.ClickElementSimple(_btnNavNext);

                                }
                            }
                        }
                        continue;
                    }


                    try {
                        this._WaitUntilExist(__siteFieldName, 2000);
                        var enabled = browser.isEnabled(__siteFieldName);
                        if (!enabled) {
                            throw new Error(fieldName + " not enabled");
                        }
                    }
                    catch (ex) {

                        var exfield = this._CheckFieldAttribute('ExceptionField', fields[element]);
                        var exValue = this._CheckFieldAttribute('ExceptionValue', fields[element]);
                        if (exfield != null && exValue != null) {
                            __siteFieldName = this._GetFieldName(exfield);
                            __siteFieldValue = exValue;
                        }
                        else {
                            this._CheckExMessage(ex.message);
                        }

                    }

                    __siteFieldCheck = this._CheckFieldAttribute('Check', fields[element]);
                    __siteFieldAdd = this._CheckFieldAttribute('Add', fields[element]);

                } catch (ex) {
                    if (__siteFieldName !== '#Warning') {
                        console.log("Error: CheckSiteFields(WaitUntilExists): " + __siteFieldName + " " + ex.message);
                    }
                    return;
                }

                __siteFieldExist = this.CheckExist(__siteFieldName, 500);

                if (__siteFieldExist) {

                    this.PauseAction(300);

                    if (__siteFieldList != null && __siteFieldList === "true") {
                        if (__siteFieldName.includes('[')) {
                            var ex = this._GetSelector(__siteFieldName);
                            __siteFieldName = '#' + ex.getAttribute('id');
                        }

                        var exist = this.CheckExist(__siteFieldName, 500);
                        if (!exist) {
                            break;
                        }

                        var List = null;
                        var values = null;
                        var Ids = null;

                        try {
                            List = this._GetSelector(__siteFieldName);
                            values = List.getAttribute("md-option[ng-repeat]", "value", true);
                            Ids = List.getAttribute("md-option[ng-repeat]", "id", true);
                        }
                        catch (ex) {
                            this.RefreshBrowser();
                            this.PauseAction(500);
                            List = this._GetSelector(__siteFieldName);
                            values = List.getAttribute("md-option[ng-repeat]", "value", true);
                            Ids = List.getAttribute("md-option[ng-repeat]", "id", true);

                        }


                        var index = values.indexOf(__siteFieldValue);

                        var checkIsEnabled = browser.getAttribute(__siteFieldName, "disabled");


                        if (Ids.length > 1 && checkIsEnabled == null) {
                            try {
                                this.ClickElementSimple(__siteFieldName, 1000);

                            }
                            catch (ex) {
                                console.log("Error: CheckSiteFields: " + ex.message);
                                List.setValue("1");
                                browser.leftClick(List.selector, 10, 10);


                                var arrowSelektor = '.md-select-icon';
                                List = this._GetSelector(arrowSelektor);
                                if (List != null) {
                                    this.ClickElementSimple(arrowSelektor, 1000);
                                }
                            }

                            if (index > -1) {
                                this.ClickElement('#' + Ids[index]);
                            }
                            else {
                                this.ClickElement('#' + Ids[0]);
                            }



                        }
                    }
                    else {
                        if (__siteFieldValue === 'Click') {
                            var checkEnableBefore = this._CheckFieldAttribute('CheckEnableBefore', fields[element]);
                            if (checkEnableBefore != null && !browser.isEnabled(__siteFieldName)) {
                                break;
                            }

                            var checkFieldBefore = this._CheckFieldAttribute('CheckFieldBefore', fields[element]);
                            if (checkFieldBefore != null && browser.isExisting(checkFieldBefore)) {
                                break;
                            }



                            var checkBefore = this._CheckFieldAttribute('CheckBefore', fields[element]);
                            if (checkBefore != null) {
                                var searchSelector = this._GetSelector('#' + checkBefore)

                                var value = searchSelector.getValue();

                                if (value != null) {
                                    this.ClickElementSimple(__siteFieldName)
                                }
                            }
                            else {
                                this.ClickElementSimple(__siteFieldName)
                            }
                        }
                        else {
                            if (__siteFieldAdd != null && __siteFieldAdd === "true") {
                                this.ClickElement(__siteFieldName);
                                var sel = this._GetSelector(__siteFieldName);
                                sel.addValue(__siteFieldValue);
                            }
                            else {
                                if (__siteFieldName.substr(0, 1) === '[') {
                                    browser.click(__siteFieldName);
                                    if (__siteFieldFieldValueArr != null) {
                                        var sel = this._GetSelector(__siteFieldName);
                                        for (var fva = 0; fva <= __siteFieldFieldValueArr.length - 1; fva++) {
                                            this.SetValue(__siteFieldName, __siteFieldFieldValueArr[fva], 300, (__siteFieldCheck != null && __siteFieldCheck === "true" && fva == 0));

                                            var ex = this._GetSelector('.ng-scope.md-input-invalid.md-input-has-value');

                                            if (ex.type === 'NoSuchElement') {
                                                break;
                                            }
                                        }
                                    }
                                    else {
                                        this.SetValue(__siteFieldName, __siteFieldValue, 1000, (__siteFieldCheck != null && __siteFieldCheck === "true"));
                                    }
                                }
                                else {
                                    this.SetValue(__siteFieldName, __siteFieldValue, 100, (__siteFieldCheck != null && __siteFieldCheck === "true"));
                                }

                            }
                        }
                    }
                }

                if (__siteFieldCheckExist != null && String(__siteFieldCheckExist) === _CurrentCheckID) {
                    break;
                }

            };
        }
    }



    _CheckPopUp(clickSelector) {
        var selectorLeaveOrGo = '.swal2-confirm.md-button.md-raised.md-accent';
        if (this.CheckIsVisible(selectorLeaveOrGo)) {
            this._GetSelector(selectorLeaveOrGo).click();
            return true;
        }
        else {
            return false;
        }
    }

    _GetXmlParser(path) {
        var existsConfigFile = fs.existsSync(path);
        assert.equal(existsConfigFile, true);

        var parser = new xml2js.Parser();

        return parser;

    }

    _SetLeftMenuVisible() {

        this._WaitUntilVisible(_leftSiteMenu);

        var checkBlock = this._GetSelector(_leftSiteMenu);

        if (checkBlock.state == 'success') {
            if (checkBlock.getAttribute('class').indexOf('navbar-folded') >= 0) {
                this.ClickElement(this.MenueMinMax);
            }
        }
    }

    _ReadXMLAttributeForRKTest(standard = false) {

        var callback = this._CheckFieldListAttribute;
        this._GetXmlParser(this.MainConfigPath).parseString(this.Fs.readFileSync(this.MainConfigPath), function (err, result) {
            if (standard) {
                _AllVersicherer = result['Config']['VersichererList'][0].$['all'];
                _BreakAtError = result['Config']['VersichererList'][0].$['breakAtError'];
                _Documents = result['Config']['Tests'][0].$['documents'];
                _debug = result['Config']['Tests'][0].$['debug'];

                _SmokeTest = result['Config']['VersichererList'][0].$['smoke'];
                _TarifSelector = result['Config']['SelectorList'][0]['Selector'];
                _Versicherer = result['Config']['VersichererList'][0]['Versicherer'];
                _ExcludeVersicherer = callback('Versicherer', result['Config']['ExcludeList'][0]);
                _AllDurchfWege = result['Config']['DurchfwegList'][0].$['all'];
                _DurchfWege = result['Config']['DurchfwegList'][0]['DurchfWeg'];

                _AllTarife = result['Config']['TarifList'][0].$['all'];
                _Tarife = result['Config']['TarifList'][0]['Tarif'];
                _TarifSmoke = result['Config']['TarifList'][0].$['smoke'];
                _OnlyTarifCheck = result['Config']['VersichererList'][0].$['onlyTarifCheck'];

                _AllType = result['Config']['TypeList'][0].$['all'];
                _Types = result['Config']['TypeList'][0]['Type'];
                _TypeSmoke = result['Config']['TypeList'][0].$['smoke'];
            }
        })

        var varBaseFile = this.ExecutablePath + 'TestSuite\\' + this.Target + '\\' + _TestFolder + _TestConfigFolder + 'sites\\new\\vn\\Stammdaten.xml';

        var fields = this._ReadXMLFieldValues(varBaseFile);
        _VnName = fields[0]['Value'][0];

        varBaseFile = this.ExecutablePath + 'TestSuite\\' + this.Target + '\\' + _TestFolder + _TestConfigFolder + 'sites\\new\\vp\\Stammdaten.xml';

        fields = this._ReadXMLFieldValues(varBaseFile);
        _VpName = fields[0]['Value'][0];
    }

    _ReadXMLAttributeCommon(standard = false) {

        this._GetXmlParser(this.MainConfigPath).parseString(this.Fs.readFileSync(this.MainConfigPath), function (err, result) {
            if (standard) {
                _BreakAtError = result['Config']['Tests'][0].$['breakaterror'];
                _Documents = result['Config']['Tests'][0].$['documents'];
                _debug = result['Config']['Tests'][0].$['debug'];
            }
        })

        var varBaseFile = this.ExecutablePath + 'TestSuite\\' + this.ConfigLib + '\\' + this.TestComponent + '\\' + this.TestConfigFolder + '\\' + 'sites\\new\\vn\\Stammdaten.xml';

        var fields = this._ReadXMLFieldValues(varBaseFile);
        _VnName = fields[0]['Value'][0];

        varBaseFile = this.ExecutablePath + 'TestSuite\\' + this.ConfigLib + '\\' + this.TestComponent + '\\' + this.TestConfigFolder + '\\' + 'sites\\new\\vp\\Stammdaten.xml';

        fields = this._ReadXMLFieldValues(varBaseFile);
        _VpName = fields[0]['Value'][0];
    }

    _ReadXMLMainConfig() {

        this._GetXmlParser(this.MainConfigPath).parseString(this.Fs.readFileSync(this.MainConfigPath), function (err, configContent) {
                _BreakAtError             =  configContent['Config']['Tests'][0].$['breakaterror'];
                _debug                    =        configContent['Config']['Tests'][0].$['debug'];
                _LoginName             = configContent['Config']['Workflow'][0]['Login'][0]['Name'][0];
                _LoginPassword              =  configContent['Config']['Workflow'][0]['Login'][0]['Password'][0];
                _WorkflowSelector = configContent['Config']['Workflow'][0]['Arbeitgeber'][0];
            });
    }




    _GetCommonConfig(list, value, field) {

        if (field.Name == __xpath) {
            var result = [field[list][0][value].length];
            var count = 0;
            field[list][0][value].forEach(element => {
                result[count++] = element;
            });
            __xpathResult = result;
        }

        return __xpathResult;
    }


    _GetCommonConfigFile(xpath, values = true) {
        __xpath = xpath;
        __xpathResult = null;
        var varBaseFile = this.CommonConfigPath;
        var fields = this._ReadXMLFieldValues(varBaseFile);
        var list = 'ValueList';
        var value = 'Value';
        if (!values) {
            list = 'NameList';
            value = 'Name';
        }

        fields.forEach(field => {
            __xpathResult = this._GetCommonConfig(list, value, field);
        });

        if (__xpathResult == null) {
            __xpathResult = [0];
        }
        return __xpathResult;
    }



    _CheckFieldAttribute(attributeName, element) {
        var result = null
        try {
            result = element[attributeName][0];

        }
        catch (ex) {
        }

        return result;
    }

    _CheckFieldListAttribute(attributeName, element) {
        var result = null
        try {
            result = element[attributeName];

        }
        catch (ex) {
            console.log("Error: CheckFieldListAttribute: " + ex.message);
        }

        return result;
    }


    _GetElementFromConfig(elementArr) {
        var res = null;

        try {
            this._GetXmlParser(this.MainConfigPath).parseString(this.Fs.readFileSync(this.MainConfigPath), function (err, result) {
                res = result['Config'];
                elementArr.forEach(function (el) {
                    res = res[el];
                    if (res != null) {
                        res = res[0];
                    }
                })
            })
        } catch (ex) {
            console.log("Error: GetElementFromConfig: " + ex.message);
        }


        return res;
    }

    _GetNewChapterList(chapter) {
        var resultArr = [_NewChapterList.length];
        _NewChapterList.forEach(function (element, index) {
            if (element === '') {
                resultArr[index] = chapter;
            }
            else {
                resultArr[index] = element;
            }
        });

        return resultArr;
    }

    _AddChapter(chapter, btnNew, waitUntilSelector = '', callbackFunc = null, saveLastSite = true) {
        this.PauseAction(500);
        var Sites = this._GetElementFromConfig(this._GetNewChapterList(chapter));
        var path = Sites.$['path'];
        var url = null;

        Sites['Site'].forEach(element => {

            // if(fs.existsSync(configFileName))
            // {

            var url = element['Url'][0];
            var BtnClick = this._CheckFieldAttribute('NewBtn', element);
            if (url == 'new') {
                var sel = this.CheckIsVisibleGetSelector(btnNew, 15000, true);
                if (waitUntilSelector != null) {
                    this._WaitUntilVisible(waitUntilSelector);
                }
            }
            else if (BtnClick != null) {
                this.Navigate2Site(url);
                this.ClickElement('#' + BtnClick)
            }
            else {
                this.Navigate2Site(url);
            }
            var fileName = element['FileName'][0];
            var configFileName = this.ExecutablePath + 'TestSuite\\' + this.ConfigLib + '\\' + this.TestComponent + '\\' + this.TestConfigFolder + '\\' + 'sites\\new\\' + path + '\\' + fileName;


            if (fileName == 'Callback' && callbackFunc != null) {
                callbackFunc(element);
            }
            else {
                this._CheckSiteFields(configFileName);
            }
        });
        if (saveLastSite) {

            this.Next();
            if (url != null && url != 'new') {
                this.Navigate2SitePrev(url);
            }
            else {
                this.Prev();
                this.PauseAction(1000);
            }
        }
    }

    _CheckisEnabled(selector, waitTime = 3000) {
        this._WaitUntilVisible(selector, waitTime);
        var result = this._GetSelector(selector).isEnabled();
        return result;
    }

    _DeleteAllFilesFromDirectory(directory) {
        fs.readdir(directory, (err, files) => {
            if (err) throw err;

            for (const file of files) {
                fs.unlink(path.join(directory, file), err => {
                    if (err) throw err;
                });
            }
        });
    }

    _GetValuefromArg(key) {
        for (var i = 0; i <= process.argv.length - 1; i++) {
            var value = process.argv[i];
            if (value.includes("--" + key)) {
                return value.substr(6);
            }
        }
    }

    _GetSelector(selector) {

        try {

            var selectorPos = _SelectorIndexArr.indexOf(selector);
            if (selectorPos < 0) {
                var sel = $(selector);

                if (sel !== null) {
                    _SelectorIndexArr.push(selector);
                    _SelectorArr.push(sel);
                    return sel;
                }
            }
            else {
                var sel = _SelectorArr[selectorPos];
                return sel;
            }
        } catch (ex) {
            return null;
        }
    }

    _GetSelectorFromAttribute(attribute, attributeValue) {

        try {

            var searchSelector = '[' + attribute + '=' + '"' + attributeValue + '"' + ']'
            return this._GetSelector(searchSelector);
        } catch (ex) {
            return null;
        }
    }

    _InitSelectorIndex() {
        try {
            for (var i = 0; i <= _SelectorArr.length - 1; i++) {
                _SelectorArr[i] = null;
                _SelectorIndexArr[i] = null;
            }
        } catch (ex) {

        }
    }

    _test(eins, zwei) {
        var x = this._GetSelector(eins, zwei);
        var z = x.isDisplayed();
        x = this._GetSelector(eins, zwei);
        z = x.isDisplayed();

    }
// List box ohne Angular und mit generieter ID 
    _SetComplexListBoxValue(searchText, selector, attribute="",searchSelector="id")
        {
            try
            {
                var listSelector = this._GetSelector(selector);
                var html = null;
                if(listSelector !== null)
                {
                    html = listSelector.getHTML();
                    listSelector.click();
                    
                    if(attribute !== "")
                    {
                        var attr = listSelector.getAttribute(attribute)
                        var attrSel = this._GetSelector('#'+attr);
                        if(attrSel !== null)
                        {
                            html = attrSel.getHTML();
                        }
                    }
                }
    
                if(html === null)
                {
                    return null;
                }
        
                var searchTextIndex = html.indexOf(searchText);
                var stringToken = html.substr(0,searchTextIndex);
                var searchIDIndex = stringToken.lastIndexOf(searchSelector);
                var stringToken = stringToken.substr(searchIDIndex, searchTextIndex)
                var splitArr = stringToken.split('"');
                if(splitArr.length >= 1 && splitArr[1] !== null && splitArr[1].includes("select"))
                {
                    var clickElem = this._GetSelector("#"+splitArr[1]);
                    if(clickElem !== null)
                    {
                        clickElem.click(); 
                    }
                    
                }
            }
            catch(ex)
            {
                if (this.IsDebug)
                    console.log("Warning: Text: " + searchText + " not found.")
            }
            return null;
            
        }    


    WalkThroughWorkflow()
    {
        this.WorkflowSelector.Selector.forEach(value => {
            var type                =  value['Type'][0];
           var attributeName       =  value['AttributeName'][0];
           var attributeValue      =  value['AttributeValue'][0];
           var action              =  value['Action'][0]._;
           var waitUntilSelector   =  value['Action'][0].$['waitSelector'];
           var value2Set   =  "";

           try
           {
                value2Set = value['Action'][0].$['value'];
           }
           catch(ex)
           {

           }
           
           if(type == "Attribute")
           {
             if(action === "Click")
             {
                this.ClickElementByAttribute(attributeName, attributeValue, waitUntilSelector);
             }
                   
           }
           else if(type == "Id")
           {
               if(action === "Click")
               {
                    this.ClickElement("#"+attributeValue,waitUntilSelector);
               }

               if(value2Set != "")
               {
                  this.SetValue("#"+attributeValue,value2Set)
               }
           }
           else if(type == "TextSelector")
           {
               if(action === "Click")
               {
                    this.ClickElement("//"+attributeValue,waitUntilSelector);
               }

               if(value2Set != "")
               {
                  this.SetValue("#"+attributeValue,value2Set)
               }
           }
           else if(type == "ComplexList")
           {
  
                this._SetComplexListBoxValue(value2Set, attributeValue);

           }     
           else if(type == "Class")
           {
               if(action === "Click")
               {
                    this.ClickElement("."+attributeValue,waitUntilSelector);
               }

               if(value2Set != "")
               {
                  this.SetValue("."+attributeValue,value2Set)
               }
           }                 
           
        });        

    }

    // this Method reads the config file and for ex, if the list from simplelist type , the click element method will be called 
    // <?xml version="1.0" encoding="utf-8"?>
    // <Config xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    // <Maincontext = "Portal">
    // <Subcontext>
    // <Login>
    // <Fields>
    // 	<Name>Common:InvestmentFond</Name>
    // 	<Value></Value>
    // 	<SimpleList>true</SimpleList>
    // 	<Attribute>value</Attribute>
    // 	<Check>true</Check>
    //   </Field>  
    // </Fields>
    // </Config>
    // </Login>
    // </Subcontext>
    // </Maincontext>
    // ---------------------------------------------------------------------------------------
    //  _ReadXMLcONFIG()
    //  {
    //     // liest config
    //     // iterate through fields
    //     var sel = $(Name);
    //     if(type == simplelist)
    //     {
    //         this.ClickElementByAttribute(value, '1');
    //     }

    //  }


}





module.exports = TestLib;






