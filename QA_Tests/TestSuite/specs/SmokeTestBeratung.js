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
		
		
		this.timeout(testLib.UrlTimeOut);

		testLib.InitBrowserStart();
	
		login.LoginUser("ProduktivAutomatikTest@xbav.de","ProduktivAutomatikTest@xbav.de")

		testLib.CheckVersion();		

		rk.StartRKTest(vn, vp);

		console.log("Test is ready");
			

	});

});


