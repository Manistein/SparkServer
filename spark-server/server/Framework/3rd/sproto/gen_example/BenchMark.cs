using System;
using SprotoType;
using Sproto;

namespace sprotoCsharp
{
	public class BenchMark
	{
		public BenchMark ()
		{

			AddressBook address = new AddressBook ();
			address.person = new System.Collections.Generic.List<Person> ();

			Person person = new Person ();
			person.name = "Alice";
			person.id = 10000;

			person.phone = new System.Collections.Generic.List<Person.PhoneNumber> ();
			Person.PhoneNumber num1 = new Person.PhoneNumber ();
			num1.number = "123456789";
			num1.type = 1;
			person.phone.Add (num1);

			Person.PhoneNumber num2 = new Person.PhoneNumber ();
			num2.number = "87654321";
			num2.type = 2;
			person.phone.Add (num2);

			address.person.Add (person);

			Person person1 = new Person ();
			person1.name = "Bob";
			person1.id = 20000;
			person1.phone = new System.Collections.Generic.List<Person.PhoneNumber> ();
			Person.PhoneNumber num3 = new Person.PhoneNumber ();
			num3.number = "01234567890";
			num3.type = 3;
			person1.phone.Add (num3);

			address.person.Add (person1);

			byte[] data = address.encode ();

			Sproto.SprotoPack spack = new Sproto.SprotoPack ();

//			byte[] pack_data = spack.pack (data);

			Sproto.SprotoStream stream = new SprotoStream ();
			double b = this.cur_mseconds ();
			for (int i = 0; i < 1000000; i++) {
				address.init (data);
//				int len = address.encode (stream);
//				stream.Seek (0, System.IO.SeekOrigin.Begin);
//				spack.pack (stream.Buffer, len);

//				byte[] unpack_data = spack.unpack (pack_data);
//				address.init (unpack_data);
			}
			double e = this.cur_mseconds ();
			Console.WriteLine ("total: " + (e - b)/1000  +"s");
		}


		double cur_mseconds() {
			TimeSpan ts = DateTime.Now - new DateTime(1960, 1, 1);
			return ts.TotalMilliseconds;
		}
	}
}

