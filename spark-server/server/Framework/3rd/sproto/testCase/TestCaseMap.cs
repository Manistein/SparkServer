using System;
using Sproto;
using SprotoType;
using System.Collections.Generic;

namespace sprotoCsharp
{
	public class TestCaseMap : TestCaseBase
	{
		public TestCaseMap ()
		{
		}

		static void assert(MetaData.Info a, MetaData.Info b) {
			assert (a.v1 == b.v1);
			assert (a.v2 == b.v2);
			assert (a.v3 == b.v3);
		}

		public override void run() {
			SprotoType.MetaData m = new SprotoType.MetaData ();

			List<MetaData.Info> array = new List<MetaData.Info> ();
			m.a1 = array;

			MetaData.Info i1 = new MetaData.Info ();
			i1.v1 = "name1";
			i1.v2 = false;
			i1.v3 = 1000;

			MetaData.Info i2 = new MetaData.Info ();
			i2.v1 = "name2";
			i2.v2 = true;
			i2.v3 = 2000;

			MetaData.Info i3 = new MetaData.Info ();
			i3.v1 = "name3";
			i3.v2 = false;
			i3.v3 = 3000;

			array.Add (i1);
			array.Add (i2);
			array.Add (i3);

			Dictionary<Int64, MetaData.Info> map1 = new Dictionary<Int64, MetaData.Info> ();
			map1.Add (i1.v3, i1);
			map1.Add (i2.v3, i2);
			map1.Add (i3.v3, i3);
			m.d3 = map1;

			Dictionary<string, MetaData.Info> map2 = new Dictionary<string, MetaData.Info> ();
			map2.Add (i1.v1, i1);
			map2.Add (i2.v1, i2);
			map2.Add (i3.v1, i3);
			m.d1 = map2;

			Dictionary<bool, MetaData.Info> map3 = new Dictionary<bool, MetaData.Info> ();
//			map3.Add (i1.v2, i1);
			map3.Add (i2.v2, i2);
			map3.Add (i3.v2, i3);
			m.d2 = map3;

			byte[] data = m.encode ();
			for (int i = 0; i < 2; i++) {
				MetaData o = new MetaData(data);
				assert (o.a1.Count == 3);
				assert (o.a1 [0], i1);
				assert (o.a1 [1], i2);
				assert (o.a1 [2], i3);

				assert (o.d1.Count == 3);
				assert (o.d1 [i1.v1] , i1);
				assert (o.d1 [i2.v1] , i2);
				assert (o.d1 [i3.v1] , i3);

				assert (o.d2.Count == 2);
				assert (o.d2 [i2.v2] , i2);
				assert (o.d2 [i3.v2] , i3);

				assert (o.d3.Count == 3);
				assert (o.d3 [i1.v3] , i1);
				assert (o.d3 [i2.v3] , i2);
				assert (o.d3 [i3.v3] , i3);

				data = new byte[] {
					0X05, 0X00, 0X03, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X45,
					0X00, 0X00, 0X00, 0X13, 0X00, 0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X00, 0X00,
					0X02, 0X00, 0Xd2, 0X07, 0X05, 0X00, 0X00, 0X00, 0X6e, 0X61, 0X6d, 0X65, 0X31,
					0X13, 0X00, 0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X00, 0X00, 0X04, 0X00, 0Xa2,
					0X0f, 0X05, 0X00, 0X00, 0X00, 0X6e, 0X61, 0X6d, 0X65, 0X32, 0X13, 0X00, 0X00,
					0X00, 0X04, 0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0X72, 0X17, 0X05, 0X00,
					0X00, 0X00, 0X6e, 0X61, 0X6d, 0X65, 0X33, 0X2e, 0X00, 0X00, 0X00, 0X13, 0X00,
					0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0X72, 0X17, 0X05,
					0X00, 0X00, 0X00, 0X6e, 0X61, 0X6d, 0X65, 0X33, 0X13, 0X00, 0X00, 0X00, 0X04,
					0X00, 0X01, 0X00, 0X00, 0X00, 0X04, 0X00, 0Xa2, 0X0f, 0X05, 0X00, 0X00, 0X00,
					0X6e, 0X61, 0X6d, 0X65, 0X32, 0X45, 0X00, 0X00, 0X00, 0X13, 0X00, 0X00, 0X00,
					0X04, 0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0Xd2, 0X07, 0X05, 0X00, 0X00,
					0X00, 0X6e, 0X61, 0X6d, 0X65, 0X31, 0X13, 0X00, 0X00, 0X00, 0X04, 0X00, 0X01,
					0X00, 0X00, 0X00, 0X02, 0X00, 0X72, 0X17, 0X05, 0X00, 0X00, 0X00, 0X6e, 0X61,
					0X6d, 0X65, 0X33, 0X13, 0X00, 0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X00, 0X00,
					0X04, 0X00, 0Xa2, 0X0f, 0X05, 0X00, 0X00, 0X00, 0X6e, 0X61, 0X6d, 0X65, 0X32,
					0X45, 0X00, 0X00, 0X00, 0X13, 0X00, 0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X00,
					0X00, 0X02, 0X00, 0Xd2, 0X07, 0X05, 0X00, 0X00, 0X00, 0X6e, 0X61, 0X6d, 0X65,
					0X31, 0X13, 0X00, 0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X00, 0X00, 0X04, 0X00,
					0Xa2, 0X0f, 0X05, 0X00, 0X00, 0X00, 0X6e, 0X61, 0X6d, 0X65, 0X32, 0X13, 0X00,
					0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0X72, 0X17, 0X05,
					0X00, 0X00, 0X00, 0X6e, 0X61, 0X6d, 0X65, 0X33,
				};
			}
		}
	}
}

