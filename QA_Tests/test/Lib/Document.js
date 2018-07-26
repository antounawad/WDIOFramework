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

        try
        {
            testLib.Navigate2Site(_SiteTitle);
            
            testLib.WaitUntilVisible(_DocumentsGenerateSelector,80000);
            
            testLib.OnlyClickAction(_DocumentsGenerateSelector,500);
    
            testLib.WaitUntilVisible(_DocumentsGenerateSelector,80000);
            
            assert.equal(browser.getText('#generatedDocuments').indexOf('mdi-alert-circle-outline'), -1 , "Fehler bei der Dokumentegenerierung");
    
        }
        catch(ex)
        {
            throw new Error(ex);

        }


        
    }
}


module.exports = Document;






