var TestLib = require('../Lib/ClassLib.js')
 var assert = require('assert');
const testLib = new TestLib();

var _vpNode = 'VM'
var _url = 'Vermittlerbereich'


class VM{
    
    get url(){return 'http://'+ testLib.TargetUrl+'.xbav-berater.de/'+_url};
    
    DeleteAgency(agency)
    {
        var refUrl = browser.getUrl();
        browser.url(this.url);

        testLib.ClickElement('.fa.fa-industry','#FilterSearch');
        testLib.SetValue('#FilterSearch',agency);
        testLib.ClickElement('.btn.btn-sm.btn-w-m.btn-outline.btn-search');
        if(browser.isExisting('.fa.fa-trash-o'))
        {
            
            testLib.ClickElementSimple('.fa.fa-trash-o');
            testLib.WaitUntilVisible('.btn.btn-primary');          
            testLib.ClickElementSimple('.modal-content')
            testLib.ClickElementSimple('.btn btn-primary');
        }
        
        browser.url(refUrl);

    }


}
module.exports = VM;






