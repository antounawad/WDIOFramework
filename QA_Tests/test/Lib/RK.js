var TestLib = require('../Lib/ClassLib.js')
 var assert = require('assert');
const testLib = new TestLib();
var VN = require('../Lib/VN.js')
const vn = new VN()
var VP = require('../Lib/VP.js')
const vp = new VP()


var checkIsEnabled = null;



class RK{

	get smokeTest()
	{
		return _smokeTest;
	}

	

	StartRKTest()
	{

		vn.CheckVN('AutomRKTestVN');
			// Navigate to Target Site
		testLib.Navigate2Selector('Arbeitgeber – Tarifvorgabe')

		this.CheckExistTariffs();

		testLib.ClickAction('#btnNewTariffConfig');


		var ImplementationList   = null;
		var ImplementationIds    = null;

		var TypeList   = null;
		 var TypeIds    = null;
	 

		var TarifList   = null;
		var TarifIds    = null;

		var	OptionList   = null;
		var	OptionIds    = null;

		var ZusageArtList   = null;
		var ZursageArtIds   = null;

		var BeantragungList   = null;
		var BeantragungIds    = null;

		var GruppenRahmenList   = null;
		var GruppenRahmenIds    = null;



		if(testLib.Versicherer != null && testLib.SmokeTest)
		{
			var Selector = null;
			var List = null;
			var Values = null;
			var Ids = null;
		
			testLib.Versicherer.forEach(versicherer => {
				for (var tarifSel = 0; tarifSel < testLib.TarifSelectoren.length; tarifSel++)
				{

						Selector = '#'+testLib.TarifSelectoren[tarifSel]["Value"][0];
						if(testLib.TarifSelectoren[tarifSel]["CheckVisible"][0] == "true")
						{
							var CheckVisible = browser.isVisible(Selector); 
							if(!CheckVisible)
							{
								continue;
							}
						}

						
						List = $(Selector);
						Values = List.getAttribute("md-option[ng-repeat]", "value",true);
						Ids = List.getAttribute("md-option[ng-repeat]", "id",true);

						testLib.OnlyClickAction(Selector,1000);

						if(tarifSel == 0)
						{
							testLib.ClickAction('#'+Ids[Values.indexOf(versicherer['Id'][0])]);
	
						}
						else
						{
							var checkIsEnabled =	browser.getAttribute(Selector, "disabled");
							if(Ids.length > 1 && checkIsEnabled == null)
							{
								testLib.ClickAction('#'+Ids[0]);
							}
						}						
				
				}
		   
				
				testLib.ClickAction('#modalContainer_btnSpeichern');
				
				testLib.Navigate2Selector('Arbeitnehmer – Auswahl')

				vp.SearchVP('AutomRKTestVP',2000);

				testLib.ClickAction('#btnFastForwardConsultation','#btnFastForward',50000,0,true);

				//testLib.ClickAction('#btnFastForward');

				testLib.PauseAction(2000);
				var IsTariRadioIsVisible = browser.isVisible('#tarifAuswahlRadioGroup'); 

				if(IsTariRadioIsVisible)	
				{
					testLib.ClickAction('#radio_8')
				}							

				testLib.Navigate2Selector('Angebot – Kurzübersicht')


				testLib.ClickAction('.fold-toggle.hide.show-gt-sm.md-font.mdi.mdi-24px.mdi-backburger');


				testLib.ClickAction('#navChapterLink_1','#navViewLink_VnVnVersorgungswerk');
				testLib.ClickAction('#navViewLink_VnVnVersorgungswerk');

				testLib.ClickAction('.ng-scope.md-font.mdi.mdi-24px.mdi-delete','#modalDeleteAreYouSure_btnLöschen');
				testLib.ClickAction('#modalDeleteAreYouSure_btnLöschen','#btnNewTariffConfig');


				testLib.ClickAction('#btnNewTariffConfig');				
	 
				

				
			

				

		 


			});
			
		}




	}

	CheckExistTariffs()
	{
		
		// Falls es beim Löschen Probleme gab, wird hier nochtmal geprüft und ggfs. gelöscht
		if(browser.isExisting('.ng-scope.md-font.mdi.mdi-24px.mdi-delete'))
		{

			testLib.ClickAction('.ng-scope.md-font.mdi.mdi-24px.mdi-delete','#modalDeleteAreYouSure_btnLöschen');
			testLib.ClickAction('#modalDeleteAreYouSure_btnLöschen');
		}		
	}


	
	

}
module.exports = RK;






