var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _DocumentsGenerateSelector = '#btn_generate';
var _SiteTitle = 'Abschluss – Dokumente'

class Document{

    get DocumentsGenerateSelector(){return _DocumentsGenerateSelector};

    GenerateDocuments()
    {
        if(!testLib.DocumentTest)
        {
            return;            
        }
        testLib.Navigate2Site(_SiteTitle);
		testLib.OnlyClickAction(_DocumentsGenerateSelector);
		testLib.WaitUntil(_DocumentsGenerateSelector,80000);
    }
}


module.exports = Document;






