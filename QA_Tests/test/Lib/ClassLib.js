var assert = require('assert');
var fs = require('fs'),
 xml2js=require('xml2js')
  
 var defaultTimout = 10000;

 var _UrlTimeOut = 999999999999999999999999999999999999999999;

// Mit Dokumentgenerierung oder nicht
var _Documents = false;
// Versicher Liste (falls in config angegeben)
var _Versicherer = null;
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

var _BreakAtError = false;

var _TarifSiteSelector = 'Arbeitgeber – Tarifvorgabe';

var _MenueMinMax = '.fold-toggle.hide.show-gt-sm.md-font.mdi.mdi-24px.mdi-backburger';

var _btnNavNext = '#btnNavNext';
var _btnNavPrev = '#btnNavBack';

var _btnFastForward = '#btnFastForward';

var _NewChapterList = ['New','Chapter','','Sites'];

var _btnBlurredOverlay = '#btnBlurredOverlay';

var _gridSelector = '#tableList';

var _btnMainAgency = '#btnXbavMainAgency';

var _btnNewVn = '#btnNewVn';


class TestLib{

   get UrlTimeOut(){return _UrlTimeOut};

   get BtnMainAgency(){return _btnMainAgency};

   get BreakAtError(){return _BreakAtError === 'true'};

    get BtnBlurredOverlay(){return _btnBlurredOverlay};
    //Wegen Config Dateien.
    get ExecutablePath(){ return _executablePath};

    // Gibt die VersichererList aus Config Datei zurück
    get Versicherer(){ return _Versicherer};

    get ExcludeVersicherer(){return _ExcludeVersicherer};
    
    // FileStream für Config Dateien
    get Fs(){return fs};

    // Übergebenes Projekt --hotfix aus Args
     //get TargetUrl() {return process.argv[3].substr(2)}
     get TargetUrl() { return process.argv[5].substr(2)}

     // Returns Version aus Args
     get Version() 
     {
         //let ver = process.argv[4]
         
         let ver = process.argv[6]
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
        }catch(ex)
        {
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
    }

    // Navigiert zur Seite des Übergebenen Seitentitels
    Navigate2Site(title)
    {
        try{
            if(this.BrowserTitle.indexOf(title) >= 0)
            {
                _Navigate2SiteIterator = 0;
                return;
            }

            if(_Navigate2SiteIterator >= 100)
            {
                throw new Error("Zu viele Navigate2Site Iterationen");
            }
            while(true)
            {
                this.ClickAction('#btnNavNext');

                var index = this.BrowserTitle.indexOf(title);
                if(index > -1 )
                {
                    _Navigate2SiteIterator = 0;
                    this.PauseAction(500);
                    break;
                }

                this.WaitUntilVisible(this.BtnNavNext);
               
                this.CheckSiteFields();
            }
        }catch(err){
            console.log(err)
            _Navigate2SiteIterator += 1;
            this.Navigate2Site(title);
        }finally
        {
            this.CheckSiteFields();
        }
    }

    GetXmlConfigPath(pathFile)
    {
        var title = this.BrowserTitle;
        var index = title.indexOf('|');
        if(index >  0)
        {
            title = title.substr(0,index-1);
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
            _ClearElementIterator += 1;

        }

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

                fieldname  = element['Name'][0];
                if(fieldname.substr(0,1)!='.')
                {
                    fieldname  = '#'+element['Name'][0];
                }
                fieldValue = element['Value'][0];
                exist = browser.isExisting(fieldname);

                if(fieldname == '#AgencyNumber')
                {
                    var x = "eins";
                }
                list =  this.CheckFieldAttribute('ListBox',element);
                clear = this.CheckFieldAttribute('Clear',element);
                check = this.CheckFieldAttribute('Check',element);
                add = this.CheckFieldAttribute('Add', element);
                
                if(exist)
                {

                    this.PauseAction(1000);

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
        
                                this.SearchElement(fieldname, fieldValue, 0, (check!=null && check==="true"));
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
            _ClickIterator += 1;
            browser.click('btnNavBack');
            this.ClickAction(selector, waitforVisibleSelector, timeout, pauseTime, click);
        }

        if(waitforVisibleSelector == '#btnFastForward')
        {
            if(!browser.isExisting(waitforVisibleSelector))
            {
                waitforVisibleSelector = '#btnNavNext';
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
            }
		})
    }

    CheckFieldAttribute(attributeName,element)
    {
        var result = null
        try{
            result = element[attributeName][0];

        }
        catch(ex){}
        
        return result;
    }

    CheckFieldListAttribute(attributeName,element)
    {
        var result = null
        try{
            result = element[attributeName];

        }
        catch(ex){}
        
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
        }catch(ex){}

        
        return res;
    }    


    
    CheckVersion()
    {
		if(this.SmokeTest && this.Version !== '')
		{
            assert.notEqual(browser.getText('#container-main').indexOf('Version '+this.Version), -1, "Fehlerhafte Version ausgliefert.");
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
            return  browser.isVisible(_WaitUntilSelector)
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


   

}
module.exports = TestLib;






