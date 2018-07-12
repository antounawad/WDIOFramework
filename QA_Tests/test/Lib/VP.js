var TestLib = require('../Lib/ClassLib.js')
 var assert = require('assert');
const testLib = new TestLib();




class VP{
    
	ShowVps(timeout)
	{
		testLib.ClickAction('#btnFastForwardConsultation', '#btnNewVp',timeout)
	}

	SearchVP(searchValue,timeout=2000,pause=500){
        //this.ShowVps(timeout)
		testLib.SearchElement('#Search',searchValue,timeout, pause)
	}
}
module.exports = VP;






