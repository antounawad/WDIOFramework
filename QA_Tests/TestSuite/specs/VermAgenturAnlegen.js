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
        login.LoginUser("antoun.awad", "qatestAantoun")
        // Click the ArbG Chapter and click add new ArbG
        testLib.ClickElementByAttribute('title', 'Agenturen');
        testLib.ClickElementByAttribute('href', '/Vermittlerbereich/Agency/Create', '#Address_Company');
        testLib._SetComplexListBoxValue('Antoun Awad (GOT )', '[ng-model="model.HierarchyId"]');
        testLib.SetValue('#Address_Firstname', 'Muster')
        testLib.SetValue('#Address_Lastname', 'Agentur')
        testLib.SetValue('#Address_Email', "agentur@test.agentur.test")
        // testLib.SetValue('#Address_Email', testLib.GenerateRandomEmail('@test.agentur.test'));
        testLib.SetValue('#NameShortCut', 'MA')
        testLib.CompareAndClickIfMatch('class', 'md-raised md-accent md-button md-ink-ripple', 'Speichern')

        var searchAgentur = '#Search';
        testLib.SetValue(searchAgentur, 'Agentur, Muster');
        testLib.ClickElement('[aria-label="Nur aktive"]');
        var searchButton = '//button[text()="Suchen"]';
        testLib.ClickElementSimple(searchButton);
        var searchElement = '//a[text()="Agentur, Muster"]';
        var result = testLib.CheckIsVisible(searchElement, 1000)

        if (result) {
            console.log("+++++++++++ Agentur ist Erfolgreich angelegt +++++++++++")
            testLib.ClickElement('[aria-label="Löschen"]', '[aria-label="Agentur löschen"]')
            testLib.ClickElement('.md-primary.md-confirm-button.md-button.md-ink-ripple.md-default-theme')
            testLib.PauseAction(2000)

        }
        else {
            console.log("+++++++++++ Fehler: Agentur ist nicht gefunden ++++++++++")
            browser.saveScreenshot(ScreenShotFolderpath);

        }
        var x = 9;







    });

});


