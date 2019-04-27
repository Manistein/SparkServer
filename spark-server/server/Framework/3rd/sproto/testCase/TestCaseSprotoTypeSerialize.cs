using System;
using Sproto;
using System.Collections.Generic;

namespace sprotoCsharp
{
	public class TestCaseSprotoTypeSerialize : TestCaseBase
	{
		public TestCaseSprotoTypeSerialize ()
		{
		}

		private void test_field() {
			SprotoTypeSerialize serialize = new SprotoTypeSerialize (20);

			/*
			 * .Test {
			 *   var1 0: integer
			 * 	 var2 1: string
			 *   var3 5: intger
			 *   var4 7: boolean
			 * 	 var5 9: integer
			 * 	 var6 10: boolean
			 *   var7 12: intger
			 * }
			 * */

			byte[] test_result_data = {
				0X0b, 0X00, 0X00, 0X00, 0X00, 0X00, 0X05, 0X00, 0Xac, 0X88, 0X01, 0X00, 0X04, 
				0X00, 0X01, 0X00, 0X00, 0X00, 0X02, 0X00, 0X01, 0X00, 0X46, 0X22, 0X04, 0X00, 
				0X00, 0X00, 0Xde, 0Xff, 0Xff, 0Xff, 0X0b, 0X00, 0X00, 0X00, 0X74, 0X65, 0X73, 
				0X74, 0X5f, 0X73, 0X74, 0X72, 0X69, 0X6e, 0X67, 0X08, 0X00, 0X00, 0X00, 0X66, 
				0X55, 0X44, 0X33, 0X22, 0X11, 0X00, 0X00
			};

			SprotoStream stream = new SprotoStream ();
			serialize.open (stream);

			serialize.write_integer (-34, 0);
			serialize.write_string ("test_string", 1);
			serialize.write_integer (0x4455, 5);
			serialize.write_boolean (true, 7);
			serialize.write_integer (0x112233445566, 9);
			serialize.write_boolean (false, 10);
			serialize.write_integer (0x1122, 12);

			int len = serialize.close ();
			byte[] buffer = new byte[len];
			stream.Seek (0, System.IO.SeekOrigin.Begin);
			stream.Read (buffer, 0, len);

			Console.WriteLine ("======== encode buffer ===========");
			dump_bytes (buffer);
			assert(buffer, test_result_data);
		}

		private void test_array() {
			/*
			 * .Test {
			 * 	var1 0: *boolean
			 *  var2 4: *integer
			 *  var3 5: *string
			 * }
			 * */

			SprotoTypeSerialize serialize = new SprotoTypeSerialize (20);

			List<Int64> data = new List<Int64> ();
			data.Add (4);
			data.Add (0x1123);
			data.Add (0x1122334455);
			data.Add (-0x778899aabb);
			data.Add (-6);

			List<bool> b_data = new List<bool> ();
			b_data.Add (true);
			b_data.Add (false);
			b_data.Add (true);

			List<string> str_data = new List<string> ();
			str_data.Add ("中文显示");
			str_data.Add ("1234");
			str_data.Add ("fgcbvb");

			byte[] test_result_data = {
				0x04, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x29, 0x00, 0x00,
				0x00, 0x08, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x23, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x55, 0x44,
				0x33, 0x22, 0x11, 0x00, 0x00, 0x00, 0x45, 0x55, 0x66, 0x77,
				0x88, 0xff, 0xff, 0xff, 0xfa, 0xff, 0xff, 0xff, 0xff, 0xff,
				0xff, 0xff, 0x22, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x00, 0x00,
				0xe4, 0xb8, 0xad, 0xe6, 0x96, 0x87, 0xe6, 0x98, 0xbe, 0xe7,
				0xa4, 0xba, 0x04, 0x00, 0x00, 0x00, 0x31, 0x32, 0x33, 0x34,
				0x06, 0x00, 0x00, 0x00, 0x66, 0x67, 0x63, 0x62, 0x76, 0x62,
			};

			SprotoStream stream = new SprotoStream ();
			serialize.open (stream);

			serialize.write_boolean (b_data, 0);
			serialize.write_integer (data, 4);
			serialize.write_string (str_data, 5);

			int len = serialize.close ();
			byte[] buffer = new byte[len];
			stream.Seek (0, System.IO.SeekOrigin.Begin);
			stream.Read (buffer, 0, len);


			Console.Write ("====== array dump ========");
			dump_bytes (buffer);

			assert(buffer, test_result_data);
		}

		public override void run() {
			this.test_field();
			this.test_array ();
		}
	}
}

