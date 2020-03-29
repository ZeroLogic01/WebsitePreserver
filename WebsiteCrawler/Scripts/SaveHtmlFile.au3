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
		SaveHtmlFile($saveFileDialogTitle, $folderPath, $confirmSaveAsDialog, $browserTabTitle, 5); 5 is the number of retries allowed
	EndIf

EndFunc


Func SaveHtmlFile($saveFileDialogTitle, $folderPath, $confirmSaveAsDialog, $browserTabTitle, $retryCount) ; Saves the html file
	If WinExists($saveFileDialogTitle) Then
		WinActivate($saveFileDialogTitle)
		WinWaitActive($saveFileDialogTitle)

		Local $fileName=ControlGetText($saveFileDialogTitle,"","Edit1")
		Local $fileExtension=GetFileExtension($fileName)

		If $fileExtension== 'html' Or $fileExtension== 'htm' Then
			SelectTheComboBox($saveFileDialogTitle, 1)
		EndIf

		; set the folder path in the address bar
		ControlFocus($saveFileDialogTitle,"","ToolbarWindow324")
		ControlSend($saveFileDialogTitle, "", "[CLASS:ToolbarWindow32; INSTANCE:4]",  "{space}")
		ControlSetText($saveFileDialogTitle,"","", $folderPath)
		ControlSend($saveFileDialogTitle, "", "",  "{Enter}")
		;Send('{Enter}')
		;sleep(500)

		While WinExists($saveFileDialogTitle)
			$fileName=ControlGetText($saveFileDialogTitle,"","Edit1")
			; Rename the file if it already exists
			if FileExists($folderPath&"\"&$fileName) Then
				Local $newFileName=GetFileNameWithoutExtension($fileName)&GetDateTime()&"."&$fileExtension
				$fileName=$newFileName
			EndIf

			Sleep(500)
			;Set focus to file name control
			ControlFocus($saveFileDialogTitle,"","Edit1")
			ControlCommand($saveFileDialogTitle,"","[CLASS:Edit;INSTANCE:1]","EditPaste", $fileName)
			ControlFocus($saveFileDialogTitle,"","Button2")
			ControlClick($saveFileDialogTitle,"","Button2")


			Sleep(800)
			;if the file still exists
			If WinExists($confirmSaveAsDialog) Then
				WinActivate($confirmSaveAsDialog)
				WinWaitActive($confirmSaveAsDialog)
				While WinExists($confirmSaveAsDialog)
					ControlClick($confirmSaveAsDialog,"","Button2")
				WEnd
			EndIF
		WEnd
		;Take image
		SaveImage($fileName, $folderPath, $confirmSaveAsDialog, $browserTabTitle)
	ElseIf $retryCount >= 0 Then
		sleep(250)
		SaveHtmlFile($saveFileDialogTitle, $folderPath, $confirmSaveAsDialog, $browserTabTitle, $retryCount - 1);
	EndIf


EndFunc

Func SaveImage($fileName, $folderPath, $confirmSaveAsDialog, $browserTabTitle) ; Saves the Web-page screenshot

	Local $imageFileNameWithoutExtenstion= GetFileNameWithoutExtension($fileName) ; get the file name without the file extension.
	$imageFileNameWithoutExtenstion=StringReplace($imageFileNameWithoutExtenstion,'#','') ; # key cause problem in web console window (so better remove it)

	;enclose full path with double quotes, because image path may contain a whitespace in it.
	Local $imageFileFullPath = '"'&$folderPath&'\'&$imageFileNameWithoutExtenstion&'"'

	; to take screenshot, send control+shift+k shortcut key
	Send("^+{k}")
	Sleep(1700)
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

	$hCombo2 = ControlGetHandle($saveFileDialogTitle, "", "[CLASS:ComboBox; INSTANCE:2]") ; get combobox handle

	;DllCall("user32.dll", "int", "SendMessage", "hwnd", $hCombo2, "int", 0x14E, "int", $iIndex, "int", 0)
	If _GUICtrlComboBox_GetCurSel($hCombo2) <> $iIndex Then
		;MsgBox("","",$iIndex)
		;ShowDropDown
		;ControlClick($saveFileDialogTitle,"","ComboBox2")

		_GUICtrlComboBox_BeginUpdate($hCombo2)
		ControlCommand($saveFileDialogTitle,"","[CLASS:ComboBox;INSTANCE:2]","ShowDropDown", "")
		_GUICtrlComboBox_SetCurSel($hCombo2, $iIndex)
		_GUICtrlComboBox_EndUpdate($hCombo2)
		ControlCommand($saveFileDialogTitle,"","[CLASS:ComboBox;INSTANCE:2]","HideDropDown", "")
		;Send('{Enter}')
		sleep(500)
	EndIf
EndFunc
