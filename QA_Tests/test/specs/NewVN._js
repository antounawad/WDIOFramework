var assert = require('assert');

describe('webdriver.io page', function () {
	it('should have the right title - the fancy generator way', function () {

		// passing the url from the CMD to the test 
		var targetUrl = process.argv[3].substr(12);
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

		var btn = browser.element('#ID_Login_Button')
		console.log(btn);
		assert.notEqual(btn, null, btn.getText+"LoginBTN is not found");
		

		browser.waitForEnabled('#ID_Login_Button', 100000);
		browser.click('#ID_Login_Button');


		title = browser.getTitle();
		console.log(title);


		var btn2 = browser.element('#btnXbavMainAgency');
		console.log(btn2);
		assert.notEqual(btn2, null);

		browser.waitForEnabled('#btnXbavMainAgency', 100000);
		browser.click('#btnXbavMainAgency');


		// Create a new VN 

		browser.waitForVisible('#btnNewVn', 100000);
		title = browser.getTitle();
		console.log(title);
		var newVN = browser.element('#btnNewVn')
		console.log(newVN);
		assert.notEqual(newVN, null);
		browser.waitForEnabled('#btnNewVn', 100000);
		browser.click('#btnNewVn');
		browser.pause(4000);

		// Should print the ArbN  Stammdaten as a title
		var title = browser.getTitle()
		console.log(title);

		var nameVN = browser.element('#Name');
		assert.notEqual(nameVN, null);
		nameVN.setValue('NewVN');

		var street = browser.element('#Strasse');
		assert.notEqual(street, null);
		street.setValue('StreetTest');

		var plz = browser.element('#Plz');
		assert.notEqual(plz, null);
		plz.setValue('66111');
		browser.pause(2000);

		var nextBtnVN = browser.element('#btnNavNext')
		assert.notEqual(nextBtnVN, null,nextBtnVN.getText+" nicht erreicht");
		browser.waitForEnabled('#btnNavNext', 100000);
		browser.click('#btnNavNext');
		browser.pause(3000);

		// Check if the next Page is the Abteiling , if yes then the test is Pass  
		var title = browser.getTitle()
		assert.equal(title,"Arbeitgeber – Abteilungen | xbAV-Berater","Seite: Abteilung nicht erricht");
		console.log(title);
		
		browser.pause(5000);
		
	});

});