1.主要解决protoc.exe生成cs代码时。像Dictionary和List等对象，在mergeFrom时没有Clear，直接Add,不利于对象池使用问题
2.解决csharp中协议对象不能直接重复使用问题
3.替代目录：protobuf-21.12\src\google\protobuf\compiler\csharp
版本是这个版本