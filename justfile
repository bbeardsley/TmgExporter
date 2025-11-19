default:
	@just --list

build:
	dotnet build TmgExporter.sln

restore:
	dotnet restore TmgExporter.sln

publish:
	dotnet publish TmgExporter/TmgExporter.csproj -c Release -o ./publish --runtime win-x64 --self-contained true -p:PublishSingleFile=true
