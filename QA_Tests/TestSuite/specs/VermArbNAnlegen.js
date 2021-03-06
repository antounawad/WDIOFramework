var TestLib = require('C:/git/shared/QA_Tests/TestSuiteLib/ClassLib.js')
const testLib = new TestLib();
const assert = require('assert');
var Login = require('C:/git/shared/QA_Tests/TestSuiteLib/Login.js')
const login = new Login()
var Consultation = require('C:/git/shared/QA_Tests/TestSuiteLib/Consultation.js')
const consultation = new Consultation()
var VP = require('C:/git/shared/QA_Tests/TestSuiteLib/VP.js')
const vp = new VP()
var VN = require('C:/git/shared/QA_Tests/TestSuiteLib/VN.js')
const vn = new VN()
var RK = require('C:/git/shared/QA_Tests/TestSuiteLib/RK.js')
const rk = new RK()
var _docSelectAll = '#consultationDocumentPagesSelectAll';
var ScreenShotFolderpath = './Temp/screenshot.png';

describe('webdriver.io page', () => {
    it('should have the right title', () => {


        browser.url('https://automatictest.xbav-berater.de/Vermittlerbereich/Account/Login?ReturnUrl=%2FVermittlerbereich%2F');
        login.LoginUser("antoun.awad@xbav.de", "qatestAantoun")

        // Click the ArbG Chapter and click add new ArbG
        testLib.ClickElementByAttribute('title', 'Arbeitnehmer');
        testLib.ClickElementByAttribute('ng-click', 'addNewVp()');
        testLib._SetComplexListBoxValue('Antoun  Co. xxxxxxx  xxxxxxxx', '[ng-model="newVp.Vn"]');
        testLib.ClickElementSimple('#newVpDialog_btnAnlegen', 1000)
        testLib.ClickElementSimple('#Stammdaten_Title', 1000)
        testLib.CompareAndClickIfMatch('ng-repeat', 'item in AvailableTitles', 'Herr')
        testLib.PauseAction(1000);
        testLib.SetValue('#Stammdaten_Firstname', 'Muster')
        testLib.SetValue('#Stammdaten_Lastname', 'ArbN')
        testLib.SetValue('#Stammdaten_Street', 'Muster Straße')
        testLib.SetValue('#Zip', '66123', 1000)
        // testLib.SetValue('#Stammdaten_City', 'Saarbrücken')
        testLib.SetValue('[class="md-datepicker-input md-input"]', '01.01.2000', 1000)
        testLib.CompareAndClickIfMatch('class', 'md-raised md-accent md-button md-ink-ripple', 'Speichern')

        var searchArbN = '#Search';
        testLib.SetValue(searchArbN, 'ArbN, Muster');

        var searchButton = '//button[text()="Suchen"]';
        testLib.ClickElementSimple(searchButton);
        var searchElement = '//a[text()="ArbN, Muster"]';
        var result = testLib.CheckIsVisible(searchElement, 1000)

        if (result) {
            console.log("+++++++++++ ArbN ist Erfolgreich angelegt +++++++++++")
        }
        else {
            console.log("+++++++++++ Fehler: ArbN konnte nicht angelegt werden!! ArbN ist nicht gefunden ++++++++++")
            browser.saveScreenshot(ScreenShotFolderpath);

        }
        var x = 9;







    });

});


