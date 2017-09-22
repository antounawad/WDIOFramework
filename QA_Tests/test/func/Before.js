var BeforeScript = (function(){
    
    function BeforeScript() {
        this.targetUrl = process.argv[3];
        this.targetUrl = this.targetUrl.substr(12);
    };
    
    return BeforeScript;
    
    })();
    
    module.exports = BeforeScript;