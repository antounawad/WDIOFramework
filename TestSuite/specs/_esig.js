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

        testLib.InitBrowserStart(true);

        login.LoginUser("antoun.awad", "qatestAantoun")

        testLib.SelectHauptAgentur();

        vn.SearchVN("Edocbox", 2000);

        testLib.Navigate2Site('Arbeitgeber – Stammdaten')

        testLib._test("name","Name");

        testLib.Navigate2Site("Arbeitnehmer – Auswahl");

        vp.SearchVP("Nachname, VPEdocBox", 2000);

        testLib.Navigate2Site("Beratungsübersicht");
        testLib.PauseAction(500);
        testLib.ClickElement('#btnFastForward');
        testLib.PauseAction(2000);
        // testLib.GetAttributeValueOfSelector('tabindex', -1);

        testLib.ClickElementByAttribute('class', 'md-tab');
        // testLib.GetInnerTextByAttribute('class', 'md-tab');
        testLib.CompareAndClickIfMatch('ng-click', "setOrder('AngelegtAm')", 'Angelegt');


        // testLib.Navigate2Site("Arbeitnehmer – Auswahl");

        // vp.SearchVP("Nachname, VPEdocBox", 2000);

        // consultation.AddConsultation();


        // //testLib.TempSelector(null);

        // //testLib.PauseAction(500);

        // testLib.Navigate2Site("Angebot – Kurzübersicht");

        // //testLib.PauseAction(500);

        // testLib.Navigate2Site("Abschluss - Status");

        // testLib.CheckIsVisible('#id_4');
        // testLib.PauseAction(2000);
        // testLib.ClickElement('#id_4');

        // var checked = browser.getAttribute(_docSelectAll, 'checked');
        // // var checked = testLib.GetAttributeValue(_docSelectAll, 'checked');
        // if (checked > 0) {
        //     testLib.ClickElement(_docSelectAll)
        // }
        // testLib.ClickElement('#modalGenerateDocuments_btnWeiter');
        // testLib.PauseAction(3000);
        // testLib.ClickElement('[ng-hide="startESignatureSuccess"]');
        // testLib.PauseAction(3000);
        // browser.switchTab();
        // testLib.PauseAction(3000);
        // testLib.ClickElement('#modalGenerateDocuments_btnArbN-Auswahl');
        // testLib.PauseAction(3000);
        // testLib.Navigate2Site("Beratungsübersicht");
        // testLib.ClickElement('#btnFastForward');
        // testLib.PauseAction(2000);

        // var eSigTab = $('#modalConsultationDetails');
        // eSigTab.getText(eSigTab);
        // console.log(eSigTab);
        // var checked = testLib.GetAttributeValue(_docSelectAll, 'checked');
        // if (testLib.CheckText(eSigTab, 'Elektronische Signatur')) {
        //     console.log("eSig is Successfull ");
        //     testLib.PauseAction(2000);

        // }

        // else {
        //     console.log("eSig is not Successfully closed");
        // }




    });

});


