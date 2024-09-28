@echo off

MOVE "C:\Users\Amitay\Documents\lethal company modding\Lootrun\Lootrun\bin\Debug\netstandard2.1\Lootrun.dll" "C:\Users\Amitay\Documents\lethal company modding\Lootrun\Package\Lootrun"

xcopy /s "C:\Users\Amitay\Documents\lethal company modding\Lootrun\Package" "C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\BepInEx\plugins" /Y

start /b C:\"Program Files (x86)"\Steam\steamapps\common\"Lethal Company"\"Lethal Company.exe"