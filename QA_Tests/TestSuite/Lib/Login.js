var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _passwd = 'qvbno@12F5';
var _user   = 'hans-peter.bremer';
var _loginBtnSelector = '#ID_Login_Button';
var _passwordSelector = '#Password';
var _usernameSelector = '#Username';

class Login{
    LoginUser(password=_passwd,username=_user,passwordSelector=_passwordSelector, loginBtnSelector=_loginBtnSelector,usernameSelector=_usernameSelector){
 		var userElement = browser.element(usernameSelector);
 		assert.notEqual(userElement, null);
 		userElement.setValue(username);
 		var passwdElement = browser.element(passwordSelector);
 		assert.notEqual(passwdElement, null);
 		passwdElement.setValue(password);
 		testLib.ClickAction(loginBtnSelector);
	}
}
module.exports = Login;





