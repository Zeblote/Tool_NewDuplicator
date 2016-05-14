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
	if(!$Pref::ND::ManualKeybinds)
		return;

	if($ND::KeybindsRegistered)
		return;

	$RemapDivision[$RemapCount] = "New Duplicator";
	$RemapName[$RemapCount]     = "Copy Selection";
	$RemapCmd[$RemapCount]      = "ndHandleCopy";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Paste Selection";
	$RemapCmd[$RemapCount]      = "ndHandlePaste";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Cut Selection";
	$RemapCmd[$RemapCount]      = "ndHandleCut";
	$RemapCount++;

	$RemapName[$RemapCount]     = "Multiselect (Hold to use)";
	$RemapCmd[$RemapCount]      = "ndHandleMultiSelect";
	$RemapCount++;

	$ND::KeybindsRegistered = true;
}

//Enable the copy, paste, cut keybinds
function clientCmdNdEnableKeybinds(%bool)
{
	if(%bool && !$ND::KeybindsEnabled)
	{
		if($Pref::ND::ManualKeybinds)
			return;

		%map = new ActionMap(ND_KeyMap);

		if(isWindows())
		{
			%map.bind("keyboard", "ctrl c", "ndHandleCopy");
			%map.bind("keyboard", "ctrl v", "ndHandlePaste");
			%map.bind("keyboard", "ctrl x", "ndHandleCut");
			%map.bind("keyboard", "lcontrol", "ndHandleMultiSelect");
		}
		else
		{
			%map.bind("keyboard", "cmd c", "ndHandleCopy");
			%map.bind("keyboard", "cmd v", "ndHandlePaste");
			%map.bind("keyboard", "cmd x", "ndHandleCut");
			%map.bind("keyboard", "cmd", "ndHandleMultiSelect");
		}

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

//Client pressed ctrl c
function ndHandleCopy(%bool)
{
	if(!%bool)
		return;

	commandToServer('ndCopy');
}

//Client pressed ctrl v
function ndHandlePaste(%bool)
{
	if(!%bool)
		return;

	commandToServer('ndPaste');	
}

//Client pressed ctrl x
function ndHandleCut(%bool)
{
	if(!%bool)
		return;

	commandToServer('ndCut');	
}

//Client pressed ctrl
function ndHandleMultiSelect(%bool)
{
	commandToServer('ndMultiSelect', %bool);	
}

