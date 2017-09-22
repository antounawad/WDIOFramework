// var helper = require ('./specs/helper.js');
var assert = require('assert');
var HelperObject  = require("../func/HelperObject.js")

describe('webdriver.io page', function () {

	var beforeScript;
	
	before(function() {
		beforeScript = new HelperObject(); 
	});

	it('should have the right title - the fancy generator way', function () {

		

  console.log(beforeScript.targetUrl);
  
   var targetUrl = beforeScript.targetUrl;
 
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



		var retValue = $('#ID_Login_Button');
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, 100000);
		browser.click(retValue.selector);
		console.log(browser.getTitle());


		retValue = $('#btnXbavMainAgency');
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, 100000);
		browser.click(retValue.selector);
		console.log(browser.getTitle());


        browser.waitForVisible('#btnNewVn', 100000);
		title = browser.getTitle();
        console.log(title);


		var searchVN = browser.element('#Search');
		assert.notEqual(searchVN, null);
        searchVN.setValue('Automatic');

		retValue = $('#btnFastForwardVp');
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, 100000);
		browser.click(retValue.selector);
		console.log(browser.getTitle());

		browser.waitForVisible('#btnNewVp', 100000);
		title = browser.getTitle();
        console.log(title);

		var  vp2 = browser.element('#Search');
        console.log(vp2.getText());
		assert.notEqual(vp2, null);
        vp2.setValue('Tests');
		browser.pause(3000);


		retValue = $('#btnFastForwardConsultation');
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, 100000);
		browser.click(retValue.selector);
		console.log(browser.getTitle());
		browser.pause(3000);
		
		
		retValue = $('#btnNewConsultation');
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, 100000);
		browser.click(retValue.selector);
		console.log(browser.getTitle());
		browser.pause(10000);


		// Give the Salary
		var salaryData = browser.element('#Bruttolohn');
		assert.notEqual(salaryData,null);
		salaryData.setValue('4343')
		browser.pause(5000);

		
		retValue = $('#btnNavNext');
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, 100000);
		browser.click(retValue.selector);
		console.log(browser.getTitle());
		browser.pause(5000);
				
		retValue = $('#navChapterLink_5');
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, 100000);
		browser.click(retValue.selector);
		console.log(browser.getTitle());		
		browser.pause(5000);

		retValue = $('#navViewLink_BeratungBeratungTarifauswahl');
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, 100000);
		browser.click(retValue.selector);
		console.log(browser.getTitle());		
        browser.pause(5000);
        

        retValue = $('#container-content');
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, 100000);
        var objarr = browser.elementIdElements(ID,retValue.selector);
        console.log("Objarr: "+objarr);		
        browser.pause(5000);








		// retValue = $('#radio_1');
		// assert.notEqual(retValue.selector,"");
		// browser.waitForEnabled(retValue.selector, 100000);
		// browser.click(retValue.selector);
		// console.log(browser.getTitle());		
		// browser.pause(8000);
		

		// browser.waitForEnabled('#btnNavNext', 100000);
		// retValue = $('#btnNavNext');
		// assert.notEqual(retValue.selector,"");
		// browser.click(retValue.selector);
		// browser.pause(8000);
		// browser.click(retValue.selector);
		// browser.pause(8000);
		// browser.click(retValue.selector);
		// browser.pause(8000);


		// retValue = $('#navChapterLink_6');
		// assert.notEqual(retValue.selector,"");
		// browser.waitForEnabled(retValue.selector, 100000);
		// browser.click(retValue.selector);
		// console.log(browser.getTitle());		
		// browser.pause(8000);


		// retValue = $('#navViewLink_AngebotAngebotAngebotsdaten');
		// assert.notEqual(retValue.selector,"");
		// browser.waitForEnabled(retValue.selector, 100000);
		// browser.click(retValue.selector);
		// console.log(browser.getTitle());		
		// browser.pause(8000);


	
		
	});

});