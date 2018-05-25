var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

class Login{
    LoginUser(password='qvbno@12F5',username='hans-peter.bremer',passwordSelector='#Password', loginBtnSelector='#ID_Login_Button',usernameSelector='#Username'){
 		var userElement = browser.element(usernameSelector);
 		console.log(userElement);
 		assert.notEqual(userElement, null);

 		userElement.setValue(username);

 		var passwdElement = browser.element(passwordSelector);
 		console.log(passwdElement);
 		assert.notEqual(passwdElement, null);

 		passwdElement.setValue(password);
 		testLib.ClickAction(loginBtnSelector);
	}
}
module.exports = Login;






