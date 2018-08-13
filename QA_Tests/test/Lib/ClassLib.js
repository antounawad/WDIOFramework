var assert = require('assert');
var fs = require('fs'),
 xml2js=require('xml2js')
 var date = require('date-and-time');
  
 var defaultTimout = 10000;

 var _UrlTimeOut = 999999999999999999999999999999999999999999;

// Mit Dokumentgenerierung oder nicht
var _Documents = false;
// Versicher Liste (falls in config angegeben)
var _Versicherer = null;
var _DurchfWege = null;
var _Tarife =  null;
var _ExcludeVersicherer = null;
// Speichert die TarifSelektoren
var _TarifSelector = null;
// Alle Versicherer oder nur bestimmte
var _AllVersicherer = false;
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

var _NewChapterList = ['New','Chapter','','Sites'];

var _btnBlurredOverlay = '#btnBlurredOverlay';

var _gridSelector = '#tableList';

var _btnMainAgency = '#btnXbavMainAgency';

var _btnNewVn = '#btnNewVn';

var _NavchapterTarif = '#navChapterLink_1'; // Arbeitgeber
var _NavchapterAngebot = '#navChapterLink_6' // Angebot
var _NavchapterDokumente = '#navChapterLink_8' // Dokumente
var _StatusSiteTitle = 'Abschluss - Status';
var _LinkAngebotKurzUebersicht = '#navViewLink_AngebotAngebotVersichererangebot';

var _OnlyTarifCheck = false;

var _CurrentCheckID = 0;



class TestLib{

    get TypeSmoke(){return _TypeSmoke === 'true'};
    get Types(){return _Types};
    get AllTypes(){return _AllType === 'true'};

    set CurrentID(value){_CurrentCheckID = value};


    get TarifSmoke(){return _TarifSmoke === 'true'};
    get OnlyTarifCheck(){return _OnlyTarifCheck === 'true'};
    get Tarife(){return _Tarife};

    get AllTarife(){return _AllTarife === 'true'};

    get NavChapterTarif(){return _NavchapterTarif};
    get NavChapterAngebot(){return _NavchapterAngebot};
    get NavChapterDokumente(){return _NavchapterDokumente};

    get StatusSiteTitle(){return _StatusSiteTitle}

    get LinkAngebotKurzUebersicht(){return _LinkAngebotKurzUebersicht};

   get UrlTimeOut(){return _UrlTimeOut};

   get BtnMainAgency(){return _btnMainAgency};

   get BreakAtError(){return _BreakAtError === 'true'};

   get AllDurchfWege(){return _AllDurchfWege === 'true'};
   
   get DurchfWege(){return _DurchfWege};

    get BtnBlurredOverlay(){return _btnBlurredOverlay};
    //Wegen Config Dateien.
    get ExecutablePath(){ return _executablePath};

    get ErrorShotPath(){return _executablePath+'errorShots\\'}

    // Gibt die VersichererList aus Config Datei zurück
    get Versicherer(){ return _Versicherer};

    get ExcludeVersicherer(){return _ExcludeVersicherer};
    
    // FileStream für Config Dateien
    get Fs(){return fs};



    // Übergebenes Projekt --hotfix aus Args
     get TargetUrl() {return process.argv[3].substr(2)}
     //get TargetUrl() { return process.argv[5].substr(2)}

     get TargetDom() { return process.argv[4].substr(2)}
     //get TargetDom() { return process.argv[6].substr(2)}

     // Returns Version aus Args
     get Version() 
     {
         let ver = process.argv[5]
         //let ver = process.argv[7]
         if(ver != null)
         {
             return ver.substr(2);
         }
         return '';
     }

     // Returns Main Config Pfad
     get MainConfigPath() {return this.ExecutablePath+'test\\config\\'+this.TargetUrl+'\\Config.xml'}

     // Einheitliche Rückgabe des Titels
     get BrowserTitle() {return browser.getTitle()}

    // Returns Versicher Liste aus Config falls angegeben
    get Versicherer() {return _Versicherer}

    // Returns Schlter ob alle Versicherer geprüft werden oder nur die aus der List
    // Wandelt um on Boolean
    get AllVersicherer() {return _AllVersicherer === 'true'}

    // Returns Smoke Test ja oder nein
    // Wandelt um in Boolean
    get SmokeTest() {return _SmokeTest === 'true'}

    // Returns TarifSelektoren aus Config
    get TarifSelectoren(){return _TarifSelector}

    // Mit Document Test oder nicht
    get DocumentTest(){return _Documents === 'true'}

    // Loggt den Browser Title und prüft, falls assertString nicht leer ist
    ShowBrowserTitle(assertString='')
    {
        console.log("Broser Title: "+this.BrowserTitle)
        if(assertString != '')
        {
            assert.equal(this.BrowserTitle, assertString);
        }

    }

    set WaitUntilSelector(value)
    {
        _WaitUntilSelector = value;
    }

    get WaitUntilSelector()
    {
        return _WaitUntilSelector;
    }

    get MenueMinMax(){return _MenueMinMax};

    get BtnNavNext(){return _btnNavNext};

    get BtnFastForward(){return _btnFastForward};


    get TarifSiteSelector(){return _TarifSiteSelector};


    TakeErrorShot(message)
    {
        browser.saveScreenshot(this.ErrorShotPath+message+'.png')
    }
    // Sucht ein Element (Selector) und ruft die Methode zum Setzen eines Values auf
    // Wird der Selector nicht gefunden, wird abgebrochen
    // Wenn ein Pause value übergeben wird, wird Pausiert
    SearchElement(selector,value, pauseTime=0, checkExist=false){
        try
        {
            if(_SearchIterator >= 100)
            {
                throw new Error("Zu viele SearchElement Iterationen");
            }
            var searchSelector = $(selector)
            assert.notEqual(searchSelector, null)

            var entryValue = searchSelector.getValue();
           

            if(checkExist)
            {
                if(entryValue != null && entryValue != "")
                {
                    return;
                }
            }

           searchSelector.setValue(value);
           var retValue = searchSelector.getValue();

            if(value != retValue)
            {
                this.OnlyClickAction(searchSelector.selector);
                this.PauseAction(500);
                searchSelector.addValue(100);
    
                _SearchIterator += 1;
                this.SearchElement(selector, value, 1000);
            }
            this.PauseAction(pauseTime);
        }catch(ex)
        {
            console.log("Error: SearchElement: "+ex.message);
            _SearchIterator += 1;
            this.SearchElement(selector, value, 1000);

        }
		
    }

    CheckisEnabled(selector)
    {
        this.WaitUntilVisible(selector);
        var result =  browser.isEnabled(selector);
        return result;
    }

    CheckIsVisible(selector)
    {
        var result = browser.isVisible(selector);
        return result;
    }

    // Navigiert zur Seite des Übergebenen Seitentitels
    Navigate2Site(title, failSite='')
    {
        try{

            if(_Navigate2SiteIterator >= 50)
            {
                throw new Error("Zu viele Navigate2Site Iterationen");
            }
            while(true)
            { 

                try{
                   
                    this.WaitUntilTitle();
                }
                catch(ex)
                {
                    console.log("Error: Navigate2Site(WaitUntilTitle): "+ex.message);
                }
    
    

                if(String(this.BrowserTitle).includes(title))
                {
                    _Navigate2SiteIterator = 0;
                    this.PauseAction(500);
                    break;
                }

                if(failSite != '')
                {
                    var fSiteArr = String(failSite).split(":");
                    var indexFail = this.BrowserTitle.indexOf(fSiteArr[0]);
                    if(indexFail >= 0)
                    {
                        this.Jump2Chapter(fSiteArr[1],fSiteArr[2]);
                        this.Navigate2Site(title, failSite);
                     }
                }                

                this.WaitUntilVisible(this.BtnNavNext);
                this.ClickAction(this.BtnNavNext);

                this.CheckSiteFields();
            }
        }catch(ex){
            _Navigate2SiteIterator += 1;
            var conslog = !ex.message.includes('is not clickable at point') && ex.message.includes('obscures it');
            if(conslog)
            {
                console.log("Error: Navigate2Site: "+ex.message);
            }
            this.Navigate2Site(title, failSite);
        }finally
        {
            this.CheckSiteFields();
        }
    }

    Jump2FailSite(failSite,title)
    {
        if(failSite === '')
        {
            this.Navigate2Site(title);
            return;
        }
        var chapterLink = String(failSite).split(":");
        this.Jump2Chapter(chapterLink[0], chapterLink[1])
    }

    Jump2Chapter(chapter, link)
    {
        this.SetLeftMenuVisible();
        if(!this.CheckIsVisible(link))
        {
            this.ClickAction(chapter, link);
        }

		this.ClickAction(link);
    }



    GetXmlConfigPath(pathFile)
    {
        var title = this.BrowserTitle;



        var index = title.indexOf('|');
        if(index >  0)
        {
            title = title.substr(0,index-1);
        }

        if(String(title).includes('Eigenbeteiligung'))
        {
            var x = "Y";
        }

        var path = this.ExecutablePath+'test\\config\\sites\\mandatory\\'+title+'.xml';

        if(pathFile != null)
        {
            path = pathFile;
        }

        
        return path;
    }

    ClearElementValue(elementName)
    {
        try
        {
            if(_ClearElementIterator >= 100)
            {
                throw new Error("Zu viele Iterationen ClearElement");
            }
            var element = $(elementName);

            if(_ClearElementIterator > 0)
            {
                this.PauseAction(1000);
            }
            
            element.clearElement();
                                    
            if(element.getValue() != "")
            {
                _ClearElementIterator += 1;
                this.ClearElementValue(elementName);
            }
    
        }
        catch(ex)
        {
            console.log("Error: ClearElementValue: "+ex.message);
            _ClearElementIterator += 1;

        }

    }

    GetFieldName(element)
    {
        var fieldname  = element;
        if(fieldname.substr(0,1)!='.' && fieldname.substr(0,1)!='[')
        {
            fieldname  = '#'+element;
        }
        return fieldname;

    }

    // Methode zum Automatisierten Füllen von Pflichtfeldern
    // Die Methode wird während des Navigierens aufgerufen (kann auch separat aufgerufen werden)
    // Wenn pathFile nicht angegeben wird, ermittelt sich der Name aus dem Titel der aktuellen Seite 
    // Ansonsten aus der Übergebenen Variablen
    // Falls eine Seite gefunden wird, werden die Felder extrahiert und ggfs. die Values gesetzt
    // Noch ein bisschen dirty aber schon funktionsfähig
    CheckSiteFields(pathFile)
    {
       
        var configFile = this.GetXmlConfigPath(pathFile);

        if(fs.existsSync(configFile))
        {

            var fields = this.ReadXMLFieldValues(configFile);
            fields.forEach(element => {

                var fieldname  =  null;
                var fieldValue = null;
                var list = null;
                var exist = null;
                var clear = null;
                var check = null;
                var add = null;
                var checkExist = null;

                fieldname = this.GetFieldName(element['Name'][0]);
                fieldValue = element['Value'][0];


                try
                {
                    
                    checkExist = this.CheckFieldAttribute('CheckExist',element);

                    if(checkExist != null && String(checkExist) !== _CurrentCheckID)
                    {
                        return;
                    }

                    list =  this.CheckFieldAttribute('ListBox',element);
                    clear = this.CheckFieldAttribute('Clear',element);
                    check = this.CheckFieldAttribute('Check',element);
                    add = this.CheckFieldAttribute('Add', element);

                    try
                    {
                        this.WaitUntilExist(fieldname,2000);
                        var enabled  = browser.isEnabled(fieldname);
                        if(!enabled)
                        {
                            throw new Error(fieldName+" not enabled");
                        }
                    }
                    catch(ex)
                    {
                        console.log(ex.message);
                        var exfield = this.CheckFieldAttribute('ExceptionField', element);
                        var exValue = this.CheckFieldAttribute('ExceptionValue', element);
                        if(exfield != null && exValue != null)
                        {
                            fieldname = this.GetFieldName(exfield);
                            fieldValue = exValue;
                        }
                    }

                    check = this.CheckFieldAttribute('Check',element);
                    add = this.CheckFieldAttribute('Add', element);

                }catch(ex)
                {
                    console.log("Error: CheckSiteFields(WaitUntilExists): "+fieldname+" "+ex.message);
                    return;
                }
                
                exist = browser.isExisting(fieldname);

                if(exist)
                {

                    this.PauseAction(300);

                    if(list != null && list === "true")
                    {
                        var List     = $(fieldname);
                        var values   = List.getAttribute("md-option[ng-repeat]", "value",true);
                        var Ids      = List.getAttribute("md-option[ng-repeat]", "id",true);


                        var index = values.indexOf(fieldValue);

                        var checkIsEnabled =	browser.getAttribute(fieldname, "disabled");
	

                        if(Ids.length > 1 && checkIsEnabled == null)
                        {
                            try{
                                this.OnlyClickAction(fieldname,1000);
                                    
                            }
                            catch(ex)
                            {
                                console.log("Error: CheckSiteFields: "+ex.message);
                                List.setValue("1");
                                browser.leftClick(List.selector,10,10);


                                var arrowSelektor = '.md-select-icon'; 
                                List = $(arrowSelektor);
                                if(List != null)
                                {
                                    this.OnlyClickAction(arrowSelektor,1000);
                                }
                            }

                            if(index > -1)
                            {
                                this.ClickAction('#'+Ids[index]);
                            }
                            else{
                                this.ClickAction('#'+Ids[0]);   
                            }

                                
                        
                        }
                    }
                    else
                    {
                        if(fieldValue === 'Click')
                        {
                            this.OnlyClickAction(fieldname)
                        }
                        else
                        {
                            if(add != null && add === "true")
                            {
                                this.ClickAction(fieldname);
                                var sel = $(fieldname);
                                sel.addValue(fieldValue);
                            }
                            else
                            {
                                if(fieldname.substr(0,1)==='[')
                                {
                                    browser.click(fieldname);
                                    this.SearchElement(fieldname, fieldValue, 1000, (check!=null && check==="true"));
                                }
                                this.SearchElement(fieldname, fieldValue, 100, (check!=null && check==="true"));
                                
                            }
                        }
                    }                    
                }
              
            });
        }
    }

    SelectHauptAgentur()
    {
		this.WaitUntilVisible(_btnMainAgency);
        this.OnlyClickAction(_btnMainAgency);
        this.WaitUntilVisible(_btnNewVn);
        
    }    


    Next(waitTime=0)
    {
        this.PauseAction(waitTime);
        this.ClickAction(_btnNavNext);
    }

    Prev(waitTime=0)
    {
        this.PauseAction(waitTime);
        this.ClickAction(_btnNavPrev);
    }

    OnlyClickAction(selector, pauseTime=0){
     try{
        
        if(!browser.isExisting(selector))
        {
            return;
        }
		var retValue = $(selector);
        assert.notEqual(retValue.selector,"");

        browser.click(retValue.selector);
        
        if(pauseTime>0)
        {
            this.PauseAction(pauseTime);
        }
        return retValue;
        }catch(ex)
        {
            console.log("Error: OnlyClickError: "+ex.message);
            if(!this.CheckPopUp(retValue.selector))
            {
                throw new Error(ex);
            }
        }
    }

    CheckPopUp(clickSelector)
    {
        var selectorLeaveOrGo = '.swal2-confirm.md-button.md-raised.md-accent';
        if(this.CheckIsVisible(selectorLeaveOrGo))
        {
            browser.click(selectorLeaveOrGo);
            browser.click(retValue.selector);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    ClickAction(selector, waitforVisibleSelector='', timeout=50000, pauseTime=0, click=false){

        if(_ClickIterator >= 100)
        {
            throw new Error("Zu viele ClickAction Iterationen");
        }

        var retValue = $(selector);
        retValue.waitForVisible(timeout);
        retValue.waitForEnabled(timeout);
        try
        {
            browser.click(retValue.selector);

        }catch(ex)
        {
            var conslog = !ex.message.includes('is not clickable at point') && ex.message.includes('obscures it');
            if(conslog)
            {
                console.log("Error: ClickAction: "+ex.message);
            }
            _ClickIterator += 1;

            if(!this.CheckPopUp(retValue.selector))
            {
                throw new Error(ex);
            }
            else
            {
                return;
            }
            
            if(this.CheckIsVisible(_btnTarifSave))
            {
                browser.click(_btnTarifSave);
            }
            else if(this.CheckIsVisible(_btnNavPrev))
            {
                browser.click(_btnNavPrev);
            }
            
            this.ClickAction(selector, waitforVisibleSelector, timeout, pauseTime, click);
        }

        if(waitforVisibleSelector == '#btnFastForward')
        {
            if(!browser.isExisting(waitforVisibleSelector))
            {
                waitforVisibleSelector = _btnNavNext;
            }
        }


        if(waitforVisibleSelector != '')
        {
            browser.waitForVisible(waitforVisibleSelector, timeout);
        }

        if(click)
        {
            this.ClickAction(waitforVisibleSelector);
        }

        this.PauseAction(pauseTime);

        return retValue;
       
	}

    PauseAction(pauseTime){
		if(pauseTime > 0)
			{
				browser.pause(pauseTime);
			}
    }

    GetXmlParser()
    {
        var existsConfigFile = fs.existsSync(this.MainConfigPath);
		assert.equal(existsConfigFile,true);

        var parser = new xml2js.Parser();
        
        return parser;

    }

    SetLeftMenuVisible()
    {

        this.WaitUntilVisible(_leftSiteMenu);

        var checkBlock = $(_leftSiteMenu);

		if(checkBlock.state == 'success')
		{
			if(checkBlock.getAttribute('class').indexOf('navbar-folded') >= 0)
			{
                this.ClickAction(this.MenueMinMax);
            }
		}
    }

    ReadXMLAttribute(standard=false){
        
        var callback = this.CheckFieldListAttribute;
		this.GetXmlParser().parseString(this.Fs.readFileSync(this.MainConfigPath), function(err,result)
		{ 
			if(standard)
			{
                _AllVersicherer = result['Config']['VersichererList'][0].$['all'];
                _BreakAtError = result['Config']['VersichererList'][0].$['breakAtError'];
                _Documents = result['Config']['Tests'][0].$['documents'];
                
                _SmokeTest = result['Config']['VersichererList'][0].$['smoke'];
                _TarifSelector  = result['Config']['SelectorList'][0]['Selector'];
                _Versicherer =  result['Config']['VersichererList'][0]['Versicherer'];
                _ExcludeVersicherer =  callback('Versicherer',result['Config']['ExcludeList'][0]);
                _AllDurchfWege = result['Config']['DurchfwegList'][0].$['all'];
                _DurchfWege =  result['Config']['DurchfwegList'][0]['DurchfWeg'];

                _AllTarife = result['Config']['TarifList'][0].$['all'];
                _Tarife =  result['Config']['TarifList'][0]['Tarif'];
                _TarifSmoke = result['Config']['TarifList'][0].$['smoke'];
                _OnlyTarifCheck = result['Config']['VersichererList'][0].$['onlyTarifCheck'];

                _AllType = result['Config']['TypeList'][0].$['all'];
                _Types =  result['Config']['TypeList'][0]['Type'];
                _TypeSmoke = result['Config']['TypeList'][0].$['smoke'];
            }
		})
    }

    CheckFieldAttribute(attributeName,element)
    {
        var result = null
        try{
            result = element[attributeName][0];

        }
        catch(ex)
        {
        }
        
        return result;
    }

    CheckFieldListAttribute(attributeName,element)
    {
        var result = null
        try{
            result = element[attributeName];

        }
        catch(ex)
        {
            console.log("Error: CheckFieldListAttribute: "+ex.message);
        }
        
        return result;
    }


    GetElementFromConfig(elementArr)
    {
        var res = null;
       
        try{
            this.GetXmlParser().parseString(this.Fs.readFileSync(this.MainConfigPath), function(err,result)
            { 
                    res = result['Config'];
                    elementArr.forEach(function(el) {
                        res = res[el];
                        if(res != null)
                        {
                            res = res[0];
                        }
                    })
            })  
        }catch(ex)
        {
            console.log("Error: GetElementFromConfig: "+ex.message);
        }

        
        return res;
    }    


    
    CheckVersion()
    {
		if(this.Version !== '')
		{
            var t = browser.getText('#container-main');
            console.log(t);
            assert.notEqual(t.indexOf('Version '+this.Version), -1, "Fehlerhafte Version ausgliefert.");
		}		
    }

    CheckText(selector,text)
    {
        var index = -1;
		if(this.SmokeTest && this.Version != '')
		{
            var text = browser.getText(selector);
            if(text != null && text.length > 0)
                index = text.indexOf(text);

        }		
        return index >= 0;
    }    


    ReadXMLFieldValues(xmlFile){
    
        _SiteFields = null;
		this.GetXmlParser().parseString(this.Fs.readFileSync(xmlFile), function(err,result)
		{
			_SiteFields =  result['Config']['Fields'][0]['Field'];
        })
        
        return _SiteFields;

    }

    WaitUntilVisible(waitUntilSelector=_btnNavNext, waitTime=50000, message="")
    {
        this.WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: '+waitUntilSelector+' to be different after: '+waitTime;
        if(message != "")
        {
            _message = message;
        }

        if(this.CheckIsVisible(_btnBlurredOverlay))
        {
            this.OnlyClickAction(_btnBlurredOverlay);
            if(this.CheckIsVisible(_gridSelector))
            {
                this.OnlyClickAction(_gridSelector);
            }
        }
    

        browser.waitUntil(function ()
        {
            return browser.isVisible(_WaitUntilSelector);
          }, waitTime, _message);
    }

    WaitUntilEnabled(waitUntilSelector=_btnNavNext, waitTime=50000, message="")
    {
        this.WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: '+waitUntilSelector+' to be different after: '+waitTime;
        if(message != "")
        {
            _message = message;
        }

        if(this.CheckIsVisible(_btnBlurredOverlay))
        {
            this.OnlyClickAction(_btnBlurredOverlay);
            if(this.CheckIsVisible(_gridSelector))
            {
                this.OnlyClickAction(_gridSelector);
            }
        }
    

        browser.waitUntil(function ()
        {
            return browser.isEnabled(_WaitUntilSelector);

          }, waitTime, _message);

    }    

    WaitUntilExist(waitUntilSelector, waitTime=5000, message="")
    {
        this.WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: '+waitUntilSelector+' to be different after: '+waitTime;
        if(message != "")
        {
            _message = message;
        }

        browser.waitUntil(function ()
        {
            return browser.isExisting(_WaitUntilSelector);

          }, waitTime, _message);

    }  

    WaitUntilTitle(waitTime=5000, message="")
    {
        var _message = 'expected: title to be different after: '+waitTime;
        if(message != "")
        {
            _message = message;
        }

        browser.waitUntil(function ()
        {
            var title = browser.getTitle().includes(' | ');
            return title;

          }, waitTime, _message);

    }    

    WaitUntilSelected(waitUntilSelector=_btnNavNext, waitTime=50000, message="")
    {
        this.WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: '+waitUntilSelector+' to be different after: '+waitTime;
        if(message != "")
        {
            _message = message;
        }

        if(this.CheckIsVisible(_btnBlurredOverlay))
        {
            this.OnlyClickAction(_btnBlurredOverlay);
            if(this.CheckIsVisible(_gridSelector))
            {
                this.OnlyClickAction(_gridSelector);
            }
        }
    

        browser.waitUntil(function ()
        {
            return  browser.isSelected(_WaitUntilSelector)
          }, waitTime, _message);
    }    

    GetNewChapterList(chapter){
        var resultArr = [_NewChapterList.length];
        _NewChapterList.forEach(function(element, index) 
        {
            if(element === '')
            {
                resultArr[index] = chapter;
            }
            else
            {
                resultArr[index] = element;
            }
        });

        return resultArr;
    }

    AddChapter(chapter, btnNew, waitUntilSelector='',callbackFunc=null)
    {
        this.PauseAction(500);
        var Sites = this.GetElementFromConfig(this.GetNewChapterList(chapter));
        var path = Sites.$['path'];
        
        Sites['Site'].forEach(element => {
           
            // if(fs.existsSync(configFileName))
            // {
    
            var url = element['Url'][0];
            var BtnClick = this.CheckFieldAttribute('NewBtn',element);
            if(url == 'new')
            {
                this.WaitUntilVisible(btnNew,10000);
                this.ClickAction(btnNew);
                if(waitUntilSelector !== '')
                {
                    this.WaitUntilVisible(waitUntilSelector);
                }

            }
            else if(BtnClick != null)
            {
                this.Navigate2Site(url);
                this.ClickAction('#'+BtnClick)
            }
            else
            {
                this.Navigate2Site(url);
            }
            var fileName = element['FileName'][0];
            var configFileName = this.ExecutablePath+'test\\config\\sites\\new\\'+path+'\\'+fileName;    
            if(fileName == 'Callback' && callbackFunc != null)
            {
                callbackFunc(element);
            }
            else
            {
                   this.CheckSiteFields(configFileName);
            }
        //}
        // else
        // {
        //     console.log('Config Datei: '+configFileName+' existiert nicht.')
        // }
		});        
    }

    RefreshBrowser(selector=null,click=false)
    {
        browser.refresh();
        if(selector != null)
        {
            this.WaitUntilVisible(selector);
            if(click)
            {
                this.OnlyClickAction(selector);
            }
        }

    }

    LogTime(string='')
    {
        if(string!='')
        {
            console.log(string);
        }
        console.log(date.format(new Date(), 'YYYY/MM/DD HH:mm:ss'));
    }


   

}
module.exports = TestLib;






