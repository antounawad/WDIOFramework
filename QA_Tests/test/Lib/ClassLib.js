var assert = require('assert');
var defaultTimout = 10000;
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
		this.PauseAction(pauseTime)
	}

    ClickAction(selector, waitforVisibleSelector='', timeout=10000, pauseTime=0){
		var retValue = $(selector);
		assert.notEqual(retValue.selector,"");
		browser.waitForEnabled(retValue.selector, timeout);
		browser.click(retValue.selector);
		console.log(browser.getTitle());
		this.PauseAction(pauseTime);

        if(waitforVisibleSelector != '')
        {
            browser.waitForVisible(waitforVisibleSelector, timeout);
        }

		retValue.selector;
	}

    PauseAction(pauseTime){
		if(pauseTime > 0)
			{
				browser.pause(pauseTime);
			}
    }

    WaitUntil(selector)
    {
       browser.waitUntil(function () {
            try {
                return !!body.element(selector).value;
            } catch (e) {
                return false;
            }
        });
    }

    CheckResult(pauseTime)
    {
		if(pauseTime > 0)
			{
				browser.pause(pauseTime);
			}
	}

}
module.exports = TestLib;






