var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

class Salary{
    ShowSalary(timeout=10000,pause=3000){
   		testLib.ClickAction('#btnNavNext','#navChapterLink_5', timeout, pause)
	}

     SetZusatzBeitrag(value,timeout=10000,pause=3000){
         this.ShowSalary(timeout,pause)
         testLib.SearchElement('#GesetzlicheKrankenversicherungZusatzbeitrag',value,timeout)	
    }

}
module.exports = Salary;






