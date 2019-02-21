var TestLib = require('C:/git/shared/QA_Tests/TestSuiteLib/ClassLib.js')
const testLib = new TestLib();
const assert = require('assert');


describe('Portal Agentur anlegen', () => {
    it('should have the right title', () => {


        browser.url('https://automatictest.xbav-berater.de/portal');



        testLib.ScrollToView('class', 'licenseFeaturePriceHint');
        testLib.PauseAction(1000);
        testLib.ClickElementSimple('.iCheck-helper')
        testLib.SetValue('#Vorname', 'TestAgentur');
        testLib.SetValue('#Nachname', 'LastName');
        testLib.SetValue('#emailfield', testLib.GenerateRandomEmail('@antoun.test.eg'));
        
        // testLib.ClickElementSimple('#AgenturTyp');
        testLib.ClickElementByAttribute('value', '1');
        // testLib.SetSimpleListBoxValue('#AgenturTyp', 2);
    //  var x = 99;







    });

});


