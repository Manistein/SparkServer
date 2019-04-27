using System;
using Sproto;

namespace sprotoCsharp
{
	public class TestCaseSprotoTypeFieldOP : TestCaseBase
	{
		public TestCaseSprotoTypeFieldOP ()
		{
		}

		public override void run() {
			SprotoTypeFieldOP op = new SprotoTypeFieldOP (32 * 3);

			bool[] fileds = {
				true, false, true, false, true, false, true, false,  
				true, false, true, false, true, false, true, false,  
				false, false, true, false, true, false, true, false, 
				true, false, true, false, true, false, true, false, 

				true, false, true, false, true, false, true, false,
				false, false, true, false, true, false, true, false,
				true, false, true, false, true, false, true, false,
				true, false, true, false, true, false, true, false,

				true, false, true, false, true, false, true, false,
				true, false, true, false, true, false, true, false,
				true, false, true, false, true, false, true, false,
				false, false, true, false, true, false, true, false,
			};

			for (int i = 0; i < fileds.Length; i++) {
				bool v = fileds [i];
				op.set_field (i, v);
			}

			for (int i = 0; i < fileds.Length; i++) {
				bool v = fileds [i];
				assert (op.has_field(i) == v);
			}


			Console.WriteLine ("==========dump has_bit==========");
			dump_bytes (op.has_bits);
		}
	}
}

