var TestLib = require('../Lib/ClassLib.js')
var assert = require('assert');
const testLib = new TestLib();
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation();

var _btnNewVp = '#btnNewVp';
var _gridSelector = '#tableList';
var _vpNode = 'VP'
var _siteTitle = 'Arbeitnehmer – Auswahl';


class VP {

    ShowVps(timeout) {
        testLib.ClickElement(consultation.BtnFastForwardConsultation, _btnNewVp, timeout)
    }

    SearchVP(searchValue, timeout = 2000, pause = 500) {
        //this.ShowVps(timeout)
        testLib.SetValue('#Search', searchValue)
    }

    AddVP(testVPName) {
        testLib.Navigate2Site(_siteTitle);
        this.SearchVP(testVPName);
        testLib.PauseAction(1000);

        if (!testLib.IsEnabled(testLib.BtnNavNext,2000)) {
            this.AddChapter(testVPName);
        }
        else {
            if (testLib.IsVisible(testLib.BtnBlurredOverlay,1000)) {
                testLib.ClickElementSimple(testLib.BtnBlurredOverlay);
            }
            testLib.ClickElementSimple(_gridSelector);
        }
    }

    AddChapter(testVNName) {
        testLib.AddChapter(_vpNode, _btnNewVp);
        testLib.Next();
    }
}
module.exports = VP;






