var TestLib = require('C:/git/shared/QA_Tests/TestSuiteLib/ClassLib.js')
const testLib = new TestLib();
const assert = require('assert');
var ScreenShotFolderpath = './Temp/screenshot.png';

describe('Portal Agentur anlegen', () => {
    it('should have the right title', () => {


        browser.url('https://www.domus-software.de/');
        testLib.CompareAndClickIfMatch("href", "https://www.domus-software.de/funktion/", "kONFIGURATOR")
        testLib.SetValue('[name="product_configurator[ap]"]', '16')
        testLib.PauseAction(1000)
        testLib.SetValue('[name="product_configurator[ve]"]', '5')
        testLib.PauseAction(1000)
        testLib.ScrollToView('href', '#gruppe7');
        testLib.CompareAndClickIfMatch("href", "#gruppe7", "CLouD services")
        testLib.ClickElementSimple('[value="Mieter-/Eigentümer-Plattform"]')
        testLib.ClickElementSimple('[value="DOMUS Software suchen"]')
        testLib.ScrollToView('href', 'https://www.domus-software.de/?page_id=37');
        testLib.ClickElementSimple('[href="https://www.domus-software.de/?page_id=37"]')

        var productDetailsCheck = $('.text--orange.produkt__features__title')

        if (productDetailsCheck.isDisplayed()) 
        {
            console.log("Product Detail is found , Test Pass")
        }
        else {

            console.log("Oppss : Something went Wrong , No Product Details found. No Worries , i will Save a Screenshot for you :) ")
            browser.saveScreenshot(ScreenShotFolderpath);
        }





        var x = 99;


    });

});


