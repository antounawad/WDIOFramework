var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Login = require('../Lib/Login.js')
const login = new Login()
var RK = require('../Lib/RK.js')
const rk = new RK()
var TestClass = require('../Lib/TestClass.js');
const testclass = new TestClass();




describe('webdriver.io page', function () {

	it('Smoke Test...', function () {

		browser.url('http://'+ testLib.TargetUrl+'.xbav-berater.de/Beratung/Account/Login?ReturnUrl=%2FBeratung%2F');
    
        this.timeout(999999999999999999999999999999999999999999);

		// Erstmal die Standard configuration auslesen
		// Alle Versicherer oder nur spezielle
		// Alle Kombinationen oder nur spezielle oder nur SmokeTest
		// SmokeTest := Nur erste funtkionierende Kombination
		testLib.ReadXMLAttribute(true);

		// Login
		// Todo extrahieren
        login.LoginUser();

        testclass.TestMethode();
        

	   console.log("Test is ready");
	});

});


