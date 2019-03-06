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

describe('webdriver.io page', () => {
    it('should have the right title', () => {


        testLib.InitBrowserStart();


        login.LoginUser("antoun.awad@xbav.de", "qatestAantoun")

        testLib.CheckVersion();
        // var version = testLib.CheckGivenVersion("5.2.22");

        

        vn.SearchVN("AutomRKTestVN", 2000)
        testLib.Navigate2Site("Arbeitnehmer – Auswahl");






    });
});

