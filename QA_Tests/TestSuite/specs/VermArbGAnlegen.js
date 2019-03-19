var TestLib = require('C:/git/shared/QA_Tests/TestSuiteLib/ClassLib.js')
const testLib = new TestLib();
var Login = require('C:/git/shared/QA_Tests/TestSuiteLib/Login.js')
const login = new Login()
var VM = require('C:/git/shared/QA_Tests/TestSuiteLib/VM.js')
const vm = new VM()
var ScreenShotFolderpath = './Temp/screenshot.png';


describe('webdriver.io page', () => {
    it('should have the right title', () => {


        // try
        // {
        //     testLib.InitBrowserStart(login,true);
        //     vm.AddArbeitgeber();
        // }
        // catch(exception)
        // {
        //     var ex = exception;
        // }


        browser.url('https://automatictest.xbav-berater.de/Vermittlerbereich/Account/Login?ReturnUrl=%2FVermittlerbereich%2F');
        // Click the ArbG Chapter and click add new ArbG
        login.LoginUser("antoun.awad", "qatestAantoun")
        testLib.ClickElementByAttribute('title', 'Arbeitgeber');
        testLib.ClickElementByAttribute('class', 'md-button-small md-accent md-button md-ink-ripple');
        testLib._SetComplexListBoxValue('antoun, awad', '[ng-model="selectedAgency"]')
        testLib.ClickElementSimple('#newVnDialog_btnAnlegen')

        // // // Fill in the ArbG data 
        testLib.ClickElement("#Stammdaten_Name")
        testLib.SetValue("#Stammdaten_Name", 'FirstTest');
        testLib.ClickElementSimple("#Stammdaten_MehrereBetriebsstaetten");
        // xpath on text() Click tab BS and add a new BS
        var tabClick = '//md-tab-item[text()="Betriebsstätten"]';
        testLib.ClickElement(tabClick, "#btnNewFacility")
        var bsName = '#Betriebsstaetten_SelectedItem_Name';
        testLib.ClickElement("#btnNewFacility", bsName);
        testLib.SetValue(bsName, 'MainBS')
        var bsStreet = '#Betriebsstaetten_SelectedItem_Street';
        testLib.SetValue(bsStreet, 'Street');
        var bsPLZ = '#FacilityPlz';
        testLib.SetValue(bsPLZ, '66111');
        var bsSave = '#modalEditFacility_btnSpeichern';
        testLib.ClickElement(bsSave);
        //Tab Zahlungsart 
        // TO-DO HAPE , WHY?
        testLib.PauseAction(1000);
        testLib.ClickElement('//md-tab-item[text()="Zahlungsart / GwG"]', '#Iban');
        var paymentMethod = '#Kontodaten_PaymentMethod';
        testLib._SetComplexListBoxValue('value="1"', paymentMethod);
        testLib.ClickElementSimple('#Kontodaten_HandeltAufEigeneRechnung');
        testLib.ClickElementByAttribute("ng-click", "saveVn($event)", '#Search')//  $('//button[text()="Speichern"]');
        // Check if the ArbG is created and exsits in the list 
        var searchArbG = '#Search';
        testLib.SetValue(searchArbG, 'FirstTest');
        var searchButton = '//button[text()="Suchen"]';
        testLib.ClickElementSimple(searchButton)
        var searchElement = '//a[text()="FirstTest"]';
        var result = testLib.CheckIsVisible(searchElement)

        if (result) {
            console.log("+++++++++++ ArbG ist Erfolgreich angelegt +++++++++++")
            testLib.ClickElement('[aria-label="Löschen"]', '[aria-label="ArbG löschen"]')
            testLib.ClickElement('.md-primary.md-confirm-button.md-button.md-ink-ripple.md-default-theme')
            testLib.PauseAction(2000)

        }
        else {
            console.log("+++++++++++ Fehler: ArbG ist nicht gefunden ++++++++++")
            browser.saveScreenshot(ScreenShotFolderpath);

        }


        // testLib.ClickElementByAttribute('aria-controls', 'tab-content-264');
        // var tabClick = $("//*[contains(text(),'Betriebsstätten')]/../md-icon");


        // testLib.PauseAction(2000);







    });

});


