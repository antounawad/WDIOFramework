var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _DocumentsGenerateSelector = '#btn_generate';
var _SiteTitle = 'Abschluss – Dokumente';
var _NavLink = 'navViewLink_AbschlussAbschlussDokumente';


class Document{

    get DocumentsGenerateSelector(){return _DocumentsGenerateSelector};

    GenerateDocuments()
    {
        if(!testLib.DocumentTest)
        {
            return;            
        }
        var failSite = testLib.StatusSiteTitle+':'+testLib.NavChapterDokumente+':'+_NavLink;
        testLib.Navigate2Site(_SiteTitle,failSite);
        testLib.OnlyClickAction(_DocumentsGenerateSelector,500);
        testLib.WaitUntilVisible(_DocumentsGenerateSelector,100000);
        assert.equal(browser.getText('#generatedDocuments').indexOf('mdi-alert-circle-outline'), -1 , "Fehler bei der Dokumentegenerierung");
        
    }
}


module.exports = Document;






