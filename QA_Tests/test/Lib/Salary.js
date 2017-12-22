var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

class Salary{
    ShowSalary(timeout=10000,pause=3000){
   		testLib.ClickAction('#btnNavNext','#navChapterLink_5', 100000, 5000)
	}

     SetZusatzBeitrag(value,timeout=10000,pause=5000){
         this.ShowSalary(timeout,pause)
         testLib.SearchElement('#GesetzlicheKrankenversicherungZusatzbeitrag',value,timeout)	
    }

}
module.exports = Salary;






