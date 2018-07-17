var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var _SalaryBeitrag = '#GesetzlicheKrankenversicherungZusatzbeitrag';

class Salary{
    ShowSalary(timeout=10000,pause=3000){
   		testLib.ClickAction(testLib.BtnNavNext,'', 20000, pause)
	}

     SetZusatzBeitrag(value,timeout=10000,pause=3000){
         this.ShowSalary(timeout,pause)
         testLib.SearchElement(_SalaryBeitrag,value)	
    }

}
module.exports = Salary;






