var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

var _DocumentsGenerateSelector = '#btn_generate';
var _SiteTitle = 'Abschluss - Status';
var _NavLink = 'navViewLink_AbschlussAbschlussDokumente';


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
            var failSite = testLib.StatusSiteTitle+':'+testLib.NavChapterDokumente+':'+_NavLink;
            testLib.Navigate2Site(_SiteTitle,failSite);
            var title = browser.getTitle();
            if(title.includes(_SiteTitle))
            {
                testLib.ClickElementSimple(testLib.BtnNavPrev,failSite);
                testLib.IsVisible(_DocumentsGenerateSelector);
            }
            testLib.ClickElementSimple(_DocumentsGenerateSelector,500);
            testLib.IsVisible(_DocumentsGenerateSelector,100000);
            
            if(browser.getText('#generatedDocuments').indexOf('mdi-alert-circle-outline') >= 0)
            {
                throw new Error("Fehler bei der Dokumentegenerierung");
            }
        }catch(ex)
        {
            throw new Error(ex);            
        }

        //assert.equal(browser.getText('#generatedDocuments').indexOf('mdi-alert-circle-outline'), -1 , "Fehler bei der Dokumentegenerierung");
        
    }
}


module.exports = Document;






