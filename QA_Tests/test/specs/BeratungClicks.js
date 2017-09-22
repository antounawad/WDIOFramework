var assert = require('assert');
var HelperObject  = require("../func/HelperObject.js")

describe('webdriver.io page', function () {

	var helperObject;
	
	before(function() {
		helperObject = new HelperObject(); 
	});


	function ClickAction(selector, timeout, pauseTime=0){
		var retValue = $(selector);
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, timeout);
		browser.click(retValue.selector);
		console.log(browser.getTitle());
		PauseAction(pauseTime);
		return retValue.selector;
	};


	function SearchAction(selector,value, pauseTime=0){
		var searchVN = browser.element(selector);
		assert.notEqual(searchVN, null);
		searchVN.setValue(value);
		PauseAction(pauseTime);
	};

	function PauseAction(pauseTime){
		if(pauseTime > 0)
			{
				browser.pause(pauseTime);
			}
	};

	function CheckResult(pauseTime){
		if(pauseTime > 0)
			{
				browser.pause(pauseTime);
			}
	};



	it('should have the right title - the fancy generator way', function () {

		

  console.log(helperObject.targetUrl);
  
   var targetUrl = helperObject.targetUrl;
 
  console.log(targetUrl);
  browser.url('http://'+targetUrl+'.xbav-berater.de/Beratung/Account/Login?ReturnUrl=%2FBeratung%2F');
  this.timeout(9999999999999999999999999999999999999999999999999999999999999999);
      
		var title = browser.getTitle();
		console.log(title);

		assert.equal(title, 'Anmelden | xbAV-Berater');


		var username = browser.element('#Username');
		console.log(username);

		assert.notEqual(username, null);

		username.setValue('hans-peter.bremer');

		var password = browser.element('#Password');
		console.log(password);

		assert.notEqual(password, null);

		password.setValue('qvbno@12F5');

		ClickAction('#ID_Login_Button', 100000);

		ClickAction('#btnXbavMainAgency', 100000);

        browser.waitForVisible('#btnNewVn', 100000);

		SearchAction('#Search','Automatic')

		ClickAction('#btnFastForwardVp', 100000);

		browser.waitForVisible('#btnNewVp', 100000);
		
		SearchAction('#Search','Tests',3000)

		ClickAction('#btnFastForwardConsultation', 100000, 3000);

		ClickAction('#btnNewConsultation', 100000, 10000);
		
		SearchAction('#Bruttolohn','4343',5000)

		ClickAction('#btnNavNext', 100000, 5000);

		

		var radio = [];
		radio[0] = '#radio_1';
		radio[1] = '#radio_4';
		radio[2] = '#radio_8';
 
		 radio.forEach(function(element) {
			console.log("Element: "+element);
			ClickAction('#navChapterLink_5', 100000, 5000);
			ClickAction('#navViewLink_BeratungBeratungTarifauswahl', 100000, 5000);			 
			ClickAction(element, 100000, 8000);
			var selector = ClickAction('#btnNavNext', 100000, 8000);
			browser.click(selector);
			browser.pause(8000);
			browser.click(selector);
			browser.pause(8000);
			ClickAction('#navChapterLink_6', 100000, 8000);
			ClickAction('#navViewLink_AngebotAngebotAngebotsdaten', 100000, 8000);
			ClickAction('#btnNavNext', 100000, 10000);

			var retValue = $('#backToOfferBtn');
			//assert.equal(retValue,null,'Fehler bei der Angebotserstellung.');
			


 		}, this);




 		// var radioGrid = $('#tarifAuswahlRadioGroup');
		// assert.notEqual(radioGrid.selector,"");
		// console.log(radioGrid);
 		// browser.pause(3000);
		

 		// var objarr = browser.elements('#tarifAuswahlRadioGroup');
		//  browser.pause(3000);
		//  console.log("Ojbect: "+objarr);
		//  console.log("Value: "+objarr.value);


		
	});

});