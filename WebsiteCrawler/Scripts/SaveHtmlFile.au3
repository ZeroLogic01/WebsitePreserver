#Region ;**** Directives created by AutoIt3Wrapper_GUI ****
#AutoIt3Wrapper_Outfile=SaveHtmlFile.exe
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
	Local $saveAsDialogTitle=$CmdLine[3] ;browser "Save As" dialog title
	;Speichern unter bestätigen
	Local $confirmSaveAsDialog=$CmdLine[4] ;Confirm Save As

	;activate browser window
	If WinExists($browserTabTitle) Then
		WinActivate($browserTabTitle)
		WinWaitActive($browserTabTitle,"",10)

		;SendKeepActive($browserTabTitle)
		ControlSend($browserTabTitle,$browserTabTitle, "", "^{s}")
		;SendKeepActive("")

		SaveHtmlFile($saveAsDialogTitle, $folderPath, $confirmSaveAsDialog, $browserTabTitle, 5); 5 is the number of retries allowed
	EndIf

EndFunc


Func SaveHtmlFile($saveAsDialogTitle, $folderPath, $confirmSaveAsDialog, $browserTabTitle, $retryCount) ; Saves the html file
	;activate save as dialog if exists
	If WinExists($saveAsDialogTitle) Then
		WinActivate($saveAsDialogTitle)

		$result=WinWaitActive($saveAsDialogTitle,"",1)
		If $result = 0 Then
			Return
		EndIf

		Local $fileName=ControlGetText($saveAsDialogTitle,"","Edit1")
		Local $fileExtension=GetFileExtension($fileName)

		If $fileExtension== 'html' Or $fileExtension== 'htm' Then
			SelectTheComboBox($saveAsDialogTitle, 1)
		EndIf

		Local $currentFolderAddressInAddressBar = StringReplace(ControlGetText($saveAsDialogTitle,"","[CLASS:ToolbarWindow32; INSTANCE:4]"), $CmdLine[5], '')
		If StringCompare($currentFolderAddressInAddressBar,$folderPath,0) <> 0 Then
			; set the folder path in the address bar
			ControlFocus($saveAsDialogTitle,"","ToolbarWindow324")
			ControlSend($saveAsDialogTitle, "", "[CLASS:ToolbarWindow32; INSTANCE:4]",  "{space}")
			ControlSetText($saveAsDialogTitle,"","", $folderPath)
			ControlSend($saveAsDialogTitle, "", "",  "{Enter}")
			Sleep(500)
		EndIf

		;$fileName=ControlGetText($saveAsDialogTitle,"","Edit1")
		; Rename the file if it already exists
		if FileExists($folderPath&"\"&$fileName) Then
			Local $newFileName=GetFileNameWithoutExtension($fileName)&GetDateTime()&"."&$fileExtension
			$fileName=$newFileName
		EndIf


		While WinExists($saveAsDialogTitle)
			;Sleep(500)
			;Set focus to file name control
			ControlFocus($saveAsDialogTitle,"","Edit1")
			ControlCommand($saveAsDialogTitle,"","[CLASS:Edit;INSTANCE:1]","EditPaste", $fileName)
			ControlFocus($saveAsDialogTitle,"","Button2")
			ControlClick($saveAsDialogTitle,"","Button2")

			Sleep(800)
			;if the file still exists
			If WinExists($confirmSaveAsDialog) Then
				WinActivate($confirmSaveAsDialog)
				WinWaitActive($confirmSaveAsDialog)
					ControlClick($confirmSaveAsDialog,"","Button1")
			EndIF
		WEnd
		;Take image
		SaveImage($fileName, $folderPath, $confirmSaveAsDialog, $browserTabTitle)
	ElseIf $retryCount >= 0 Then
		sleep(250)
		SaveHtmlFile($saveAsDialogTitle, $folderPath, $confirmSaveAsDialog, $browserTabTitle, $retryCount - 1);
	EndIf


EndFunc

Func SaveImage($fileName, $folderPath, $confirmSaveAsDialog, $browserTabTitle) ; Saves the Web-page screenshot
	If WinExists($browserTabTitle) Then
		WinActivate($browserTabTitle)

		$result=WinWaitActive($browserTabTitle,"",1)
		If $result = 0 Then
			Return
		EndIf

		Local $imageFileNameWithoutExtenstion= GetFileNameWithoutExtension($fileName) ; get the file name without the file extension.
		$imageFileNameWithoutExtenstion=StringReplace($imageFileNameWithoutExtenstion,'#','') ; # key causes problem in web console window (so better remove it)

		;enclose full path with double quotes, because image path may contain a whitespace.
		Local $imageFileFullPath = '"'&$folderPath&'\'&$imageFileNameWithoutExtenstion&'"'

		; to take screenshot, send control+shift+k shortcut key
		SendKeepActive($browserTabTitle)
		Send("^+{k}")
		Sleep(1500)
		Send(":screenshot "&$imageFileFullPath&" --fullpage",1)
		Send("{Enter}")
		SendKeepActive("")

		;ControlSend($browserTabTitle, "", "", "^+{k}",0)
		;ControlSend($browserTabTitle, "", "", "^+{k}",0)
		;Sleep(200)
		;Send("^+{k}") ;send this key again to set focus into web console
		;SendKeepActive($browserTabTitle)


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

Func SelectTheComboBox($saveAsDialogTitle, $iIndex)

	$hCombo2 = ControlGetHandle($saveAsDialogTitle, "", "[CLASS:ComboBox; INSTANCE:2]") ; get combobox handle

	;DllCall("user32.dll", "int", "SendMessage", "hwnd", $hCombo2, "int", 0x14E, "int", $iIndex, "int", 0)
	If _GUICtrlComboBox_GetCurSel($hCombo2) <> $iIndex Then
		;MsgBox("","",$iIndex)
		;ShowDropDown
		;ControlClick($saveFileDialogTitle,"","ComboBox2")

		_GUICtrlComboBox_BeginUpdate($hCombo2)
		ControlCommand($saveAsDialogTitle,"","[CLASS:ComboBox;INSTANCE:2]","ShowDropDown", "")
		_GUICtrlComboBox_SetCurSel($hCombo2, $iIndex)
		_GUICtrlComboBox_EndUpdate($hCombo2)
		ControlCommand($saveAsDialogTitle,"","[CLASS:ComboBox;INSTANCE:2]","HideDropDown", "")
		;Send('{Enter}')
		sleep(500)
	EndIf
EndFunc
