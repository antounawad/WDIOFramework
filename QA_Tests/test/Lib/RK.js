var TestLib = require('../Lib/ClassLib.js')
 var assert = require('assert');
const testLib = new TestLib();
var VN = require('../Lib/VN.js')
const vn = new VN()
var VP = require('../Lib/VP.js')
const vp = new VP()
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation()
var Tarif = require('../Lib/Tarif.js')
const tarif = new Tarif()
var Document = require('../Lib/Document.js')
const document = new Document();


class RK{

	StartRKTest()
	{

		vn.CheckVN('AutomRKTestVN');
			
		testLib.Navigate2Site('Arbeitgeber – Tarifvorgabe')

		tarif.RemoveExistTariffs();

		this.AddTarifOption();

	}

	AddTarifOption()
	{
		tarif.AddTarif();

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

				document.GenerateDocuments();

				tarif.Jump2TarifSite();

				tarif.RemoveExistTariffs();

				tarif.AddTarif();

			});
			
		}
	}


	CheckRKResult()
	{
		    testLib.WaitUntil();
			
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






