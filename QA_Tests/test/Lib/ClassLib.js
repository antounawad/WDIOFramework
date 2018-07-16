var assert = require('assert');
var fs = require('fs'),
 xml2js=require('xml2js')

var defaultTimout = 10000;

// Versicher Liste (falls in config angegeben)
var _Versicherer = null;
// Speichert die TarifSelektoren
var _TarifSelector = null;
// Alle Versicherer oder nur bestimmte
var _AllVersicherer = false;
// Smoke Test Ja oder Nein
var _SmokeTest = false;
// Temporär zum Auslesen der allgemein gültigen Config Values
var _Common = false;
// Liefert die Felder in Site Config
var _SiteFields = null;
// Config Path
var _executablePath = "C:\\Git\\Shared\\QA_Tests\\";
// Helper um Fehler bei den Iterationen abzufangen
var _Navigate2SiteIterator = 0;

var _ClickIterator = 0;


class TestLib{

    //Wegen Config Dateien.
    get ExecutablePath(){ return _executablePath};

    // Gibt die VersichererList aus Config Datei zurück
    get Versicherer(){ return _Versicherer};
    
    // FileStream für Config Dateien
    get Fs(){return fs};

    // Übergebenes Projekt --hotfix aus Args
     get TargetUrl() {return process.argv[5].substr(2)}

     // Returns Version aus Args
     get Version() 
     {
         var ver = process.argv[6]
         if(ver != '')
         {
             return ver.substr(2);
         }

     }

     // Returns Main Config Pfad
     get MainConfigPath() {return this.ExecutablePath+'test\\config\\'+this.TargetUrl+'\\Config.xml'}

     // Einheitliche Rückgabe des Titels
     get BrowserTitle() {return browser.getTitle()}

    // Returns Versicher Liste aus Config falls angegeben
    get Versicherer() {return _Versicherer}

    // Returns Schlter ob alle Versicherer geprüft werden oder nur die aus der List
    // Wandelt um on Boolean
    get AllVersicherer() {return _AllVersicherer == 'true'}

    // Returns Smoke Test ja oder nein
    // Wandelt um in Boolean
    get SmokeTest() {return _SmokeTest == 'true'}

    // Returns TarifSelektoren aus Config
    get TarifSelectoren(){return _TarifSelector}

    // Loggt den Browser Title und prüft, falls assertString nicht leer ist
    ShowBrowserTitle(assertString='')
    {
        console.log("Broser Title: "+this.BrowserTitle)
        if(assertString != '')
        {
            assert.equal(this.BrowserTitle, assertString);
        }

    }

    // Sucht ein Element (Selector) und ruft die Methode zum Setzen eines Values auf
    // Wird der Selector nicht gefunden, wird abgebrochen
    // Wenn ein Pause value übergeben wird, wird Pausiert
    SearchElement(selector,value, pauseTime=0){
		var searchSelector = browser.element(selector)
		assert.notEqual(searchSelector, null)
		searchSelector.setValue(value)
		this.PauseAction(pauseTime)
    }

    // Navigiert zur Seite des Übergebenen Seitentitels
    Navigate2Site(title)
    {
        try{
            if(_Navigate2SiteIterator >= 100)
            {
                throw new Error("Zu viele Navigate2Site Iterationen");
            }
            while(true)
            {
                this.ClickAction('#btnNavNext');
                if(_Navigate2SiteIterator == 0)
                {
                    this.PauseAction(500);
                }
                else
                {
                    this.PauseAction(1500);
                }
                this.CheckSiteFields();
                var index = this.BrowserTitle.indexOf(title);
                if(index > -1 )
                {
                    _Navigate2SiteIterator = 0;
                    break;
                }
    
            }
        }catch(err){
            console.log(err)
            _Navigate2SiteIterator += 1;
            this.Navigate2Site(title);
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

        if(title.indexOf('Stammdaten') >= 0)
        {
            var t = "asdfaf";
        }


        var path = this.ExecutablePath+'test\\config\\sites\\'+title+'.xml';

        if(pathFile != null)
        {
            path = pathFile;
        }
        return path;
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
                var fieldname  = element['Name'][0];
                if(fieldname.substr(0,1)!='.')
                {
                    fieldname  = '#'+element['Name'][0];
                }
                var fieldValue = element['Value'][0];
                var list = element['ListBox'][0];
                var exist = browser.isExisting(fieldname);
                
                if(exist)
                {
                    this.PauseAction(1000);
                    if(list != null && list == "true")
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
                        if(fieldValue == 'Click')
                        {
                            this.OnlyClickAction(fieldname)
                        }
                        else
                        {
                            this.SearchElement(fieldname, fieldValue);
                        }
                    }                    
                }
              
            });
        }
    }


    OnlyClickAction(selector, pauseTime=0){
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

    ReadXMLAttribute(standard){
	
		_Common = standard;

		this.GetXmlParser().parseString(this.Fs.readFileSync(this.MainConfigPath), function(err,result)
		{
			if(_Common)
			{
				_AllVersicherer = result['Config']['VersichererList'][0].$['all'];
                _SmokeTest = result['Config']['VersichererList'][0].$['smoke'];
                _TarifSelector  = result['Config']['SelectorList'][0]['Selector'];
				if(_AllVersicherer == "false")
				{
					_Versicherer =  result['Config']['VersichererList'][0]['Versicherer'];
				}
            }
		})

		_Common = false;
    }
    
    CheckVersion()
    {
		if(this.SmokeTest && this.Version != '')
		{
			assert.notEqual(browser.getText('#container-main').indexOf('Version '+this.Version), -1, "Fehlerhafte Version ausgliefert.");
		}		
    }


    ReadXMLFieldValues(xmlFile){
	
		_SiteFields = null;

		this.GetXmlParser().parseString(this.Fs.readFileSync(xmlFile), function(err,result)
		{
			_SiteFields = result['Config']['Fields'][0]['Field'];
		})

		return _SiteFields;
    }
}
module.exports = TestLib;






