using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace sprotoCsharp
{
	public abstract class TestCaseBase
	{
		public TestCaseBase ()
		{
		}

		abstract public void run();
	
		public static void assert(bool condition){
			if (!condition)
				throw new Exception ();
		}

		public static void assert(byte[] a, byte[] b) {
			assert (a.Length == b.Length);
			for (int i = 0; i < a.Length; i++) {
				assert (a [i] == b [i]);
			}
		}

		public static void assert(List<Int64> a, Int64[] b) {
			assert (a.Count == b.Length);
			for (int i = 0; i < a.Count; i++) {
				assert (a[i] == b[i]);
			}
		}


		public static void assert(List<string> a, string[] b) {
			assert (a.Count == b.Length);
			for (int i = 0; i < a.Count; i++) {
				assert (a[i] == b[i]);
			}
		}

		public static void assert(List<byte[]> a, List<byte[]> b) {
			assert (a.Count == b.Count);
			for (int i = 0; i < a.Count; i++) {
				assert (a [i], b [i]);
			}
		}

		public static void assert(List<double> a, List<double> b) {
			assert (a.Count == b.Count);
			for (int i = 0; i < a.Count; i++) {
				assert (a [i] == b [i]);
			}
		}


		public static void assert(List<bool> a, bool[] b) {
			assert (a.Count == b.Length);
			for (int i = 0; i < a.Count; i++) {
				assert (a[i] == b[i]);
			}
		}


		public static void dump_bytes(byte[] data) {
			string s = "\nlen: " + data.Length + "\n";
			foreach(byte v in data) {
				s = s + String.Format ("{0:X}  ", v);
			}

			Console.WriteLine (s);
		}

		public static void dump_bytes(UInt32[] data) {
			string s = "\nlen: " + data.Length + "\n";
			foreach (UInt32 v in data) {
				s = s + String.Format ("{0:x8}  ", v);
			}

			Console.WriteLine (s);
		}

		public static void dump_list(List<Int64> array){
			string s = "\nlen: " + array.Count + "\n";
			foreach (Int64 v in array) {
				s = s + v + " ";
			}

			Console.WriteLine (s);
		}

		public static void dump_list(List<string> array){
			string s = "\nlen: " + array.Count + "\n";
			foreach (string v in array) {
				s = s + v + " ";
			}

			Console.WriteLine (s);
		}

		public static void dump_list(List<bool> array){
			string s = "\nlen: " + array.Count + "\n";
			foreach (bool v in array) {
				s = s + v + " ";
			}

			Console.WriteLine (s);
		}
	}
}

