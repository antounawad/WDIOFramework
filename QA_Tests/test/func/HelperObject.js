var Helper =(function(){
    
    function Helper() {
        this.url = process.argv[3];
        this.targetUrl = this.url.substr(12);


        function ClickAction(selector, timeout){
            var retValue = $(selector);
            assert.notEqual(retValue.selector,"");
            browser.waitForEnabled(retValue.selector, timeout);
            browser.click(retValue.selector);
            console.log(browser.getTitle());
        };


    };

    
    
    return Helper;
    
    })();
    
    module.exports = Helper;

