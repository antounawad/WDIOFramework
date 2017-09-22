var Helper =(function(){
    
    function Helper() {
        this.url = process.argv[3];
        this.targetUrl = this.url.substr(12);

    };

    
    
    return Helper;
    
    })();
    
    module.exports = Helper;

