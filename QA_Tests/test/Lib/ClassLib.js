var assert = require('assert');
class TestLib{
    
     get url()  {return  process.argv[3]}
     get targetUrl() {return this.url.substr(2)}


    get browserTitle() {return browser.getTitle()}

    ShowBrowserTitle(assertString='')
    {
        console.log("Browser title: ")
        console.log(this.browserTitle)
        if(assertString != '')
        {
            assert.equal(this.browserTitle, assertString);
        }

    }

     SearchElement(selector,value, pauseTime=0){
		var searchSelector = browser.element(selector)
		assert.notEqual(searchSelector, null)
		searchSelector.setValue(value)
		PauseAction(pauseTime)
	};

     //set url(value) {this.url = value}

}
module.exports = TestLib;





