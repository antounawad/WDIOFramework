var TestLib = require('C:/git/shared/QA_Tests/TestSuiteLib/ClassLib.js')
const testLib = new TestLib();
var Tarif = require('C:/git/shared/QA_Tests/TestSuiteLib/Tarif.js')
const tarif = new Tarif()
var Document = require('C:/git/shared/QA_Tests/TestSuiteLib/Document.js')

class RK {

	StartRKTest(vn, vp) {
		testLib.AddChapter(vn,vp);
		testLib.LogTime('Start RK Test...');
		this.CreateTarifOptions();
		testLib.LogTime('Ende RK Test');
	}

	CreateTarifOptions() {
		tarif.DeleteAllTarife(true);
		this.Navigate2RK();
	}
	Navigate2RK(versicherer) {

		var vArr = this.GetVersichererArray();
		if(vArr === '1048')
		{
			testLib.CurrentID = vArr;
			tarif.CreateListTarif(vArr, false);
			tarif.ResultArr[tarif.ResultCounter] = versicherer;
			console.log("Versicherer: " + String(vArr) + " erfolgreich durchlaufen");

		}
		else
		{
			for (var i = 0; i <= vArr.length - 1; i++) {
				versicherer = vArr[i];
				
				testLib.CurrentID = versicherer;
				tarif.CreateListTarif(versicherer, vArr.length != i + 1);
				tarif.ResultArr[tarif.ResultCounter] = versicherer;
				console.log("Versicherer: " + String(versicherer) + " erfolgreich durchlaufen");
			};
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






