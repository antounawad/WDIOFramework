var TestLib = require('../Lib/ClassLib.js')
var assert = require('assert');
const testLib = new TestLib();
// var VN = require('../Lib/VN.js')
// const vn = new VN()
// var VP = require('../Lib/VP.js')
// const vp = new VP()
var Consultation = require('../Lib/Consultation.js')
const consultation = new Consultation()
var Tarif = require('../Lib/Tarif.js')
const tarif = new Tarif()
var Document = require('../Lib/Document.js')
const document = new Document();
var _ErrorList = [1000];
var _ErrorCounter = 0;
var _RepeatCounter = 0;




class RK {

	StartRKTest(vn, vp) {
		try
		{
			testLib.LogTime('Start RK Test...');
			vn.AddVN(testLib.VnName, true);

			vp.AddVP(testLib.VpName);

			this.CreateTarifOptions();
			testLib.LogTime('Ende RK Test');
		}catch(ex)
		{
			console.log("Error: StartRKTest: "+ex.message);

			if(String(ex.message).indexOf('AssertionError') >= 0)
				throw new Error(ex);

			_RepeatCounter++

			tarif.CancelTarif();

			this.CreateTarifOptions();
			if(_RepeatCounter >= 10)
			{
				console.log("Fehler: Nach 10 maliger Wiederholung abgebrochen...")
				assert.equal(1,0,ex.message);
			}
		}
	}

	CreateTarifOptions() {
		tarif.DeleteAllTarife(true);

		this.Navigate2RK();
	}

	ErrorFunction(message) {
		if (!testLib.BreakAtError) {
			console.log(message);
			_ErrorList[_ErrorCounter] = message+' Bild: '+String(_ErrorCounter+'.png');
			testLib.TakeErrorShot(String(_ErrorCounter)+'.png');
			tarif.DeleteAllTarife(true);
			_ErrorCounter += 1;
		}
		else {
			console.log("BreakAtError = false; Fehler:")
			assert.equal(1,0,message);
		}
	}



	Navigate2RK(versicherer) {
		try {

			var vArr = this.GetVersichererArray();
			for(var i = 0; i <= vArr.length-1; i++)
			{
				versicherer = vArr[i];

				testLib.CurrentID = versicherer;

				try {
					if (testLib.SmokeTest) {
						tarif.CreateSmokeTarif(versicherer);
						tarif.CheckAngebot(vArr.length != i+1,testLib.OnlyTarifCheck);
					}
					else {
						tarif.CreateListTarif(versicherer,vArr.length != i+1);
					}
					tarif.ResultArr[tarif.ResultCounter] = versicherer;
					console.log("Versicherer: "+String(versicherer)+" erfolgreich durchlaufen");
				}
				catch (ex) {
					var message = 'Versicherer: '+versicherer+' ' + ex.message;
					this.ErrorFunction(message);
					if(ex.message.indexOf('Fehler bei Angebotserstellung') == -1)
					{
						throw new Error(ex);
					}
				}
				
			};
		}
		catch (ex) {
			console.log("Error: Navigate2RK: "+ex.message);
			throw new Error(ex);
		}
	}



	GetVersichererArray() {
		
		if (!testLib.AllVersicherer) {
			var versicherArr = [testLib.Versicherer.length];
			testLib.Versicherer.forEach(function (value, index) {
				versicherArr[index] = value['Id'][0];
			});

			return versicherArr;
		}

		return tarif.GetAllVersicherer();
	}

}
module.exports = RK;






