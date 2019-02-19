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
        testLib.PauseAction(2000);
        // Click the ArbG Chapter and click add new ArbG
        testLib.ClickElementByAttribute('title', 'Arbeitgeber');
        testLib.ClickElementByAttribute('class', 'md-button-small md-accent md-button md-ink-ripple');
        // Fill in the ArbG data 
        testLib.ClickElementSimple("#Stammdaten_Name")
        testLib.SetValue("#Stammdaten_Name", 'FirstTest');
        testLib.PauseAction(2000);
        testLib.ClickElementSimple("#Stammdaten_MehrereBetriebsstaetten");
        testLib.PauseAction(1000);
        // xpath on text() Click tab BS and add a new BS
        var tabClick = $('//md-tab-item[text()="Betriebsstätten"]');
        tabClick.click();
        testLib.PauseAction(1000);
        testLib.ClickElementSimple("#btnNewFacility");
        testLib.PauseAction(2000);
        var bsName = $('#Betriebsstaetten_SelectedItem_Name');
        bsName.click();
        bsName.setValue('MainBS');
        var bsStreet = $('#Betriebsstaetten_SelectedItem_Street');
        bsStreet.click();
        bsStreet.setValue('Street');
        testLib.PauseAction(2000);
        var bsPLZ = $('#FacilityPlz');
        bsPLZ.click();
        bsPLZ.setValue('66111');
        testLib.PauseAction(2000);
        var bsSave = $('#modalEditFacility_btnSpeichern');
        bsSave.click();
        testLib.PauseAction(2000);
        //Tab Zahlungsart 
        var tabClick = $('//md-tab-item[text()="Zahlungsart / GwG"]');
        tabClick.click();
        var paymentMethod = $('#Kontodaten_PaymentMethod');
        paymentMethod.click();
        var clickTransfer = $('#select_option_139');
        //var clickTransfer = $('//md-option[text()="Überweisung"]');
        clickTransfer.click();
        var moneyLaundry = $('#Kontodaten_HandeltAufEigeneRechnung');
        moneyLaundry.click();
        testLib.ClickElementByAttribute("ng-click", "saveVn($event)")//  $('//button[text()="Speichern"]');

        // Check if the ArbG is created and exsits in the list 
        testLib.PauseAction(1000);
        var searchArbG = $('#Search');
        searchArbG.click();
        searchArbG.setValue('FirstTest');
        var tabClick = $('//button[text()="Suchen"]');
        tabClick.click();
        var tabClick = $('//a[text()="FirstTest"]');
        if (tabClick.isDisplayed()) {
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


