using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;

namespace Alosha
{
	public class ConfigAlosha : IRocketPluginConfiguration, IDefaultable
	{
		public void LoadDefaults()
		{
			this.ölümSüresi = 300f;
			this.bertarafEdilecekYetki = "alosha.olebilir";
			this.canlandirmaYetkisi = "alosha.canlandir";
			this.doktorYetkisi = "alosha.doktor";
			this.doktorCanlandirmaSüresi = 5f;
			this.yaralıGörünümüMesafesi = 5f;
			this.taşımaYetkisi = "alosha.taşıma";
			this.kaçCandaDüşsün = 20f;
			this.yereDüştüğündeKaçCanıOlsun = 15;
			this.kalktığındaKaçCanıOlsun = 10;
			this.canlandırmaSüresi = 10f;
			this.neKadarMesafesiOlsun = 5f;
			this.gelişmişSunucu = false;
			this.yereDüştüğündeBacakKırıl = false;
			this.yereDüştüğündeKanamaDurdur = true;
			this.yereDüştüğündeKanamaBaşlat = false;
			this.Yerde_Vur_Ölsün = false;
			this.elindeSilahVarsaDüşsün = true;
			this.araçlaEzdiğindeÖl = true;
			this.füzeAttığındaÖl = true;
			this.buSilahlarKesinÖldürsün = new List<ushort>
			{
				18,
				297
			};
		}

		public float ölümSüresi;

		public string bertarafEdilecekYetki;

		public string canlandirmaYetkisi;

		public string doktorYetkisi;

		public string taşımaYetkisi;

		public float kaçCandaDüşsün;

		public float yaralıGörünümüMesafesi;

		public float canlandırmaSüresi;

		public float doktorCanlandirmaSüresi;

		public float neKadarMesafesiOlsun;

		public byte yereDüştüğündeKaçCanıOlsun;

		public byte kalktığındaKaçCanıOlsun;

		public bool yereDüştüğündeBacakKırıl;

		public bool gelişmişSunucu;

		public bool yereDüştüğündeKanamaDurdur;

		public bool yereDüştüğündeKanamaBaşlat;

		public bool Yerde_Vur_Ölsün;

		public bool araçlaEzdiğindeÖl;

		public bool füzeAttığındaÖl;

		public bool elindeSilahVarsaDüşsün;

		[XmlArray("Silahlar")]
		[XmlArrayItem("Silah")]
		public List<ushort> buSilahlarKesinÖldürsün = new List<ushort>();
	}
}
