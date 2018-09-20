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





describe('webdriver.io page', function () {

	it('LayoutTest...', function () {
		
		
			this.timeout(testLib.UrlTimeOut);
			
            testLib.InitBrowserStart();
            
            testLib.TakeScreenShotAllDialogs = true;
		
			// // Login
			// // Todo extrahieren
			login.LoginUser()
			
            testLib.SelectHauptAgentur();
            
            testLib.AddChapter(vn,vp,consultation);

            testLib.Navigate2Site(testLib.StatusSiteTitle);

			console.log("Test is ready");
			
	});

});


