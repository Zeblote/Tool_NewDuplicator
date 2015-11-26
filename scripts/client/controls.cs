// * ######################################################################
// *
// *    New Duplicator - Client
// *    Controls
// *
// *    -------------------------------------------------------------------
// *    Adds keybinds for copy, paste, cut when active
// *
// * ######################################################################

//Enable the copy, paste, cut keybinds
function clientCmdNdEnableKeybinds(%bool)
{
	if(%bool && !$ND::KeybindsEnabled)
	{
		%map = new ActionMap(ND_KeyMap);

		if(isWindows())
		{
			%map.bind("keyboard", "ctrl c", "ndHandleCopy");
			%map.bind("keyboard", "ctrl v", "ndHandlePaste");
			%map.bind("keyboard", "ctrl x", "ndHandleCut");
		}
		else
		{
			%map.bind("keyboard", "cmd c", "ndHandleCopy");
			%map.bind("keyboard", "cmd v", "ndHandlePaste");
			%map.bind("keyboard", "cmd x", "ndHandleCut");
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
