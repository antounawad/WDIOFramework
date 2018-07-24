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

var _ErrorList = [100];
var _ErrorCounter = 0;


class RK{

	StartRKTest()
	{
		vn.AddVN('AutomRKTestVN',true);

		vp.AddVP('AutomRKTestVP');

		this.CreateTarifOptions();

		if(!testLib.BreakAtError)
		{
			if(_ErrorList.length > 1)
			{
				console.log("Fehler beim RK Test");
				_ErrorList.forEach(function(value, index) 
				{
					console.log(value)
				});
				  
				//throw new Error("Fehler beim RK Test, siehe vorige Logs...")
				  
			}

		}		
	}

	CreateTarifOptions()
	{
		tarif.DeleteAllTarife(true);


		if(testLib.Versicherer != null && testLib.SmokeTest)
		{
			var iterateArr = testLib.Versicherer;
			if(testLib.AllVersicherer)
			{
				var onlineTarife = tarif.GetAllTarife();
				onlineTarife.forEach(versicherer => {

					try
					{
						this.Navigate2RK(versicherer);
					}
					catch(ex)
					{
						
					   this.ErrorFunction(ex);		
					}
				 
				   });
				
			}
			else
			{
				testLib.Versicherer.forEach(versicherer => {
			
					try
					{
						var specialTarif = testLib.CheckFieldAttribute('Tarif',versicherer);
						var specialDurchfWeg = testLib.CheckFieldAttribute('DurchfWeg',versicherer); 

						this.Navigate2RK(versicherer['Id'][0],specialTarif, specialDurchfWeg);
					}
					catch(ex)
					{
						
					   this.ErrorFunction(ex);		
					}
					
					
			   });
	   
			}


		}
	}

	ErrorFunction(ex)
	{
		if(!testLib.BreakAtError)
		{
			_ErrorList[_ErrorCounter++] = ex.message;
			tarif.DeleteAllTarife(true);	
		}
		else
		{
			throw new Error(ex);
		}		
	}


	CheckRKResult()
	{
		    testLib.WaitUntilVisible(testLib.BtnNavNext,100000);
			
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


	Navigate2RK(versicherer,specialTarif='',specialDurchfWeg='')
	{
		try
		{
			tarif.CreateTarif(versicherer,specialTarif,specialDurchfWeg);

			testLib.Navigate2Site('Beratungsübersicht');
		
			consultation.AddConsultation();

			testLib.Navigate2Site('Auswertung – Rendite')

			this.CheckRKResult();

			document.GenerateDocuments();
				
				//testLib.Next(500);

			tarif.DeleteAllTarife(true);	
		}
		catch(ex)
		{
			throw new Error(ex);
		}
	}

}
module.exports = RK;






