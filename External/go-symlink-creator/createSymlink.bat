@echo off
cd /d %~dp0
call go-symlink-creator SymlinkSettings.yaml
pause