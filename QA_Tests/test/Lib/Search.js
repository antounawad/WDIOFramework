class SearchElement{
    
     get url()  {return  process.argv[3]}
     get targetUrl() {return this.url.substr(2);}
     //set url(value) {this.url = value}

}
module.exports = SearchElement;






