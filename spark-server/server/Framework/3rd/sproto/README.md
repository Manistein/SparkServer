sproto-Csharp
=============

A pure C# implementation of [sproto](https://github.com/cloudwu/sproto). and using [sprotodump](https://github.com/lvzixun/sproto_dump/blob/master/sprotodump.lua) compiler for C# language on your `.sproto` file to generate data access classes.

## Tutorials
You write a `Member.sproto` file :
```
  .Person {
    name 0 : string
    id 1 : integer
    email 2 : string

    .PhoneNumber {
        number 0 : string
        type 1 : integer
    }

    phone 3 : *PhoneNumber
}

.AddressBook {
    person 0 : *Person
}
```
Then you compile it with [sprotodump](https://github.com/lvzixun/sproto_dump/blob/master/sprotodump.lua), to produce code in C#.


```
$ lua sprotodump.lua
usage: lua sprotodump.lua [[<out_option> <out>] ...] <option> <sproto_file ...>

  out_option:
    -d               dump to speciffic dircetory
    -o               dump to speciffic file
    -p               set package name(only cSharp code use)

  option: 
    -cs              dump to cSharp code file
    -spb             dump to binary spb  file
$
$ lua sprotodump.lua -cs Member.sproto  -o Member.cs
```

Then you use that code like this:

~~~~.c#
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
~~~~

serialize and deserialize :
~~~~.c#
byte[] data = address.encode ();                  // encode to bytes

Sproto.SprotoStream stream = new SprotoStream (); // encode to stream
address.encode(stream);

Sproto.SprotoPack spack = new Sproto.SprotoPack ();
byte[] pack_data = spack.pack (data);             // pack
byte[] unpack_data = spack.unpack(pack_data);     // unpack

AddressBook obj = new AddressBook(unpack_data);   // decode
~~~~

## protocol
the `Test.sproto` file:
```
Foobar 1 {
  request {
    what 0 : string
  }
  response {
    ok 0 : boolean
  }
}
```

dump to c# code:
~~~~.c#
public class Protocol : ProtocolBase {
  public static  Protocol Instance = new Protocol();
  static Protocol() {
    Protocol.SetProtocol<Foobar> (Foobar.Tag);
    Protocol.SetRequest<SprotoType.Foobar.request> (Foobar.Tag);
    Protocol.SetResponse<SprotoType.Foobar.response> (Foobar.Tag);

  }

  public class Foobar {
    public const int Tag = 1;
  }
}
~~~~

## RPC API
Read [TestCaseRpc.cs](https://github.com/lvzixun/sproto-Csharp/blob/master/testCase/TestCaseRpc.cs) for detail.


## Use in Unity
[sproto-Unity](https://github.com/m2q1n9/sproto-Unity)


## benchmark

in my i5-3470 @3.20GHz :

| library | encode 1M times | decode 1M times |
|:-------:|:---------------:|:---------------:|
| sproto-Csharp | 2.84s         | 3.00s     |
| sproto-Csharp(unpack) | 1.36s | 2.12s     |
| [protobuf-net](https://github.com/mgravell/protobuf-net) | 6.97s | 8.09s |





