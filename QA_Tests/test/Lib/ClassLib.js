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


class TestLib{

    //Wegen Config Dateien.
    get ExecutablePath(){ return _executablePath};

    // Gibt die VersichererList aus Config Datei zurück
    get Versicherer(){ return _Versicherer};
    
    // FileStream für Config Dateien
    get Fs(){return fs};

    // Übergebenes Projekt --hotfix
     get TargetUrl() {return process.argv[5].substr(2)}

     get version() 
     {
         var ver = process.argv[6]
         if(ver != '')
         {
             return ver.substr(2);
         }

     }

     get configUrl() {return this.ExecutablePath+'test\\config\\'+this.TargetUrl+'\\Config.xml'}


    get browserTitle() {return browser.getTitle()}

    get Versicherer() {return _Versicherer}
    get AllVersicherer() {return _AllVersicherer == 'true'}
    get SmokeTest() {return _SmokeTest == 'true'}
    get TarifSelectoren(){return _TarifSelector}


    ShowBrowserTitle(assertString='')
    {
        console.log("Browser title: ")
        console.log(this.browserTitle)
        if(assertString != '')
        {
            assert.equal(this.browserTitle, assertString);
        }

    }

    SearchElement(selector,value, pauseTime=0){
		var searchSelector = browser.element(selector)
		assert.notEqual(searchSelector, null)
		searchSelector.setValue(value)
		this.PauseAction(pauseTime)
    }

    Navigate2Selector(selector)
    {
        var test = null;
        try{
            while(true)
            {
                //browser.scroll(0,800);
                this.ClickAction('#btnNavNext');
                this.PauseAction(1500);
                this.CheckSiteFields();
                var title = this.browserTitle;
                var index = title.indexOf(selector);
                if(index > -1 )
                {
                    break;
                }
    
            }
        }catch(err){
            console.log(err)
            test = true;
        }

        if(test)
        {
            while(true)
            {
                //browser.scroll(0,800);
                this.ClickAction('#btnNavNext');
                this.PauseAction(1500);
                this.CheckSiteFields();
                var title = this.browserTitle;
                var index = title.indexOf(selector);
                if(index > -1 )
                {
                    break;
                }
    
            }
        }

        

    }

    CheckSiteFields(pathFile)
    {
        var title = this.browserTitle;
        var index = title.indexOf('|');
        if(index >  0)
        {
            title = title.substr(0,index-1);
        }
        var path = this.ExecutablePath+'test\\config\\sites\\'+title+'.xml';
        if(pathFile != null)
        {
            path = pathFile;
        }
        
        if(title.indexOf('Stammdaten') >= 0)
        {
            var t = "asdfaf";
        }

        if(fs.existsSync(path))
        {
            var fields = this.ReadXMLFieldValues(path);
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
        var retValue = $(selector);
        retValue.waitForVisible(timeout);
        retValue.waitForEnabled(timeout);
        var ex = false;
        try
        {
            browser.click(retValue.selector);
        }catch(ex)
        {
            ex = true;
            browser.click('btnNavBack');

        }

        if(ex)
        {
            ex = false;
            try
            {
                browser.click(retValue.selector);
            }catch(ex)
            {
                ex = true;
            }            
        }

        if(ex)
        {
            ex = false;
            try
            {
                browser.click(retValue.selector);
            }catch(ex)
            {
                ex = true;
            }            
        }        
        console.log(browser.getTitle());
        
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

    getAttributes(selector, value, show=false)
    {
       var attributes =  browser.getAttribute(selector, value);
        if(show)
        {
            console.log(attributes);
        }
        return attributes;
    }

    checkAttribute(selector, value)
    {
        var attribute = browser.getAttribute(selector, value)
        return attribute != null;
    }


    WaitUntil(selector)
    {
       browser.waitUntil(function () {
            try {
                return !!body.element(selector).value;
            } catch (e) {
                return false;
            }
        });
    }

    CheckResult(pauseTime)
    {
		if(pauseTime > 0)
			{
				browser.pause(pauseTime);
			}
    }
    
    GetSelector(selector, withTitle=false, waitTime=0)
    {
        var sel =  $(selector);
        assert.notEqual(sel.selector,"");
        if(withTitle)
        {
            sel.getTitle();
        }

        if(waitTime > 0)
        {
            browser.waitForEnabled(sel.selector, waitTime * 1000);
        }


        return sel;
    }

    ReadXML(channel){
		
		console.log(process.cwd());

        var existsConfigFile = fs.existsSync(this.ExecutablePath+'test\\config\\'+channel+'Config.xml');
		assert.equal(existsConfigFile,true);

		var parser = new xml2js.Parser();
		var data = fs.readFileSync(this.ExecutablePath+'test\\config\\Config.xml');


			parser.parseString(data, function(err,result)
			{
				var tarife = result['Config']['Tariffs'];
				for (var i = 0; i < tarife.length; i++) {
					console.log(tarife[i].$.Vr)
					var tarif = tarife[i]['Tariff'];
					for (var j = 0; j < tarif.length; j++) {
						console.log(tarif[i]["EnumValue"])
					}
				}
			})
    }

    GetXmlParser()
    {
        var existsConfigFile = fs.existsSync(this.configUrl);
		assert.equal(existsConfigFile,true);

        var parser = new xml2js.Parser();
        
        return parser;

    }


    ReadXMLAttribute(standard){
	
		_Common = standard;

		this.GetXmlParser().parseString(this.Fs.readFileSync(this.configUrl), function(err,result)
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
		if(this.SmokeTest && this.version != '')
		{
			assert.notEqual(browser.getText('#container-main').indexOf('Version '+this.version), -1, "Fehlerhafte Version ausgliefert.");
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



    // GetSelector(selector, withTitle=false, waitTime=0)
    // {
    //     var retSelector = $(selector);
    //     assert.notEqual(retSelector,"");

    //     if(withTitle)
    //     {
    //         retSelector.getTitle();
    //     }

    //     return $(retSelector);
    // }



}
module.exports = TestLib;






