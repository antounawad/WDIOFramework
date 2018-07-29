var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _DocumentsGenerateSelector = '#btn_generate';
var _SiteTitle = 'Abschluss – Dokumente';
var _FailSite = 'Abschluss - Status';

class Document{

    get DocumentsGenerateSelector(){return _DocumentsGenerateSelector};

    GenerateDocuments()
    {
        if(!testLib.DocumentTest)
        {
            return;            
        }
        testLib.Navigate2Site(_SiteTitle,_FailSite);
        testLib.OnlyClickAction(_DocumentsGenerateSelector,500);
        testLib.WaitUntilVisible(_DocumentsGenerateSelector,100000);
        testLib.PauseAction(1000);
        assert.equal(browser.getText('#generatedDocuments').indexOf('mdi-alert-circle-outline'), -1 , "Fehler bei der Dokumentegenerierung");
        
    }
}


module.exports = Document;






