var TestLib = require('../Lib/ClassLib.js')
 var assert = require('assert');
const testLib = new TestLib();
var VN = require('../Lib/VN.js')
const vn = new VN()
var VP = require('../Lib/VP.js')
const vp = new VP()
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation()


class RK{

	StartRKTest()
	{

		vn.CheckVN('AutomRKTestVN');
			// Navigate to Target Site
		testLib.Navigate2Site('Arbeitgeber – Tarifvorgabe')

		this.RemoveExistTariffs();

		this.AddTarifOption();

	}

	AddTarifOption()
	{
		testLib.OnlyClickAction('#btnNewTariffConfig');
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

						testLib.OnlyClickAction(Selector);

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

				if(vn.NewVn)
				{		
					vn.AddZahlungsart();
				}
				
				testLib.Navigate2Site('Arbeitnehmer – Auswahl')

				vp.CheckVP('AutomRKTestVP');

				testLib.Navigate2Site('Beratungsübersicht');

				consultation.AddConsultation();

				testLib.Navigate2Site('Angebot – Kurzübersicht')

				this.CheckRKResult();

				this.Jump2TarifSite();

				this.RemoveExistTariffs();

				testLib.ClickAction('#btnNewTariffConfig');

			});
			
		}
	}

	RemoveExistTariffs()
	{
		testLib.PauseAction(1000);
		if(browser.isExisting('.ng-scope.md-font.mdi.mdi-24px.mdi-delete'))
		{

			testLib.ClickAction('.ng-scope.md-font.mdi.mdi-24px.mdi-delete','#modalDeleteAreYouSure_btnLöschen');
			testLib.ClickAction('#modalDeleteAreYouSure_btnLöschen','#btnNewTariffConfig');
			testLib.PauseAction(1000);
			
		}		
	}


	Jump2TarifSite()
	{
		testLib.ClickAction('.fold-toggle.hide.show-gt-sm.md-font.mdi.mdi-24px.mdi-backburger');

		testLib.ClickAction('#navChapterLink_1','#navViewLink_VnVnVersorgungswerk');

		testLib.ClickAction('#navViewLink_VnVnVersorgungswerk');

	}

	CheckRKResult()
	{
			browser.waitUntil(function () 
			{
				return  browser.isVisible('#btnNavNext');
		 	 }, 50000, 'expected text to be different after 5s');
			
			var errorBlock = $("md-card[ng-show='HasErrorMessages']");
	
			if(errorBlock !== undefined)
			{
				console.log("Errorblock class: " + errorBlock.getAttribute('class'));
				console.log("Errorblock index of ng-hide: " + errorBlock.getAttribute('class').indexOf('ng-hide'));
				assert.notEqual(errorBlock.getAttribute('class').indexOf('ng-hide'), -1, 'Fehler bei Angebotserstellung für Tarif: ' + browser.getText("span[class='label-tarif']")+ browser.getText("div[class='label-details']"));
			}
			else
			{
				assert.equal(0, 1, 'Rechenkernseite prüfen');
			} 

	}

}
module.exports = RK;






