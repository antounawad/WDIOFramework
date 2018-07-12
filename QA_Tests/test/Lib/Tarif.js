var assert = require('assert');
var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();

class Tarif{
    ShowTarif(timeout=10000,pause=3000){
   		testLib.ClickAction('#btnNavNext','#navViewLink_BeratungBeratungTarifauswahl', timeout, pause)
	}

	AddTarifConfig()
	{


		testLib.ClickAction('#md-select-icon',1000)
		//md-select-icon

		//$('md-select[placeholder="Allianz"]').click()


		//var selector = $('.md-text.ng-binding');
		//selector.selectByValue('Allianz')

		var selector = $('.md-select-icon')
		browser.click(selector);



		var selectBox = $('#SelectedItem_VrId'); 
		testLib.ClickAction(selectBox);



		var selectBox2 = $('#SelectedItem.VrId');
		var selectBox3 = $('#Key:AvailableInsurers');



	//	var element = $('[value="1006"]');
	//	console.log(element.isSelected()); 
		var selectBox = $('.ng-scope md-ink-ripple');
		console.log(selectBox.getValue());

		

		
		var selector = testLib.ClickAction('#select_container_18');
		browser.selectByVisibleText(selector,'Allianz');
		sel.selectByValue('Allianz');
		var searchSelector = browser.element('#select_value_label_0');
		searchSelector.selectByValue('1006');

		browser.selectByValue('#SelectedItem_VrId','Allianz')
		testLib.SearchElement('#SelectedItem_ImplementationMethodId','Direktversicherung § 3 Nr. 63 EStG');
		

	}


    IterateTarife(timeout=10000,pause=3000){

        this.ShowTarif(timeout,pause)

 		var radio = [];
 		radio[0] = '#radio_1';
		//sradio[1] = '#radio_3';
	    // radio[2] = '#radio_7';
	    // radio[3] = '#radio_8';s
		// radio[4] = '#radio_10';
		// radio[5] = '#radio_11';*
		// radio[6] = '#radio_12';
	   

		 radio.forEach(function(element) {

			testLib.ClickAction('#navChapterLink_5','#navViewLink_BeratungBeratungTarifauswahl');
			testLib.ClickAction('#navViewLink_BeratungBeratungTarifauswahl');



			 testLib.ClickAction(element,'', timeout, pause);
		    // var tarifLogo = $(element + " div.vr-tarif-info-logo");		
			// console.log("Selected Tarif Logo: "+ tarifLogo.getAttribute('back-img').substr(29));
			// var selector = testLib.ClickAction('#btnNavNext', '', 100000, 5000);
			// testLib.ClickAction('#btnNavNext', '', 100000, 5000);
			// browser.click(selector);
			// browser.pause(8000);
			// browser.click(selector);
			// testLib.ClickAction('#btnNavNext', '', 100000, 5000);
			// browser.pause(8000);
			// testLib.ClickAction('#navChapterLink_6','#navViewLink_AngebotAngebotAngebotsdaten', 100000, 8000);
			// testLib.ClickAction('#navViewLink_AngebotAngebotAngebotsdaten','', 100000, 8000);
			// testLib.ClickAction('md-select-value#select_value_label_2.md-select-value','#select_option_34', 100000, 5000); // md-select-value#select_value_label_2.md-select-value
			// testLib.ClickAction('#select_option_34','#btnNavNext', 100000, 3000);
			 testLib.ClickAction('#btnNavNext','', timeout, pause);

			// var errorBlock = $("md-card[ng-show='HasErrorMessages']");
			// if(errorBlock !== undefined)
			// 	{
			// 		console.log("Errorblock class: " + errorBlock.getAttribute('class'));
			// 		console.log("Errorblock index of ng-hide: " + errorBlock.getAttribute('class').indexOf('ng-hide'));
			// 		assert.notEqual(errorBlock.getAttribute('class').indexOf('ng-hide'), -1, 'Fehler bei Angebotserstellung für Tarif: ' + browser.getText("span[class='label-tarif']")+ browser.getText("div[class='label-details']")); // the MsG of the Assert will be printed only if Equal is happned 
			// 	}
			// 	else{
			// 		assert.equal(0, 1, 'Test passt nicht mehr');
			// 	} // alles kaputt!!!
			



		 }, this);
    }

}
module.exports = Tarif;






