var assert = require('assert');
var date = require('date-and-time');
var KernelLib = require('./KernelLib.js')
const kernelLib = new KernelLib();


var _Navigate2SiteIterator = 0;
var _SearchIterator = 0;
var _ClearValueIterator = 0;

class TestLib {

    get VnName() { return kernelLib.VnName };
    get VpName() { return kernelLib.VpName };

    get BtnMainAgency(){return kernelLib.BtnMainAgency};

    get IsDebug() { return kernelLib.IsDebug };
    get TypeSmoke() { return kernelLib.TypeSmoke };
    get Types() { return kernelLib.Types };
    get AllTypes() { return kernelLib.AllTypes };

    set CurrentID(value) { kernelLib.CurrentID = value };


    get TarifSmoke() { return kernelLib.TarifSmoke };
    get OnlyTarifCheck() { return kernelLib.OnlyTarifCheck };
    get Tarife() { return kernelLib.Tarife };

    get AllTarife() { return kernelLib.AllTarife };

    get NavChapterTarif() { return kernelLib.NavChapterTarif };
    get NavChapterAngebot() { return kernelLib.NavChapterAngebot };
    get NavChapterDokumente() { return kernelLib.NavChapterDokumente };

    get StatusSiteTitle() { return kernelLib.StatusSiteTitle };

    get LinkAngebotKurzUebersicht() { return kernelLib.LinkAngebotKurzUebersicht };

    get UrlTimeOut() { return kernelLib.UrlTimeOut };

    get BreakAtError() { return kernelLib.BreakAtError };

    get AllDurchfWege() { return kernelLib.AllDurchfWege };

    get DurchfWege() { return kernelLib.DurchfWege };

    get BtnBlurredOverlay() { return kernelLib.BtnBlurredOverlay };

    // Gibt die VersichererList aus Config Datei zurück
    get Versicherer() { return kernelLib.Versicherer };

    get ExcludeVersicherer() { return kernelLib.ExcludeVersicherer };

    // Einheitliche Rückgabe des Titels
    get Get_BrowserTitle() { return browser.getTitle() }


    // Returns Schlter ob alle Versicherer geprüft werden oder nur die aus der List
    // Wandelt um on Boolean
    get AllVersicherer() { return kernelLib.AllVersicherer }

    // Returns Smoke Test ja oder nein
    // Wandelt um in Boolean
    get SmokeTest() { return kernelLib.SmokeTest };

    // Returns TarifSelektoren aus Config
    get TarifSelectoren() { return kernelLib.TarifSelectoren };

    // Mit Document Test oder nicht
    get DocumentTest() { return kernelLib.DocumentTest }

    get MenueMinMax() { return kernelLib.MenueMinMax };
    get BtnNavNext() { return kernelLib.BtnNavNext };
    get BtnNavPrev() { return kernelLib.BtnNavPrev };

    get BtnFastForward() { return kernelLib.BtnFastForward };


    get TarifSiteSelector() { return kernelLib.TarifSiteSelector };


     // Loggt den Browser Title und prüft, falls assertString nicht leer ist
     ShowBrowserTitle(assertString = '') {
        console.log("Broser Title: " + this.Get_BrowserTitle)
        if (assertString != '') {
            assert.equal(this.Get_BrowserTitle, assertString);
        }

    }


    TakeErrorShot(message) {
        // Todo verbessern :-)
        message = message.replace(':', '_');
        message = message.replace(':', '_');
        message = message.replace(':', '_');
        message = message.replace(':', '_');
        message = message.replace(':', '_');
        browser.saveScreenshot(kernelLib.ErrorShotPath + message + '.png')
    }

    InitBrowserStart() {
        var url = 'http://' + kernelLib.TargetUrl + '.' + kernelLib.TargetDom + '.de' + '/Beratung/Account/Login?ReturnUrl=%2FBeratung%2F';
        if (kernelLib.TargetUrl == 'beratung') {
            url = 'http://beratung.xbav-berater.de/Account/Login?ReturnUrl=%2F';
        }
        browser.url(url);


        // Erstmal die Standard configuration auslesen
        // Alle Versicherer oder nur spezielle
        // Alle Kombinationen oder nur spezielle oder nur SmokeTest
        // SmokeTest := Nur erste funtkionierende Kombination
        kernelLib.ReadXMLAttribute(true);

    }

    SetListBoxValue(selector, value) {
        var fieldname = kernelLib.GetFieldName(selector);

        if (fieldname.includes('[')) {
            var ex = $(fieldname);
            fieldname = '#' + ex.getAttribute('id');
        }

        var exist = kernelLib.WaitUntilExist(fieldname);

        if (!exist) {
            throw new Error("Selector not found...");
        }

        var List = $(fieldname);
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


    // Sucht ein Element (Selector) und ruft die Methode zum Setzen eines Values auf
    // Wird der Selector nicht gefunden, wird abgebrochen
    // Wenn ein Pause value übergeben wird, wird Pausiert
    SetValue(selector, value, pauseTime = 0, checkExist = false) {
       kernelLib.SetValue(selector, value, pauseTime, checkExist);

    }

    CompareValue(selector, value) {
        if (this.IsVisible(selector)) {
            var searchSelector = $(selector)

            var check = searchSelector.getValue()
            if (check != null) {
                return searchSelector.getValue() === value;
            }
            else {
                var text = browser.getText(searchSelector.selector);

                return (text.includes(value));
            }
        }
        else {
            console.log("Selector: " + selector + " not found.")
            return false;
        }

    }

    IsEnabled(selector, waitTime = 3000) {
      return kernelLib.WaitUntilEnabled(selector, waitTime);
    }

    IsVisible(selector, waitTime = 10000) {
      return kernelLib.WaitUntilVisible(selector, waitTime);
    }



    // Navigiert zur Seite des Übergebenen Seitentitels
    Navigate2Site(title, failSite = '') {
        try {

            if (_Navigate2SiteIterator >= 20) {
                _Navigate2SiteIterator = 0;
                throw new Error("Zu viele Navigate2Site Iterationen");
            }
            while (true) {

                try {

                    kernelLib.WaitUntilTitle();
                    if (this.IsDebug) {
                        console.log(this.Get_BrowserTitle);
                    }
                }
                catch (ex) {
                    console.log("Error: Navigate2Site(WaitUntilTitle): " + ex.message);
                }



                if (String(this.Get_BrowserTitle).includes(title)) {
                    _Navigate2SiteIterator = 0;
                    this.PauseAction(500);
                    break;
                }

                if (failSite != '') {
                    var fSiteArr = String(failSite).split(":");
                    var indexFail = this.Get_BrowserTitle.indexOf(fSiteArr[0]);
                    if (indexFail >= 0) {
                        this.Jump2Chapter(fSiteArr[1], fSiteArr[2]);
                        this.Navigate2Site(title, failSite);
                    }
                }

                if (this.IsVisible(this.BtnNavNext, 5000)) {
                    this.ClickElement(this.BtnNavNext);
                    kernelLib.CheckSiteFields();
                } else {
                    throw new Error("BtnNavNext not visible in 5 sec")
                }

            }
        } catch (ex) {
            _Navigate2SiteIterator += 1;
            var conslog = !ex.message.includes('is not clickable at point') && !ex.message.includes('obscures it')  && !ex.message.includes('BtnNavNext not visible in 5 sec');
            if (conslog) {
                console.log("Error: Navigate2Site: " + ex.message);
            }
            this.Navigate2Site(title, failSite);
        } finally {
            kernelLib.CheckSiteFields();
        }
    }

    Jump2Chapter(chapter, link) {
        kernelLib.SetLeftMenuVisible();
        if (!this.IsVisible(link, 1000)) {
            this.ClickElement(chapter, link);
        }

        this.ClickElement(link);
    }

    ClearValue(selector) {
        try {
            if (_ClearValueIterator >= 20) {
                _ClearValueIterator = 0;
                throw new Error("Zu viele Iterationen ClearElement");
            }
            var element = $(selector);

            if (_ClearValueIterator > 0) {
                this.PauseAction(1000);
            }

            element.clearElement();

            if (element.getValue() != "") {
                _ClearValueIterator += 1;
                this.ClearValue(selector);
            }

        }
        catch (ex) {
            console.log("Error: ClearElementValue: " + ex.message);
            _ClearValueIterator += 1;

        }

    }

    SelectHauptAgentur() {
        kernelLib.WaitUntilVisible(this.BtnMainAgency);
        this.ClickElementSimple(this.BtnMainAgency);
        kernelLib.WaitUntilVisible(kernelLib.BtnNewVn);
    }

    Next(waitTime = 0, checksitefields = false) {
        if (checksitefields) {
            kernelLib.CheckSiteFields();
        }
        this.PauseAction(waitTime);
        this.ClickElement(this.BtnNavNext);
    }

    Previous(waitTime = 0) {
        this.PauseAction(waitTime);
        this.ClickElement(tgus,);
    }

    NextPrevious(waitTime = 0, checksitefields = false)
    {
        this.Next(waitTime, checksitefields);
        this.Previous(waitTime);
    }

    ClickElementSimple(selector, pauseTime = 0) {
     return kernelLib.OnlyClickAction(selector, pauseTime)
    }

    ClickElement(selector, waitforVisibleSelector = '', timeout = 50000, pauseTime = 0, click = false) {
      return   kernelLib.ClickAction(selector, waitforVisibleSelector, timeout, pauseTime, click)
    }

    PauseAction(pauseTime) {
        if (pauseTime > 0) {
            browser.pause(pauseTime);
        }
    }

    CheckVersion() {
        if (kernelLib.Version !== '') {
            var t = browser.getText('#container-main');
            console.log(t);
            assert.notEqual(t.indexOf('Version ' + kernelLib.Version), -1, "Fehlerhafte Version ausgliefert.");
        }
    }


    AddChapter(chapter, btnNew, waitUntilSelector = '', callbackFunc = null) {
        this.PauseAction(500);
        var Sites = kernelLib.GetElementFromConfig(kernelLib.GetNewChapterList(chapter));
        var path = Sites.$['path'];
        
        Sites['Site'].forEach(element => {
           
            // if(fs.existsSync(configFileName))
            // {
    
            var url = element['Url'][0];
            var BtnClick = kernelLib.CheckFieldAttribute('NewBtn',element);
            if(url == 'new')
            {
                this.IsVisible(btnNew,3000);
                this.ClickElement(btnNew);
                if(waitUntilSelector !== '')
                {
                    this.IsVisible(waitUntilSelector);
                }

            }
            else if(BtnClick != null)
            {
                this.Navigate2Site(url);
                this.ClickElement('#'+BtnClick)
            }
            else
            {
                this.Navigate2Site(url);
            }
            var fileName = element['FileName'][0];
            var configFileName = kernelLib.ExecutablePath+'TestSuite\\'+kernelLib.TargetUrl+'\\' +kernelLib.TestFolder+kernelLib.TestConfigFolder+'sites\\new\\'+path+'\\'+fileName;    

            if(fileName == 'Callback' && callbackFunc != null)
            {
                callbackFunc(element);
            }
            else
            {
                   kernelLib.CheckSiteFields(configFileName);
            }
        //}
        // else
        // {
        //     console.log('Config Datei: '+configFileName+' existiert nicht.')
        // }
		});        
    }

    RefreshBrowser(selector = null, click = false) {
        browser.refresh();
        if (selector != null) {

            if (click && this.IsVisible(selector, 5000)) {
                this.ClickElementSimple(selector);
            }
        }
    }

    LogTime(string = '') {
        if (string != '') {
            console.log(string);
        }
        let dt = date.format(new Date(), 'YYYY:MM:DD HH:mm:ss').toString();
        dt = dt.replace(' ', '__');
        console.log(dt);
        return dt;
    }
}
module.exports = TestLib;






