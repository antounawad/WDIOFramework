var TestLib = require('C:/git/shared/QA_Tests/TestSuiteLib/ClassLib.js')
const testLib = new TestLib();

var _DocumentsGenerateSelector = '#btn_generate';
var _SiteTitle = 'Abschluss - Status';
var _NavLink = 'navViewLink_AbschlussAbschlussDokumente';
var _errorSelector = "[ng-show='DocumentsToGenerate.ErrorMessage.length']";
var _errorText = "Fehler bei der Dokumentegenerierung";
var _errorCircle = 'mdi-alert-circle-outline';
var _documentSelector = '#generatedDocuments';



class Document {

    get DocumentsGenerateSelector() { return _DocumentsGenerateSelector };

    GenerateDocuments(errorcounter=0) {
        if (!testLib.DocumentTest) {
            return;
        }
        try {
            var failSite = testLib.StatusSiteTitle + ':' + testLib.NavChapterDokumente + ':' + _NavLink;
            testLib.Navigate2Site(_SiteTitle, failSite);
            var title = browser.getTitle();
            if (title.includes(_SiteTitle)) {
                testLib.ClickElementSimple(testLib.BtnNavPrev, failSite);
                testLib._WaitUntilVisible(_DocumentsGenerateSelector);
			}
            testLib.ClickElementSimple(_DocumentsGenerateSelector, 500);
            testLib._WaitUntilVisible(_DocumentsGenerateSelector, 100000);
            var errText = browser.getText(_documentSelector);
            var errorBlock = $(_errorSelector);
            if(errorBlock.state === "success"  || errText.indexOf(_errorCircle) >= 0 )
            {
                var x = errorBlock.getAttribute('class');
                if(!String(x).includes('ng-hide'))
                {
                    if(errorcounter > 0)
                    {
                        throw new Error(_errorText);
                    }
                    else
                    {
                        this.GenerateDocuments(1)
                    }
                }
            }

        } catch (ex) {
            throw new Error(ex);
        }


    }
}

//ng-show=DocumentsToGenerate.ErrorMessage.length
//_md ng-hide


module.exports = Document;






