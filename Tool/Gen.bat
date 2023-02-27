@echo off

set toolPath=%cd%\protoc-21.12-win64\bin\protoc.exe 
set protoPath=%cd%\..\proto\
set outputPath=%cd%\..\Assets\Scripts\Game\Base\Network\Message\MessageDefine\Protocols\

for /f "delims=" %%i in ('dir /b "%protoPath%\*.proto"') do (
	echo %toolPath% --proto_path=%protoPath% %%i --outputPath=%outputPath%
	%toolPath% --proto_path=%protoPath% %%i --csharp_out=%outputPath%
)

echo %copyToolPath% --proto_path=%protoPath% --outputPath=%outputPath%
start %copyToolPath% %protoPath% %outputPath%

@echo Export Completed!
pause