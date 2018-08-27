var assert = require('assert');
var fs = require('fs'),
 xml2js=require('xml2js')
 var date = require('date-and-time');
  
 var defaultTimout = 10000;

 var _UrlTimeOut = 999999999999999999999999999999999999999999;

// Mit Dokumentgenerierung oder nicht
var _Documents = false;
var _debug = false;

// Versicher Liste (falls in config angegeben)
var _Versicherer = null;
var _DurchfWege = null;
var _Tarife =  null;
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
var _ExMessage =  [3];
var _ExMessageCnt = 0;
var _TestFolder = null; 
var _TestConfigFolder = null;
var __xpath = null;
var __xpathResult = null;



class TestLib{

    get VnName(){return _VnName};
    get VpName(){return _VpName};

    get IsDebug(){return _debug === 'true'}
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

    get ErrorShotPath(){return this.ExecutablePath+'TestSuite\\'+this.TargetUrl+'\\'+_TestFolder+_TestConfigFolder+'errorShots\\'};

    // Gibt die VersichererList aus Config Datei zurück
    get Versicherer(){ return _Versicherer};

    get ExcludeVersicherer(){return _ExcludeVersicherer};
    
    // FileStream für Config Dateien
    get Fs(){return fs};

    get ArgVOffset(){return 2};

    // Übergebenes Projekt --hotfix aus Args
     get TargetUrl() 
     { 
         var targetArr = String(process.argv[5].substr(2)).split(':');
         _TestFolder = targetArr[1]+'\\';
         _TestConfigFolder = targetArr[2]+'\\';
         return targetArr[0];
    }

    get TestFolder()
    {
        return _TestFolder;
    }

    get TestConfigFolder()
    {
        return _TestConfigFolder;
    }

     get TargetDom() { return process.argv[6].substr(2)}
     

     // Returns Version aus Args
     get Version() 
     {
         let ver = process.argv[7]
         if(ver != null)
         {
             return ver.substr(2);
         }
         return '';
     }

     // Returns Main Config Pfad
     get MainConfigPath() 
     {
         return this.ExecutablePath+'TestSuite\\'+this.TargetUrl+'\\'+_TestFolder+_TestConfigFolder+'Config.xml'
     }

     get CommonConfigPath()
     {
        return this.ExecutablePath+'TestSuite\\common\\Config.xml';
     }

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
    get BtnNavPrev(){return _btnNavPrev};

    get BtnFastForward(){return _btnFastForward};


    get TarifSiteSelector(){return _TarifSiteSelector};


    TakeErrorShot(message)
    {
        // Todo verbessern :-)
        message =  message.replace(':','_');
        message =  message.replace(':','_');
        message =  message.replace(':','_');
        message =  message.replace(':','_');
        message =  message.replace(':','_');
        browser.saveScreenshot(this.ErrorShotPath+message+'.png')
    }


    InitBrowserStart()
    {
        var url = 'http://'+ this.TargetUrl+'.'+this.TargetDom+'.de'+'/Beratung/Account/Login?ReturnUrl=%2FBeratung%2F';
		if(this.TargetUrl == 'beratung')
		{
			url = 'http://beratung.xbav-berater.de/Account/Login?ReturnUrl=%2F';
		}
		browser.url(url);
     
        
        // Erstmal die Standard configuration auslesen
		// Alle Versicherer oder nur spezielle
		// Alle Kombinationen oder nur spezielle oder nur SmokeTest
		// SmokeTest := Nur erste funtkionierende Kombination
		this.ReadXMLAttribute(true);

    }

    SetListBoxValue(selector,value)
    {
        var fieldname = this.GetFieldName(selector);
        
        if(fieldname.includes('['))
        {
            var ex = $(fieldname);
            fieldname = '#'+ex.getAttribute('id');
        }

        var exist = this.WaitUntilExist(fieldname);

        if(!exist)
        {
            throw new Error("Selector not found...");
        }

        var List     = $(fieldname);
        var values   = List.getAttribute("md-option[ng-repeat]", "value",true);
        var Ids      = List.getAttribute("md-option[ng-repeat]", "id",true);

       var index = values.indexOf(value);

        if(Ids.length > 1)
        {
            this.OnlyClickAction(fieldname,1000);
            if(index > -1)
            {
                this.ClickAction('#'+Ids[index]);
            }
            else{
                this.ClickAction('#'+Ids[0]);   
            }
        }
    }


    // Sucht ein Element (Selector) und ruft die Methode zum Setzen eines Values auf
    // Wird der Selector nicht gefunden, wird abgebrochen
    // Wenn ein Pause value übergeben wird, wird Pausiert
    SearchElement(selector,value, pauseTime=0, checkExist=false){
        try
        {
            if(_SearchIterator >= 20)
            {
                _SearchIterator = 0;
                throw new Error("Zu viele SearchElement Iterationen");
            }
            var searchSelector = $(selector)
            assert.notEqual(searchSelector, null)

            var entryValue = null;
           

            if(checkExist)
            {
                entryValue = searchSelector.getValue();
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

    CompareValue(selector,value)
    {
        if(this.WaitUntilVisible(selector))
        {
            var searchSelector = $(selector)  

            var check= searchSelector.getValue()
            if(check != null)
            {
                return searchSelector.getValue() === value; 
            }
            else
            {
              var text =  browser.getText(searchSelector.selector);
              
              return(text.includes(value));
            }
        }
        else
        {
            console.log("Selector: "+selector+" not found.")
            return false;
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

            if(_Navigate2SiteIterator >= 20)
            {
                _Navigate2SiteIterator = 0;
                throw new Error("Zu viele Navigate2Site Iterationen");
            }
            while(true)
            { 

                try{
                   
                    this.WaitUntilTitle();
                    if(this.IsDebug)
                    {
                        console.log(this.BrowserTitle);
                    }
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

        if(String(title).includes('Investmentauswahl'))
        {
            var x = "Y";
        }

        var path = this.ExecutablePath+'TestSuite\\'+this.TargetUrl+'\\'+_TestFolder+_TestConfigFolder+'sites\\mandatory\\'+title+'.xml';

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
            if(_ClearElementIterator >= 20)
            {
                _ClearElementIterator = 0;
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
        if(fieldname.substr(0,1)!='.' && fieldname.substr(0,1)!='[' && !fieldname.includes('Common:'))
        {
            fieldname  = '#'+element;
        }
        
        return fieldname;
    }


    CheckExMessage(message)
    {
        if(_ExMessageCnt > 2)
        {
            this.CleanUpExMessage();
            console.log(message);
            return;
        }

        if(_ExMessageCnt == 0 || _ExMessage.indexOf(message) >= 0)
        {
            _ExMessage[_ExMessageCnt++] = message;
        }
        else if(_ExMessage.indexOf(message) == -1)
        {
            this.CleanUpExMessage();
            this.CheckExMessage(message);
        }
    }

    CleanUpExMessage()
    {
        for(var i=0;i<= 2;i++)
        {
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
    CheckSiteFields(pathFile)
    {
       
        var configFile = this.GetXmlConfigPath(pathFile);

        if(fs.existsSync(configFile))
        {

            var fields = this.ReadXMLFieldValues(configFile);
            for(var element=0;element <= fields.length-1;element++) {

                var __siteFieldName  =  null;
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

                __siteFieldName = this.GetFieldName(fields[element]['Name'][0]);
                if(__siteFieldName.includes("Common:"))
                {
                   __siteFieldFieldNameArr = this.GetCommonConfig(String(__siteFieldName).split(':')[1],false);
                   
                   for(var fna=0;fna<=__siteFieldFieldNameArr.length-1;fna++)
                   {
                    try
                    {
                        var fn = __siteFieldFieldNameArr[fna];
                        if(!String(fn).includes('['))
                        {
                            fn = '#'+fn;
                        }
                        this.WaitUntilExist(fn,1000);
                        __siteFieldName = fn;
                        if(this.IsDebug)
                        {
                            console.log('SiteFieldName: '+__siteFieldName);
                        }
                        break;
                    }
                    catch(ex)
                    {

                    }
                    
                   };

                   if(__siteFieldName.includes("Common:"))
                   {
                        continue;
                   }


                }

                __siteFieldValue = fields[element]['Value'][0];
                if(__siteFieldValue.includes("Common"))
                {
                   __siteFieldFieldValueArr = this.GetCommonConfig(String(__siteFieldValue).split(':')[1]);
                }

                try
                {
                    
                    __siteFieldCheckExist = this.CheckFieldAttribute('CheckExist',fields[element]);

                    if(__siteFieldCheckExist != null && String(__siteFieldCheckExist) !== _CurrentCheckID)
                    {
                        return;
                    }

                    __siteFieldList =  this.CheckFieldAttribute('ListBox',fields[element]);
                    __siteFieldClear = this.CheckFieldAttribute('Clear',fields[element]);
                    __siteFieldCheck = this.CheckFieldAttribute('Check',fields[element]);
                    __siteFieldAdd = this.CheckFieldAttribute('Add', fields[element]);
                    __siteFieldWarning = this.CheckFieldAttribute('WarningField', fields[element]);

                    if(__siteFieldWarning != null)
                    {
                        this.PauseAction(3000);
                        var warningBlock = $(__siteFieldWarning);
                        if(warningBlock != null)
                        {
                            var text = browser.getText(warningBlock.selector);
                            if(text.includes(__siteFieldValue))
                            {
                                var exfield = this.CheckFieldAttribute('ExceptionField', fields[element]);
                                var exValue = this.CheckFieldAttribute('ExceptionValue', fields[element]);
                                if(exfield != null && exValue != null)
                                {
                                    __siteFieldName = this.GetFieldName(exfield);
                                    __siteFieldName = $(__siteFieldName);
                                    browser.click(__siteFieldName.selector);
                                    if(this.WaitUntilEnabled())
                                    {
                                        this.PauseAction(5000);
                                        this.OnlyClickAction(_btnNavNext);
                                    }
                                    continue;
                                }
                            }
                        }
                    }


                    try
                    {
                        this.WaitUntilExist(__siteFieldName,2000);
                        var enabled  = browser.isEnabled(__siteFieldName);
                        if(!enabled)
                        {
                            throw new Error(fieldName+" not enabled");
                        }
                    }
                    catch(ex)
                    {
                        
                        var exfield = this.CheckFieldAttribute('ExceptionField', fields[element]);
                        var exValue = this.CheckFieldAttribute('ExceptionValue', fields[element]);
                        if(exfield != null && exValue != null)
                        {
                            __siteFieldName = this.GetFieldName(exfield);
                            __siteFieldValue = exValue;
                        }
                        else
                        {
                            this.CheckExMessage(ex.message);
                        }

                    }

                    __siteFieldCheck = this.CheckFieldAttribute('Check',fields[element]);
                    __siteFieldAdd = this.CheckFieldAttribute('Add', fields[element]);

                }catch(ex)
                {
                    if(__siteFieldName !== '#Warning')
                    {
                        console.log("Error: CheckSiteFields(WaitUntilExists): "+__siteFieldName+" "+ex.message);
                    }
                    return;
                }
                
                __siteFieldExist = browser.isExisting(__siteFieldName);

                if(__siteFieldExist)
                {

                    this.PauseAction(300);

                    if(__siteFieldList != null && __siteFieldList === "true")
                    {
                        if(__siteFieldName.includes('['))
                        {
                            var ex = $(__siteFieldName);
                            __siteFieldName = '#'+ex.getAttribute('id');
                        }
                
                        var exist = this.WaitUntilExist(__siteFieldName);
                        if(!exist)
                        {
                            break;
                        }



                        var List     = $(__siteFieldName);
                        var values   = List.getAttribute("md-option[ng-repeat]", "value",true);
                        var Ids      = List.getAttribute("md-option[ng-repeat]", "id",true);


                        var index = values.indexOf(__siteFieldValue);

                        var checkIsEnabled =	browser.getAttribute(__siteFieldName, "disabled");
	

                        if(Ids.length > 1 && checkIsEnabled == null)
                        {
                            try{
                                this.OnlyClickAction(__siteFieldName,1000);
                                    
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
                        if(__siteFieldValue === 'Click')
                        {
                            var checkEnableBefore  = this.CheckFieldAttribute('CheckEnableBefore', fields[element]);
                            if(checkEnableBefore != null && !browser.isEnabled(__siteFieldName))
                            {
                                break;
                            }

                            var checkFieldBefore  = this.CheckFieldAttribute('CheckFieldBefore', fields[element]);
                            if(checkFieldBefore != null && browser.isExisting(checkFieldBefore))
                            {
                                break;
                            }



                            var checkBefore  = this.CheckFieldAttribute('CheckBefore', fields[element]);
                            if(checkBefore != null)
                            {
                                var searchSelector = $('#'+checkBefore)
                    
                                var value = searchSelector.getValue();
                               
                                if(value != null)
                                {
                                    this.OnlyClickAction(__siteFieldName)        
                                }
                            }
                            else
                            {
                                this.OnlyClickAction(__siteFieldName)
                            }
                        }
                        else
                        {
                            if(__siteFieldAdd != null && __siteFieldAdd === "true")
                            {
                                this.ClickAction(__siteFieldName);
                                var sel = $(__siteFieldName);
                                sel.addValue(__siteFieldValue);
                            }
                            else
                            {
                                if(__siteFieldName.substr(0,1)==='[')
                                {
                                    browser.click(__siteFieldName);
                                    if(__siteFieldFieldValueArr != null)
                                    {
                                        var sel = $(__siteFieldName);    
                                        for(var fva = 0;fva <= __siteFieldFieldValueArr.length-1;fva++)
                                        {
                                            this.SearchElement(__siteFieldName, __siteFieldFieldValueArr[fva], 300, (__siteFieldCheck!=null && __siteFieldCheck==="true" && fva==0));    
                                            
                                            var ex = $('.ng-scope.md-input-invalid.md-input-has-value');

                                            if(ex.type === 'NoSuchElement')
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        this.SearchElement(__siteFieldName, __siteFieldValue, 1000, (__siteFieldCheck!=null && __siteFieldCheck==="true"));
                                    }
                                }
                                else
                                {
                                    this.SearchElement(__siteFieldName, __siteFieldValue, 100, (__siteFieldCheck!=null && __siteFieldCheck==="true"));
                                }
                                
                            }
                        }
                    }                    
                }
              
            };
        }
    }


    SelectHauptAgentur()
    {
		this.WaitUntilVisible(_btnMainAgency);
        this.OnlyClickAction(_btnMainAgency);
        this.WaitUntilVisible(_btnNewVn);
        
    }    


    Next(waitTime=0,checksitefields=false)
    {
        if(checksitefields)
        {
            this.CheckSiteFields();
        }
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

        if(_ClickIterator >= 20)
        {
            _ClickIterator = 0;
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

    GetXmlParser(path)
    {
        var existsConfigFile = fs.existsSync(path);
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

    LogDebug(logText)
    {
        if(this.IsDebug)
        {
            console.log(logText);
        }
    }

    ReadXMLAttribute(standard=false){
        
        var callback = this.CheckFieldListAttribute;
		this.GetXmlParser(this.MainConfigPath).parseString(this.Fs.readFileSync(this.MainConfigPath), function(err,result)
		{ 
			if(standard)
			{
                try
                {
                    _AllVersicherer = result['Config']['VersichererList'][0].$['all'];
                    _BreakAtError = result['Config']['VersichererList'][0].$['breakAtError'];
                    _Documents = result['Config']['Tests'][0].$['documents'];
                    _debug = result['Config']['Tests'][0].$['debug'];
                    
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
                }catch(ex)
                {
                    
                }
            }
        })

        try{
            var varBaseFile = this.ExecutablePath+'TestSuite\\'+this.TargetUrl+'\\' +_TestFolder+_TestConfigFolder+'sites\\new\\vn\\Stammdaten.xml';    

            var fields = this.ReadXMLFieldValues(varBaseFile);
            _VnName = fields[0]['Value'][0];
            
            varBaseFile = this.ExecutablePath+'TestSuite\\'+this.TargetUrl+'\\' +_TestFolder+_TestConfigFolder+'sites\\new\\vp\\Stammdaten.xml';    

            fields = this.ReadXMLFieldValues(varBaseFile);
            _VpName = fields[0]['Value'][0];
        }
        catch(ex)
        {
            this.LogDebug(ex.message);
        }
    }

    

    _GetCommonConfig(list,value,field)
    {
        
            if(field.Name == __xpath)
            {
                var result = [field[list][0][value].length];
                var count = 0;
                field[list][0][value].forEach(element => {
                    result[count++] = element;
                });
                __xpathResult = result;
            }

        return __xpathResult;
    }    


    GetCommonConfig(xpath,values=true)
    {
        __xpath = xpath;
        __xpathResult = null;
        var varBaseFile = this.CommonConfigPath;
        var fields = this.ReadXMLFieldValues(varBaseFile);
        var list = 'ValueList';
        var value = 'Value';
        if(!values)
        {
            list = 'NameList';
            value = 'Name';
        }
        
        fields.forEach(field =>{
            __xpathResult = this._GetCommonConfig(list,value,field);
        });

        if(__xpathResult == null)
        {
            __xpathResult = [0];
        }
        return __xpathResult;
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
            this.GetXmlParser(this.MainConfigPath).parseString(this.Fs.readFileSync(this.MainConfigPath), function(err,result)
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
		this.GetXmlParser(xmlFile).parseString(this.Fs.readFileSync(xmlFile), function(err,result)
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
    
        var result =  false;

       try
       {
        var result =  browser.waitUntil(function ()
            {
                return browser.isVisible(_WaitUntilSelector);
            }, waitTime, _message);
        }
        catch(ex)
        {
            this.LogDebug(ex.message);
        }
        finally
        {
            return result;
        }
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
    

        var result = false;
        try{

            result =  browser.waitUntil(function ()
            {
                return browser.isEnabled(_WaitUntilSelector);

            }, waitTime, _message);
        }catch(ex)
        {
            result = false;
        }
        finally
        {
            return result;
        }

    }    

    WaitUntilExist(waitUntilSelector, waitTime=5000, message="")
    {
        this.WaitUntilSelector = waitUntilSelector;
        var _message = 'expected: '+waitUntilSelector+' to be different after: '+waitTime;
        if(message != "")
        {
            _message = message;
        }

        var result = false;

        try
        {

        result = browser.waitUntil(function ()
            {
                var res = browser.isExisting(_WaitUntilSelector);
                return res;

            }, waitTime, _message);
        }catch(ex)
        {
            result = false;
        }finally
        {
            return true;
        }
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
            var configFileName = this.ExecutablePath+'TestSuite\\'+this.TargetUrl+'\\' +_TestFolder+_TestConfigFolder+'sites\\new\\'+path+'\\'+fileName;    

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
        let dt = date.format(new Date(), 'YYYY:MM:DD HH:mm:ss').toString();
        dt = dt.replace(' ','__');
        console.log(dt);
        return dt;
    }


   

}
module.exports = TestLib;






