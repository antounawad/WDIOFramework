var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

class VN{

    ShowVNs(timeout=20000)
    {
        testLib.ClickAction('#btnXbavMainAgency', '#btnNewVn',timeout)
    }

    SearchVN(searchValue,timeout=20000){
        this.ShowVNs(timeout)
        testLib.SearchElement('#Search',searchValue)
	}
}
module.exports = VN;






