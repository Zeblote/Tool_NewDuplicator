// * ######################################################################
// *
// *    New Duplicator - Client
// *    Controls
// *
// *    -------------------------------------------------------------------
// *    Adds keybinds for copy, paste, cut when active
// *
// * ######################################################################

//Register rebind-able controls
function ndRegisterKeybinds()
{
	if($ND::KeybindsRegistered)
		return;

	$RemapDivision[$RemapCount] = "New Duplicator";
	$RemapName[$RemapCount]     = "Copy Selection (Ctrl C)";
	$RemapCmd[$RemapCount]      = "ndInputCopy";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Paste Selection (Ctrl V)";
	$RemapCmd[$RemapCount]      = "ndInputPaste";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Cut Selection (Ctrl X)";
	$RemapCmd[$RemapCount]      = "ndInputCut";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Super-Cut Bricks (Shift-Ctrl X)";
	$RemapCmd[$RemapCount]      = "ndInputSuperCut";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Multiselect (Ctrl, Hold to use)";
	$RemapCmd[$RemapCount]      = "ndInputMultiSelect";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Send /NewDuplicator";
	$RemapCmd[$RemapCount]      = "ndInputNewDuplicator";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Send /FillWrench";
	$RemapCmd[$RemapCount]      = "ndInputFillWrench";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Send /ForcePlant";
	$RemapCmd[$RemapCount]      = "ndInputForcePlant";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Send /ToggleForcePlant";
	$RemapCmd[$RemapCount]      = "ndInputToggleForcePlant";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Send /MirrorX";
	$RemapCmd[$RemapCount]      = "ndInputMirrorX";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Send /MirrorY";
	$RemapCmd[$RemapCount]      = "ndInputMirrorY";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Send /MirrorZ";
	$RemapCmd[$RemapCount]      = "ndInputMirrorZ";
	$RemapCount++;

	$ND::KeybindsRegistered = true;
}

//Enable the copy, paste, cut keybinds
function clientCmdNdEnableKeybinds(%bool)
{
	if(%bool && !$ND::KeybindsEnabled)
	{
		%map = new ActionMap(ND_KeyMap);

		if(MoveMap.getBinding("ndInputCopy") $= "")
			%map.bind("keyboard", isWindows() ? "ctrl c" : "cmd c", "ndInputCopy");

		if(MoveMap.getBinding("ndInputPaste") $= "")
			%map.bind("keyboard", isWindows() ? "ctrl v" : "cmd v", "ndInputPaste");

		if(MoveMap.getBinding("ndInputCut") $= "")
			%map.bind("keyboard", isWindows() ? "ctrl x" : "cmd x", "ndInputCut");

		if(MoveMap.getBinding("ndInputSuperCut") $= "")
			%map.bind("keyboard", isWindows() ? "shift-ctrl x" : "shift-cmd x", "ndInputSuperCut");

		if(MoveMap.getBinding("ndInputMultiSelect") $= "")
			%map.bind("keyboard", isWindows() ? "lcontrol" : "cmd", "ndInputMultiSelect");

		%map.push();
		$ND::KeybindsEnabled = true;
	}
	else if(!%bool && $ND::KeybindsEnabled)
	{
		ND_KeyMap.pop();
		ND_KeyMap.delete();
		$ND::KeybindsEnabled = false;
	}
}

//Input handlers
function ndInputNewDuplicator   (%bool) {if(!%bool)return; commandToServer('newDuplicator'   );}
function ndInputCopy            (%bool) {if(!%bool)return; commandToServer('ndCopy'          );}
function ndInputPaste           (%bool) {if(!%bool)return; commandToServer('ndPaste'         );}
function ndInputCut             (%bool) {if(!%bool)return; commandToServer('ndCut'           );}
function ndInputSuperCut        (%bool) {if(!%bool)return; commandToServer('superCut'        );}
function ndInputFillWrench      (%bool) {if(!%bool)return; commandToServer('fillWrench'      );}
function ndInputForcePlant      (%bool) {if(!%bool)return; commandToServer('forcePlant'      );}
function ndInputToggleForcePlant(%bool) {if(!%bool)return; commandToServer('toggleForcePlant');}
function ndInputMirrorX         (%bool) {if(!%bool)return; commandToServer('mirrorX'         );}
function ndInputMirrorY         (%bool) {if(!%bool)return; commandToServer('mirrorY'         );}
function ndInputMirrorZ         (%bool) {if(!%bool)return; commandToServer('mirrorZ'         );}

function ndInputMultiSelect(%bool) {commandToServer('ndMultiSelect', %bool);}
