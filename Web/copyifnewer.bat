echo Comparing two files: %1 with %2

if not exist %1 goto File1NotFound
if not exist %2 goto File2NotFound

fc %1 %2 
if %ERRORLEVEL%==0 GOTO NoCopy

echo Files are not the same.  Copying %1 over %2
attrib -r %2
copy %1 %2 /y & goto END

:NoCopy
echo Files are the same.  Did nothing
goto END

:File1NotFound
echo %1 not found.
goto END

:File2NotFound
copy %1 %2 /y
goto END

:END
echo Done.