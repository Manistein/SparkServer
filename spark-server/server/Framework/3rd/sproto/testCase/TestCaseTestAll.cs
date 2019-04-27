using System;
using Sproto;
using SprotoType;
using System.Collections.Generic;

namespace sprotoCsharp
{
	public class TestCaseTestAll : TestCaseBase
	{
		public TestCaseTestAll ()
		{
		}


		public override void run() {
			Foobar obj = new Foobar ();
			obj.a = "hello";
			obj.b = 1000000;
			obj.c = true;

			obj.d = new Foobar.Nest ();
			obj.d.a = "world";
			obj.d.c = -1;

			obj.e = new System.Collections.Generic.List<string> {"ABC", "def"};
			obj.f = new System.Collections.Generic.List<long> { -3, -2, -1, 0 , 1, 2};
			obj.g = new System.Collections.Generic.List<bool> { true, false, true};

			obj.h = new System.Collections.Generic.List<Foobar> ();

			Foobar tmp = new Foobar ();
			tmp.b = 100;
			obj.h.Add (tmp);

			obj.h.Add (new Foobar ());

			tmp = new Foobar ();
			tmp.b = -100;
			tmp.c = false;
			obj.h.Add (tmp);

			tmp = new Foobar ();
			tmp.b = 0;
			tmp.e = new System.Collections.Generic.List<string> { "test" };
			obj.h.Add (tmp);

			byte[] data = obj.encode ();
			byte[] result_data = new byte[] {
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
			assert (data, result_data);

			Sproto.SprotoPack spack = new SprotoPack ();

			byte[] pack_data = spack.pack (data);
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
			assert (pack_data, result_pack_data);

			byte[] unpack_data = spack.unpack (pack_data);
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
			assert (unpack_data, result_unpack_data);

			Foobar foo = new Foobar (unpack_data);
			assert (foo.a == "hello");
			assert (foo.b == 1000000);
			assert (foo.c == true);
			assert (foo.d.a == "world");
			assert (foo.d.c == -1);
			assert (foo.e [0] == "ABC");
			assert (foo.e [1] == "def");
			assert (foo.f, new Int64[] { -3, -2, -1, 0 , 1, 2});
			assert (foo.g, new bool[] {true, false, true });
			assert (foo.h [0].b == 100);
			assert (foo.h [2].b == -100);
			assert (foo.h [2].c == false);
			assert (foo.h [3].b == 0);
			assert (foo.h [3].e [0] == "test");

			// test extend proto

			byte[] extend_data = new byte[] {
				0X0a, 0X00, 0X00, 0X00, 0X00, 0X00, 0X04, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00,
				0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X70, 0X4a, 0X05, 0X00, 0X00, 0X00,
				0X68, 0X65, 0X6c, 0X6c, 0X6f, 0X04, 0X00, 0X00, 0X00, 0X40, 0X42, 0X0f, 0X00,
				0X1b, 0X00, 0X00, 0X00, 0X04, 0X00, 0X01, 0X00, 0X00, 0X00, 0X05, 0X00, 0X00,
				0X00, 0X05, 0X00, 0X00, 0X00, 0X77, 0X6f, 0X72, 0X6c, 0X64, 0X04, 0X00, 0X00,
				0X00, 0Xff, 0Xff, 0Xff, 0Xff, 0X0e, 0X00, 0X00, 0X00, 0X03, 0X00, 0X00, 0X00,
				0X41, 0X42, 0X43, 0X03, 0X00, 0X00, 0X00, 0X64, 0X65, 0X66, 0X19, 0X00, 0X00,
				0X00, 0X04, 0Xfd, 0Xff, 0Xff, 0Xff, 0Xfe, 0Xff, 0Xff, 0Xff, 0Xff, 0Xff, 0Xff,
				0Xff, 0X00, 0X00, 0X00, 0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0X00, 0X00,
				0X03, 0X00, 0X00, 0X00, 0X01, 0X00, 0X01, 0X3e, 0X00, 0X00, 0X00, 0X06, 0X00,
				0X00, 0X00, 0X02, 0X00, 0X01, 0X00, 0Xca, 0X00, 0X02, 0X00, 0X00, 0X00, 0X00,
				0X00, 0X10, 0X00, 0X00, 0X00, 0X03, 0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00,
				0X04, 0X00, 0X00, 0X00, 0X9c, 0Xff, 0Xff, 0Xff, 0X16, 0X00, 0X00, 0X00, 0X04,
				0X00, 0X01, 0X00, 0X02, 0X00, 0X03, 0X00, 0X00, 0X00, 0X08, 0X00, 0X00, 0X00,
				0X04, 0X00, 0X00, 0X00, 0X74, 0X65, 0X73, 0X74, 0X05, 0X00, 0X00, 0X00, 0X7a,
				0X69, 0X78, 0X75, 0X6e, 0x95, 0x27
			};

			long size =  foo.init (extend_data);
			Console.WriteLine ("szie: " + size);
		}
	}
}

