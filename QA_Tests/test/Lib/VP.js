var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

class VP{
    
	ShowVps(timeout)
	{
		testLib.ClickAction('#btnFastForwardVp', '#btnNewVp',timeout)
	}

	SearchVP(searchValue,timeout=20000,pause=3000){
        this.ShowVps(timeout)
		testLib.SearchElement('#Search','Tests',timeout, pause)
	}
}
module.exports = VP;






