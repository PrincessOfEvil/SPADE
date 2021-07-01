REM "%~f1" %~n1lit%~x1

SET imagick="%UserProfile%\Desktop\imagick\convert.exe"

%imagick% "%~f1" -crop "64x64+0" "%~n1_a%~x1"
%imagick% "%~f1" -crop "64x64+64" "%~n1_b%~x1"
%imagick% "%~f1" -crop "64x64+128" "%~n1_c%~x1"

DEL "%~f1"

PAUSE