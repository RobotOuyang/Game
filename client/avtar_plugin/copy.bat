rd /s /q ..\Assets\Plugins\Android\libs
XCOPY /E /H libs ..\Assets\Plugins\Android\libs\
del ..\Assets\Plugins\Android\libs\classes_release.jar

rd /s /q ..\Assets\Plugins\Android\assets
XCOPY /E /H assets ..\Assets\Plugins\Android\assets\

copy AndroidManifest.xml ..\Assets\Plugins\Android

rd /s /q ..\Assets\Plugins\Android\res
XCOPY /E /H res ..\Assets\Plugins\Android\res\

pause