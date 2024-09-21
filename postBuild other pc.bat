@echo off

MOVE "D:\Amitay\Documents\lethal company modding\Lootrun\LCSpeedlootMod\bin\Debug\Lootrun.dll" "D:\Amitay\Documents\lethal company modding\Lootrun\Package\Lootrun"

xcopy /s "D:\Amitay\Documents\lethal company modding\Lootrun\Package" "D:\Amitay\SteamLibrary\steamapps\common\Lethal Company\BepInEx\plugins" /Y

start steam://rungameid/1966720