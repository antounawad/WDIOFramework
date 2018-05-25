var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Login = require('../Lib/Login.js')
const login = new Login()


describe('webdriver.io page', function () {


	it('Login Logout Test', function () {
		
        browser.url('http://'+testLib.targetUrl+'.xbav-berater.de/Beratung/Account/Login?ReturnUrl=%2FBeratung%2F')
		this.timeout(9999999999999999999999999999999999999999999999999999999999999999)

		testLib.ShowBrowserTitle('Anmelden | xbAV-Berater')

		login.LoginUser();

		browser.pause(5000);
		 
	
	});

});