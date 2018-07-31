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




class RK {

	StartRKTest(vn, vp) {
		try
		{
			vn.AddVN('AutomRKTestVN', true);

			vp.AddVP('AutomRKTestVP');

			this.CreateTarifOptions();
		}catch(ex)
		{
			this.CreateTarifOptions();
		}
	}

	CreateTarifOptions() {
		tarif.DeleteAllTarife(true);

		this.Navigate2RK();
	}

	ErrorFunction(message) {
		if (!testLib.BreakAtError) {
			console.log(message);
			_ErrorList[_ErrorCounter++] = ex.message;
			tarif.DeleteAllTarife(true);
		}
		else {
			console.log("BreakAtError = false; Fehler:")
			assert.equal(1,0,ex.message);
		}
	}



	Navigate2RK(versicherer) {
		try {

			var vArr = this.GetVersichererArray();
			for(var i = 0; i <= vArr.length-1; i++)
			{
				versicherer = vArr[i];

				try {
					if (testLib.SmokeTest) {
						tarif.CreateSmokeTarif(versicherer);
						tarif.CheckAngebot(vArr.length != i+1);
					}
					else {
						tarif.CreateListTarif(versicherer,vArr.length != i+1);
					}
					tarif.ResultArr[tarif.ResultCounter] = versicherer;
				}
				catch (ex) {
					var message = 'Versicherer: '+versicherer+' ' + ex.message;
					this.ErrorFunction(message);
				}
				
			};
		}
		catch (ex) {
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






