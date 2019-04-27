using System;
using Sproto;

namespace sprotoCsharp
{
	public class TestCaseSprotoPack : TestCaseBase
	{
		public TestCaseSprotoPack ()
		{
		}

		public override void run() {
			SprotoPack extract = new SprotoPack ();

			byte[] data = new byte[] {
				0X08, 0X00, 0X00, 0X00, 0X00, 0X00, 0X04, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00,
				0X00, 0X00, 0X00, 0X00, 0X00, 0X05, 0X00, 0X00, 0X00, 0X68, 0X65, 0X6c, 0X6c,
				0X6f, 0X04, 0X00, 0X00, 0X00, 0X40, 0X42, 0X0f, 0X00, 0X1b, 0X00, 0X00, 0X00,
				0X04, 0X00, 0X01, 0X00, 0X00, 0X00, 0X05, 0X00, 0X00, 0X00, 0X05, 0X00, 0X00,
				0X00, 0X77, 0X6f, 0X72, 0X6c, 0X64, 0X04, 0X00, 0X00, 0X00, 0Xff, 0Xff, 0Xff,
				0Xff, 0X0e, 0X00, 0X00, 0X00, 0X03, 0X00, 0X00, 0X00, 0X41, 0X42, 0X43, 0X03,
				0X00, 0X00, 0X00, 0X64, 0X65, 0X66, 0X19, 0X00, 0X00, 0X00, 0X04, 0Xfd, 0Xff,
				0Xff, 0Xff, 0Xfe, 0Xff, 0Xff, 0Xff, 0Xff, 0Xff, 0Xff, 0Xff, 0X00, 0X00, 0X00,
				0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0X00, 0X00, 0X03, 0X00, 0X00, 0X00,
				0X01, 0X00, 0X01, 0X3e, 0X00, 0X00, 0X00, 0X06, 0X00, 0X00, 0X00, 0X02, 0X00,
				0X01, 0X00, 0Xca, 0X00, 0X02, 0X00, 0X00, 0X00, 0X00, 0X00, 0X10, 0X00, 0X00,
				0X00, 0X03, 0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0X04, 0X00, 0X00, 0X00,
				0X9c, 0Xff, 0Xff, 0Xff, 0X16, 0X00, 0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X02,
				0X00, 0X03, 0X00, 0X00, 0X00, 0X08, 0X00, 0X00, 0X00, 0X04, 0X00, 0X00, 0X00,
				0X74, 0X65, 0X73, 0X74,
			};

			byte[] result_pack_data = new byte[] {
				0X41, 0X08, 0X04, 0X00, 0Xc4, 0X05, 0X68, 0X65, 0X8f, 0X6c, 0X6c, 0X6f, 0X04,
				0X40, 0X8b, 0X42, 0X0f, 0X1b, 0X04, 0X22, 0X01, 0X05, 0Xe2, 0X05, 0X77, 0X6f,
				0X72, 0Xc7, 0X6c, 0X64, 0X04, 0Xff, 0Xff, 0X47, 0Xff, 0Xff, 0X0e, 0X03, 0X3c,
				0X41, 0X42, 0X43, 0X03, 0X1e, 0X64, 0X65, 0X66, 0X19, 0Xff, 0X00, 0X04, 0Xfd,
				0Xff, 0Xff, 0Xff, 0Xfe, 0Xff, 0Xff, 0X1f, 0Xff, 0Xff, 0Xff, 0Xff, 0Xff, 0X22,
				0X01, 0X02, 0Xa2, 0X03, 0X01, 0X01, 0X11, 0X3e, 0X06, 0X55, 0X02, 0X01, 0Xca,
				0X02, 0X10, 0X10, 0X45, 0X03, 0X01, 0X02, 0Xf1, 0X04, 0X9c, 0Xff, 0Xff, 0Xff,
				0X51, 0X16, 0X04, 0X01, 0X45, 0X02, 0X03, 0X08, 0Xc4, 0X04, 0X74, 0X65, 0X03,
				0X73, 0X74,

			};

			byte[] result_unpack_data = new byte[] {
				0X08, 0X00, 0X00, 0X00, 0X00, 0X00, 0X04, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00,
				0X00, 0X00, 0X00, 0X00, 0X00, 0X05, 0X00, 0X00, 0X00, 0X68, 0X65, 0X6c, 0X6c,
				0X6f, 0X04, 0X00, 0X00, 0X00, 0X40, 0X42, 0X0f, 0X00, 0X1b, 0X00, 0X00, 0X00,
				0X04, 0X00, 0X01, 0X00, 0X00, 0X00, 0X05, 0X00, 0X00, 0X00, 0X05, 0X00, 0X00,
				0X00, 0X77, 0X6f, 0X72, 0X6c, 0X64, 0X04, 0X00, 0X00, 0X00, 0Xff, 0Xff, 0Xff,
				0Xff, 0X0e, 0X00, 0X00, 0X00, 0X03, 0X00, 0X00, 0X00, 0X41, 0X42, 0X43, 0X03,
				0X00, 0X00, 0X00, 0X64, 0X65, 0X66, 0X19, 0X00, 0X00, 0X00, 0X04, 0Xfd, 0Xff,
				0Xff, 0Xff, 0Xfe, 0Xff, 0Xff, 0Xff, 0Xff, 0Xff, 0Xff, 0Xff, 0X00, 0X00, 0X00,
				0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0X00, 0X00, 0X03, 0X00, 0X00, 0X00,
				0X01, 0X00, 0X01, 0X3e, 0X00, 0X00, 0X00, 0X06, 0X00, 0X00, 0X00, 0X02, 0X00,
				0X01, 0X00, 0Xca, 0X00, 0X02, 0X00, 0X00, 0X00, 0X00, 0X00, 0X10, 0X00, 0X00,
				0X00, 0X03, 0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0X04, 0X00, 0X00, 0X00,
				0X9c, 0Xff, 0Xff, 0Xff, 0X16, 0X00, 0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X02,
				0X00, 0X03, 0X00, 0X00, 0X00, 0X08, 0X00, 0X00, 0X00, 0X04, 0X00, 0X00, 0X00,
				0X74, 0X65, 0X73, 0X74, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00,
			};

			byte[] pack_data = extract.pack (data);
			dump_bytes (pack_data);
			assert (pack_data, result_pack_data);

			byte[] unpack_data = extract.unpack (pack_data);
			assert (unpack_data, result_unpack_data);

			byte[] data2 = new byte[] {
				1, 2, 3, 4, 5, 6, 7, 8,
				1, 2, 3, 4, 5, 0, 7, 8,
				1, 2, 3, 4, 5, 0,
			};

			Console.WriteLine ("======pack=======");

			byte[] pack_data1 = extract.pack (data2);
			dump_bytes (pack_data1);

			Console.WriteLine ("=======unpack======");
			byte[] unpack_data1 = extract.unpack (pack_data1);
			dump_bytes (unpack_data1);
		}
	}
}

