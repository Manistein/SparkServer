using System;
using Sproto;


namespace sprotoCsharp
{
	public class TestCaseRpc : TestCaseBase
	{
		public TestCaseRpc ()
		{
		}

		public override void run() {
			SprotoRpc client = new SprotoRpc ();
			SprotoRpc service = new SprotoRpc (Protocol.Instance);
			SprotoRpc.RpcRequest clientRequest = client.Attach (Protocol.Instance);

			// ===============foobar=====================
			// request

			SprotoType.foobar.request obj = new SprotoType.foobar.request ();
			obj.what = "foo";
			byte[] req = clientRequest.Invoke<Protocol.foobar> (obj, 1);
			assert (req, new byte[] {0X55, 0X02, 0X04, 0X04, 0X01, 0Xc4, 0X03, 0X66, 0X6f, 0X01, 0X6f});

			// dispatch
			SprotoRpc.RpcInfo sinfo = service.Dispatch (req);
			assert (sinfo.type == SprotoRpc.RpcType.REQUEST);
			assert (sinfo.requestObj.GetType () == typeof(SprotoType.foobar.request));
			assert (sinfo.Response != null);
			SprotoType.foobar.request req_obj = (SprotoType.foobar.request)sinfo.requestObj;
			assert (req_obj.what == "foo");

			// response
			SprotoType.foobar.response obj2 = new SprotoType.foobar.response ();
			obj2.ok = true;
			byte[] resp = sinfo.Response (obj2);
			assert (resp, new byte[] {0X55, 0X02, 0X01, 0X04, 0X01, 0X01, 0X04});

			// dispatch
			sinfo = client.Dispatch (resp);
			assert (sinfo.type == SprotoRpc.RpcType.RESPONSE);
			assert (sinfo.session == 1);
			assert (((SprotoType.foobar.response)sinfo.responseObj).ok == true);
	
			// ================foo====================
			// request
			req =  clientRequest.Invoke<Protocol.foo> (null, 2);
			assert (req, new byte[] {0X15, 0X02, 0X06, 0X06});

			// dispatch
			sinfo = service.Dispatch (req);
			assert (sinfo.type == SprotoRpc.RpcType.REQUEST);
			assert (sinfo.tag == Protocol.foo.Tag);
			assert (sinfo.requestObj == null);

			// response
			SprotoType.foo.response obj3 = new SprotoType.foo.response();
			obj3.ok = false;
			resp = sinfo.Response (obj3);
			assert (resp, new byte[] {0X55, 0X02, 0X01, 0X06, 0X01, 0X01, 0X02});

			// dispatch
			sinfo = client.Dispatch (resp);
			assert (sinfo.type == SprotoRpc.RpcType.RESPONSE);
			assert (sinfo.session == 2);
			assert (((SprotoType.foo.response)sinfo.responseObj).ok == false);

			// ================bar====================
			// request
			req = clientRequest.Invoke<Protocol.bar> ();
			assert (req, new byte[] { 0X05, 0X01, 0X08, });

			// dispatch
			sinfo = service.Dispatch (req);
			assert (sinfo.type == SprotoRpc.RpcType.REQUEST);
			assert (sinfo.requestObj == null);
			assert (sinfo.tag == Protocol.bar.Tag);
			assert (sinfo.Response == null);

			// ================blackhole====================
			// request
			req = clientRequest.Invoke<Protocol.blackhole> ();
			assert (req, new byte[]{ 0X05, 0X01, 0X0a });
		}
	}
}

