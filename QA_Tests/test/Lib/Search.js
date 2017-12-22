class SearchElement{
    
     SearchElement(selector,value, pauseTime=0){
		var searchVN = browser.element(selector);
		assert.notEqual(searchVN, null);
		searchVN.setValue(value);
		PauseAction(pauseTime);
	};


}
module.exports = SearchElement;






