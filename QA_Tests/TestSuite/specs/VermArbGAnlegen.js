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


describe('webdriver.io page', () => {
    it('should have the right title', () => {


        browser.url('https://automatictest.xbav-berater.de/Vermittlerbereich/Account/Login?ReturnUrl=%2FVermittlerbereich%2F');
        login.LoginUser("antoun.awad@xbav.de", "qatestAantoun")




        // Click the ArbG Chapter and click add new ArbG
        testLib.ClickElementByAttribute('title', 'Arbeitgeber');
        testLib.ClickElementByAttribute('class', 'md-button-small md-accent md-button md-ink-ripple');
        // Fill in the ArbG data 
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

        //testLib.ClickElement('//md-content[text()="Lastschrift');
        var paymentMethod = '#Kontodaten_PaymentMethod';
        testLib.ClickElement(paymentMethod);
        var clickTransfer = $('#select_option_139');
        clickTransfer.click();

        testLib.ClickElementSimple('#Kontodaten_HandeltAufEigeneRechnung');

        //testLib.SetValue(paymentMethod, 'Lastschrift')
        //HaPe prüfen und ClassLib ergänzen testLib.SetSimpleListBoxValue(paymentMethod, '2')
        //var clickTransfer = $('//md-option[text()="Überweisung"]');
        //clickTransfer.click();
        testLib.ClickElementByAttribute("ng-click", "saveVn($event)", '#Search')//  $('//button[text()="Speichern"]');

        // Check if the ArbG is created and exsits in the list 
        var searchArbG = '#Search';
        testLib.SetValue(searchArbG, 'FirstTest');

        var searchButton = '//button[text()="Suchen"]';
        testLib.ClickElementSimple(searchButton);



        var searchElement = '//a[text()="FirstTest"]';
        var result = testLib.CheckIsVisible(searchElement)

        if (result) {
            console.log("FirstTest is there!!")
        }
        else {
            console.log("Opppss FirstTest is not there!!!")
        }

        // testLib.ClickElementByAttribute('aria-controls', 'tab-content-264');
        // var tabClick = $("//*[contains(text(),'Betriebsstätten')]/../md-icon");


        // testLib.PauseAction(2000);







    });

});


