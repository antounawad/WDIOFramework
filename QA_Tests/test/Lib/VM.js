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

        testLib.ClickAction('.fa.fa-industry','#FilterSearch');
        testLib.SearchElement('#FilterSearch',agency);
        testLib.ClickAction('.btn.btn-sm.btn-w-m.btn-outline.btn-search');
        if(browser.isExisting('.fa.fa-trash-o'))
        {
            
            testLib.OnlyClickAction('.fa.fa-trash-o');
            testLib.WaitUntilVisible('.btn.btn-primary');          
            testLib.OnlyClickAction('.modal-content')
            testLib.OnlyClickAction('.btn btn-primary');
        }
        
        browser.url(refUrl);

    }


}
module.exports = VM;






