@echo off

MOVE "C:\Users\Amitay\Documents\lethal company modding\Lootrun\LCSpeedlootMod\bin\Debug\Lootrun.dll" "C:\Users\Amitay\Documents\lethal company modding\Lootrun\Package\Lootrun"

xcopy /s "C:\Users\Amitay\Documents\lethal company modding\Lootrun\Package" "C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\BepInEx\plugins" /Y

start steam://rungameid/1966720