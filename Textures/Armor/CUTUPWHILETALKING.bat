REM "%~f1" %~n1lit%~x1

SET imagick="%UserProfile%\Desktop\imagick\convert.exe"

%imagick% "%~f1" -crop "128x128+0" "%~n1_south%~x1"
%imagick% "%~f1" -crop "128x128+128" "%~n1_north%~x1"
%imagick% "%~f1" -crop "128x128+256" "%~n1_east%~x1"

DEL "%~f1"

PAUSE