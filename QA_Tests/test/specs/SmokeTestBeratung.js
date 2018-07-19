var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Login = require('../Lib/Login.js')
const login = new Login()
var VP = require('../Lib/VP.js')
const vp = new VP()
var VN = require('../Lib/VN.js')
const vn = new VN()
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation()
var Salary = require('../Lib/Salary.js')
const salary = new Salary()
var Tarif = require('../Lib/Tarif.js')
const tarif = new Tarif()
var RK = require('../Lib/RK.js')
const rk = new RK()




describe('webdriver.io page', function () {




	it('Smoke Test...', function () {


		testLib.ReadXMLAttribute(true);

		browser.url('http://'+ testLib.TargetUrl+'.xbav-berater.de/Beratung/Account/Login?ReturnUrl=%2FBeratung%2F');
		this.timeout(999999999999999999999999999999999999999999);


		

		// Erstmal die Standard configuration auslesen
		// Alle Versicherer oder nur spezielle
		// Alle Kombinationen oder nur spezielle oder nur SmokeTest
		// SmokeTest := Nur erste funtkionierende Kombination
		testLib.ReadXMLAttribute(true);


		


//		testLib.CheckVersion();

		// Login
		// Todo extrahieren
		login.LoginUser();

		
		// Todo Test manuell anlegen und übergeben

		rk.StartRKTest();

	   
	   console.log("Test is ready");

		 
	
	});

});


