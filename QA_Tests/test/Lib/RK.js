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
		vn.CheckVN('AutomRKTestVN',true);

		vp.CheckVP('AutomRKTestVP');

		this.CreateTarifOptions();
	}

	CreateTarifOptions()
	{
		tarif.DeleteAllTarife(true);

		//tarif.GetAllTarife();

		if(testLib.Versicherer != null && testLib.SmokeTest)
		{
			testLib.Versicherer.forEach(versicherer => {

			 tarif.CreateTarif(versicherer)
			 
			testLib.Navigate2Site('Beratungsübersicht');

			consultation.AddConsultation();

			//testLib.Navigate2Site('Angebot – Angebotsdaten');
			
			testLib.Navigate2Site('Angebot – Kurzübersicht')

			this.CheckRKResult();

				document.GenerateDocuments();
				
				testLib.Next(500);

			tarif.DeleteAllTarife(true);	
			});
		}
	}


	CheckRKResult()
	{
		    testLib.WaitUntil(testLib.BtnNavNext,100000);
			
			var errorBlock = $("md-card[ng-show='HasErrorMessages']");
	
			if(errorBlock !== undefined)
			{
				assert.notEqual(errorBlock.getAttribute('class').indexOf('ng-hide'), -1, 'Fehler bei Angebotserstellung für Tarif: ' + browser.getText("span[class='label-tarif']")+ browser.getText("div[class='label-details']"));
			}
			else
			{
				assert.equal(0, 1, 'Rechenkernseite prüfen');
			} 

	}

}
module.exports = RK;






