var TestLib = require('../Lib/ClassLib.js')
const testLib = new TestLib();
var Tarif = require('../Lib/Tarif.js')
const tarif = new Tarif()
var _crlf = '\r\n';

class RK {

	StartRKTest(vn, vp)
	{
		vn.AddVN(testLib.VnName, true);
		vp.AddVP(testLib.VpName);

		testLib.LogTime('Start RK Test...' + _crlf);
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

			if (testLib.SmokeTest) {
				tarif.CreateSmokeTarif(versicherer);
				tarif.CheckAngebot(vArr.length != i + 1, testLib.OnlyTarifCheck);
			}
			else {
				tarif.CreateListTarif(versicherer, vArr.length != i + 1);
			}
			tarif.ResultArr[tarif.ResultCounter] = versicherer;
			console.log("Versicherer: " + String(versicherer) + " erfolgreich durchlaufen" + _crlf);

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






