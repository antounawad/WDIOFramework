var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Login = require('../Lib/Login.js')
const login = new Login()
var VN = require('../Lib/VN.js')
const vn = new VN()
var VP = require('../Lib/VP.js')
const vp = new VP()
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation()
var Salary = require('../Lib/Salary.js')
const salary = new Salary()
var Tarif = require('../Lib/Tarif.js')
const tarif = new Tarif()


describe('webdriver.io page', function () {


	it('Smoke Test...', function () {
		
        browser.url('http://'+testLib.targetUrl+'.xbav-berater.de/Beratung/Account/Login?ReturnUrl=%2FBeratung%2F')
		this.timeout(9999999999999999999999999999999999999999999999999999999999999999)

		testLib.ShowBrowserTitle('Anmelden | xbAV-Berater')

		login.LoginUser();

		vn.SearchVN('Automatic')

	 vp.SearchVP('Tests')

		 consultation.NewConsultation();

		 consultation.SetBruttoLohn(4343)

		 salary.SetZusatzBeitrag(10)

		tarif.IterateTarife()

		 browser.pause(5000);
		 
	
	});

});