#Region ;**** Directives created by AutoIt3Wrapper_GUI ****
#AutoIt3Wrapper_Outfile=C:\Users\User\source\repos\WebsitePreserver.App\WebsiteCrawler\Scripts\SaveHtmlFile.exe
#EndRegion ;**** Directives created by AutoIt3Wrapper_GUI ****

;#AutoIt3Wrapper_UseUpx=n
;#AutoIt3Wrapper_Run_After=upx.exe --best --compress-exports=0 "%out%"

#include <StringConstants.au3>
#include <Date.au3>
#include <GUIComboBox.au3>

Main()


Func Main()
	Local $folderPath = $CmdLine[1]
	Local $browserTabTitle = $CmdLine[2]
	Local $isFacebookUrl = $CmdLine[3]
	;Speichern unter
	Local $saveFileDialogTitle="Speichern unter" ;browser "Save As" dialog title
	;Speichern unter bestätigen
	Local $confirmSaveAsDialog="Speichern unter bestätigen" ;Confirm Save As

	;activate browser window
	If WinExists($browserTabTitle) Then
		WinActivate($browserTabTitle)
		WinWaitActive($browserTabTitle)
		Send("^{s}")
		Sleep(200)
		SaveHtmlFile($saveFileDialogTitle, $folderPath, $isFacebookUrl, $confirmSaveAsDialog, $browserTabTitle, 5); 5 is the number of retries allowed
	EndIf

EndFunc


Func SaveHtmlFile($saveFileDialogTitle, $folderPath, $isFacebookUrl, $confirmSaveAsDialog, $browserTabTitle, $retryCount) ; Saves the html file
	If WinExists($saveFileDialogTitle) Then
		WinActivate($saveFileDialogTitle)
		WinWaitActive($saveFileDialogTitle)

		Local $fileName=ControlGetText($saveFileDialogTitle,"","Edit1")
		Local $fileExtension=GetFileExtension($fileName)

		; set the folder path in the address bar
		ControlFocus($saveFileDialogTitle,"","ToolbarWindow324")
		Send('{space}')
		ControlSetText($saveFileDialogTitle,"","", $folderPath)
		Send('{Enter}')
		sleep(1000)

		; Rename the file if it already exists
		if FileExists($folderPath&"\"&$fileName) Then
			Local $newFileName=GetFileNameWithoutExtension($fileName)&GetDateTime()&"."&$fileExtension
			$fileName=$newFileName
		EndIf

		;Set focus to file name control
		ControlFocus($saveFileDialogTitle,"","Edit1")
		ControlSetText($saveFileDialogTitle,"","Edit1", $fileName)

		;If it's a fb page, save it as single html file
		If $isFacebookUrl == 1 Then
			SelectTheComboBox($saveFileDialogTitle, 1)
		EndIf

		Sleep(300)

		ControlFocus($saveFileDialogTitle,"","Button2")
		ControlClick($saveFileDialogTitle,"","Button2")

		Sleep(800)
		;if the file still exists, replace it
		If WinExists($confirmSaveAsDialog) Then
			WinActivate($confirmSaveAsDialog)
			WinWaitActive($confirmSaveAsDialog)
			ControlClick($confirmSaveAsDialog,"","Button1")
		EndIF

		;Take image
		SaveImage($fileName, $folderPath, $confirmSaveAsDialog, $browserTabTitle)
	ElseIf $retryCount >= 0 Then
		sleep(250)
		SaveHtmlFile($saveFileDialogTitle, $folderPath, $isFacebookUrl, $confirmSaveAsDialog, $browserTabTitle, $retryCount - 1);
	EndIf


EndFunc

Func SaveImage($fileName, $folderPath, $confirmSaveAsDialog, $browserTabTitle) ; Saves the Web-page screenshot

	Local $imageFileNameWithoutExtenstion= GetFileNameWithoutExtension($fileName) ; get the file name without the file extension.
	$imageFileNameWithoutExtenstion=StringReplace($imageFileNameWithoutExtenstion,'#','') ; # key cause problem in web console window (so better remove it)

	;enclose full path with double quotes, because image path may contain a whitespace in it.
	Local $imageFileFullPath = '"'&$folderPath&'\'&$imageFileNameWithoutExtenstion&'"'

	; to take screenshot, send control+shift+k shortcut key
	Send("^+{k}")
	Sleep(1200)
	WinActivate($browserTabTitle)
	if WinActive($browserTabTitle) Then
		Send("^+{k}") ;send this key again to set focus into web console
		SendKeepActive($browserTabTitle)
		Send(":screenshot "&$imageFileFullPath&" --fullpage")
		Send("{Enter}")
	EndIf
EndFunc

Func GetFileNameWithoutExtension($Input)
    Return StringRegExpReplace($Input, "\.[^.]*$", "")
EndFunc

Func GetFileExtension($Input)
    Return StringRegExpReplace($Input, "^.*\.", "")
EndFunc

Func GetDateTime()
	Return " "&StringReplace(_NowDate(),"/",".")&"_"&StringReplace(_NowTime(5), ":", ".")
EndFunc

Func SelectTheComboBox($saveFileDialogTitle, $iIndex)
	ControlClick($saveFileDialogTitle,"","ComboBox2")
	$hCombo2 = ControlGetHandle($saveFileDialogTitle, "", "[CLASS:ComboBox; INSTANCE:2]") ; get combobox handle
	_GUICtrlComboBox_SetCurSel($hCombo2, $iIndex)
	Send('{Enter}')
EndFunc
