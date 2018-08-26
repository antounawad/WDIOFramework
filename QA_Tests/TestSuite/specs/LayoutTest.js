var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Login = require('../Lib/Login.js')
const login = new Login()
var VN = require('../Lib/VN.js')
const vn = new VN()


describe('LayoutTest', function () {

	it('The Test...', function () {

		this.timeout(this.UrlTimeOut);
		testLib.InitBrowserStart();
 
		// Login
		// Todo extrahieren
		login.LoginUser();

		testLib.SelectHauptAgentur();

		vn.AddVN(testLib.VnName, true);

		var result = testLib.CompareValue('#Wageslip','An Lexware angelehnrr')
	  
  	    console.log("Test is ready");

		 
	
	});

});


