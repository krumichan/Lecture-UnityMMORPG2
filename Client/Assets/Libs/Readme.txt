Library를 dll 형식으로 가져올 필요가 있다.

1. Server측의 Project 접근.
 /Server/Server/bin/Release/netcoreapp3.1/ServerCore.dll

2.  Google.Protobuf.dll
 - Server Project에서 NuGet으로 Google.Protobuf를 설치했을 경우.
 - C://{user}/.nuget/packages/google.protobuf/3.18.0/lib/net45/Google.Protobuf.dll

3. System.Buffers.dll
 - Server Project에서 NuGet으로 System.Buffers를 설치했을 경우.
 - C://{user}/.nuget/packages/system.buffers/4.5.1/lib/netstandard2.0/System.Buffers.dll

4. System.Memory.dll
 - Server Project에서 NuGet으로 System.Memory를 설치했을 경우.
 - C://{user}/.nuget/packages/system.memory/4.5.4/lib/netstandard2.0/System.Memory.dll

6. System.Runtime.CompilerServices.Unsafe
 - Server Project에서 NuGet으로 System.Runtime.CompilerServices.Unsafe를 설치했을 경우.
 - C://{user}/.nuget/packages/system.runtime.compilerservices.unsafe/5.0.0/lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll
