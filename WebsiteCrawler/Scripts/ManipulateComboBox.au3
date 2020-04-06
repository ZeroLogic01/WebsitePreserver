#include <GUIComboBox.au3>

Local $saveAsDialogTitle=$CmdLine[1]
Local $iIndex=$CmdLine[2]

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
