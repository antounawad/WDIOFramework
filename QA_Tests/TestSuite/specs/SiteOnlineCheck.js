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


        browser.url('https://automatictest.xbav-berater.de/Vermittlerbereich/Account/Login?ReturnUrl=%2FVermittlerbereich%2F');
        login.LoginUser("antoun.awad@xbav.de", "qatestAantoun")

        var version = testLib.CheckVersion();
        // var version = testLib.CheckGivenVersion("5.2.22");
        if (version === false) {
            console.log("Fehlerhafte Version ausgliefert");
        }
        if (version === true) {
            console.log("Version Match , good job ");
        }
    });
});

