var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Tarif = require('../Lib/Tarif.js')
const tarif = new Tarif()
var Document = require('../Lib/Document.js')

class RK {

	StartRKTest(vn, vp) {
		testLib.LogTime('Start RK Test...');
		vn.AddVN(testLib.VnName, true);
		vp.AddVP(testLib.VpName);
		this.CreateTarifOptions();
		testLib.LogTime('Ende RK Test');
	}

	CreateTarifOptions() {
		tarif.DeleteAllTarife(true);
		this.Navigate2RK();
	}
	Navigate2RK(versicherer) {

		var vArr = this.GetVersichererArray();
		for (var i = 0; i <= vArr.length - 1; i++) {
			versicherer = vArr[i];

			testLib.CurrentID = versicherer;
			tarif.CreateListTarif(versicherer, vArr.length != i + 1);
			tarif.ResultArr[tarif.ResultCounter] = versicherer;
			console.log("Versicherer: " + String(versicherer) + " erfolgreich durchlaufen");
		};
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






