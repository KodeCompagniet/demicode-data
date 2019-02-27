@echo off
@msbuild build.msbuild /t:BuildPackageArtifacts /p:Configuration=Release /p:NugetSecret=<insert secret here>