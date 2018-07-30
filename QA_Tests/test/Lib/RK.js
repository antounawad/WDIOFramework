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

var _ErrorList = [100];
var _ErrorCounter = 0;


class RK {

	StartRKTest(vn, vp) {
		vn.AddVN('AutomRKTestVN', true);

		vp.AddVP('AutomRKTestVP');

		this.CreateTarifOptions();

		if (!testLib.BreakAtError) {
			if (_ErrorList.length > 1) {
				console.log("Fehler beim RK Test");
				_ErrorList.forEach(function (value, index) {
					console.log(value)
				});

				//throw new Error("Fehler beim RK Test, siehe vorige Logs...")

			}

		}
	}

	CreateTarifOptions() {
		tarif.DeleteAllTarife(true);

		this.Navigate2RK();
	}

	ErrorFunction(ex) {
		if (!testLib.BreakAtError) {
			_ErrorList[_ErrorCounter++] = ex.message;
			tarif.DeleteAllTarife(true);
		}
		else {
			throw new Error(ex);
		}
	}



	Navigate2RK(versicherer) {
		try {


			this.GetVersichererArray().forEach(versicherer => {

				try {
					if (testLib.SmokeTest) {
						tarif.CreateSmokeTarif(versicherer);
						tarif.CheckAngebot();
					}
					else {
						tarif.CreateListTarif(versicherer);
					}
				}
				catch (ex) {
					this.ErrorFunction(ex);
				}
				
			});
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






