var assert = require('assert');
var TestLib = require('C:/git/shared/QA_Tests/TestSuiteLib/ClassLib.js')
const testLib = new TestLib();

var _passwd = 'qvbno@12F5';
var _user   = 'hans-peter.bremer';
var _loginBtnSelector = '#ID_Login_Button';
var _passwordSelector = '#Password';
var _usernameSelector = '#Username';
var _sessionBusy = '#btnLogOffOthers';
var _newlogin = '.md-raised.md-accent.md-button.md-ink-ripple.flex-none';

class Login{
    LoginUser(username=_user,password=_passwd,passwordSelector=_passwordSelector, loginBtnSelector=_loginBtnSelector,usernameSelector=_usernameSelector){
 		var userElement = $(usernameSelector);
 		assert.notEqual(userElement, null);
 		userElement.setValue(username);
 		var passwdElement = $(passwordSelector);
 		assert.notEqual(passwdElement, null);
 		passwdElement.setValue(password);
		 testLib.ClickElement(loginBtnSelector);
		 
		 testLib.SaveScreenShot();

		if(testLib.CheckIsVisible(_sessionBusy))
		{
			testLib.ClickElementSimple(_sessionBusy);
		}
		if(testLib.CheckIsVisible(_newlogin))
		{
			this.LoginUser(password,username);
		}

		testLib.SaveScreenShot();
	}
}
module.exports = Login;






