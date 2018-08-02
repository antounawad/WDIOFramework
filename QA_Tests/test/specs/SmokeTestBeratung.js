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
var VM = require('../Lib/VM.js')
const vm = new VM();





describe('webdriver.io page', function () {




	it('Smoke Test...', function () {


		var url = 'http://'+ testLib.TargetUrl+'.'+testLib.TargetDom+'.de'+'/Beratung/Account/Login?ReturnUrl=%2FBeratung%2F';
		if(testLib.TargetUrl == 'beratung')
		{
			url = 'http://beratung.xbav-berater.de/Account/Login?ReturnUrl=%2F';
		}
		browser.url(url);
		this.timeout(testLib.UrlTimeOut);

		// Erstmal die Standard configuration auslesen
		// Alle Versicherer oder nur spezielle
		// Alle Kombinationen oder nur spezielle oder nur SmokeTest
		// SmokeTest := Nur erste funtkionierende Kombination
		testLib.ReadXMLAttribute(true);
 
		// Login
		// Todo extrahieren
		login.LoginUser();

		

		testLib.SelectHauptAgentur();

		//vp.AddVP("AutomRKTestVP");

		//rk.StartRKTest();

		//consultation.AddConsultation();

		testLib.CheckVersion();

		rk.StartRKTest(vn, vp);

	   
	   console.log("Test is ready");

		 
	
	});

});


