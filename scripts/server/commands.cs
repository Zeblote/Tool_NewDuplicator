// General server commands used to control the new duplicator.
// -------------------------------------------------------------------

//Information commands
///////////////////////////////////////////////////////////////////////////

//Shows version of blockland and the new duplicator
function serverCmdDupVersion(%client)
{
	messageClient(%client, '', "\c6Blockland version: \c3r" @ getBuildNumber());
	messageClient(%client, '', "\c6New duplicator version: \c3" @ $ND::Version);

	if(%client.ndClient)
		messageClient(%client, '', "\c6Your new duplicator version: \c3" @ %client.ndVersion);
	else
		messageClient(%client, '', "\c6You don't have the new duplicator installed");
}

//Shows versions of other clients
function serverCmdDupClients(%client)
{
	messageClient(%client, '', "\c6New duplicator versions:");

	%cnt = ClientGroup.getCount();
	for(%i = 0; %i < %cnt; %i++)
	{
		%cl = ClientGroup.getObject(%i);

		if(%cl.ndClient)
			messageClient(%client, '', "\c3" @ %cl.name @ "\c6 has \c3" @ %cl.ndVersion);
	}
}

//Alternative short commands
function serverCmdDV(%client){serverCmdDupVersion(%client);}
function serverCmdDC(%client){serverCmdDupClients(%client);}



//Equip commands
///////////////////////////////////////////////////////////////////////////

//Command to equip the new duplicator
function serverCmdNewDuplicator(%client)
{
	//Check admin
	if($Pref::Server::ND::AdminOnly && !%client.isAdmin)
	{
		messageClient(%client, '', "\c6The new duplicator is admin only. Ask an admin for help.");
		return;
	}

	//Check minigame
	if(isObject(%client.minigame) && !%client.minigame.enablebuilding)
	{
		messageClient(%client, '', "\c6You cannot use the new duplicator while building is disabled in your minigame.");
		return;
	}

	//Check player
	if(!isObject(%player = %client.player))
	{
		messageClient(%client, '', "\c6You must be spawned to equip the new duplicator.");
		return;
	}

	//Hide brick selector and tool gui
	%client.ndLastEquipTime = $Sim::Time;
	commandToClient(%client, 'setScrollMode', 3);

	//Give player a duplicator
	%image = %client.ndImage;

	if(!isObject(%image))
		%image = ND_Image;

	%player.updateArm(%image);
	%player.mountImage(%image, 0);
	%client.ndEquippedFromItem = false;
}

//Alternative commands to equip the new duplicator (override old duplicators)
package NewDuplicator_Server_Final
{
	function serverCmdDuplicator(%client){serverCmdNewDuplicator(%client);}
	function serverCmdDuplicato (%client){serverCmdNewDuplicator(%client);}
	function serverCmdDuplicat  (%client){serverCmdNewDuplicator(%client);}
	function serverCmdDuplica   (%client){serverCmdNewDuplicator(%client);}
	function serverCmdDuplic    (%client){serverCmdNewDuplicator(%client);}
	function serverCmdDupli     (%client){serverCmdNewDuplicator(%client);}
	function serverCmdDupl      (%client){serverCmdNewDuplicator(%client);}
	function serverCmdDup       (%client){serverCmdNewDuplicator(%client);}
	function serverCmdDu        (%client){serverCmdNewDuplicator(%client);}
	function serverCmdD         (%client){serverCmdNewDuplicator(%client);}
};



//Default keybind commands
///////////////////////////////////////////////////////////////////////////
package NewDuplicator_Server
{
	//Light key (default: R)
	function serverCmdLight(%client)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onLight(%client);
		else
			parent::serverCmdLight(%client);
	}

	//Next seat (default: .)
	function serverCmdNextSeat(%client)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onNextSeat(%client);
		else
			parent::serverCmdNextSeat(%client);
	}

	//Previous seat (default: ,)
	function serverCmdPrevSeat(%client)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onPrevSeat(%client);
		else
			parent::serverCmdPrevSeat(%client);
	}

	//Shifting the ghost brick (default: numpad 2468/13/5+)
	function serverCmdShiftBrick(%client, %x, %y, %z)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onShiftBrick(%client, %x, %y, %z);

		//Call parent to play animation
		parent::serverCmdShiftBrick(%client, %x, %y, %z);
	}

	//Super-shifting the ghost brick (default: alt numpad 2468/5+)
	function serverCmdSuperShiftBrick(%client, %x, %y, %z)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onSuperShiftBrick(%client, %x, %y, %z);

		//Call parent to play animation
		parent::serverCmdSuperShiftBrick(%client, %x, %y, %z);
	}

	//Rotating the ghost brick (default: numpad 79)
	function serverCmdRotateBrick(%client, %direction)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onRotateBrick(%client, %direction);

		//Call parent to play animation
		parent::serverCmdRotateBrick(%client, %direction);
	}

	//Undo bricks (default: ctrl z)
	function serverCmdUndoBrick(%client)
	{
		if(%client.ndUndoInProgress)
		{
			messageClient(%client, '', "\c6Please wait for the current undo task to finish.");
			return;
		}

		//This really needs a better api.
		//Wtf were you thinking, badspot?
		%state = %client.undoStack.pop();
		%type = getField(%state, 1);

		if(%type $= "ND_PLANT"
		|| %type $= "ND_PAINT"
		|| %type $= "ND_WRENCH")
		{
			%obj = getField(%state, 0);

			if(%obj.brickCount > 10 && %client.ndUndoConfirm != %obj)
			{
				messageClient(%client, '', "\c6Next undo will affect \c3" @ %obj.brickCount @ "\c6 bricks. Press undo again to continue.");
				%client.undoStack.push(%state);
				%client.ndUndoConfirm = %obj;
				return;
			}

			%obj.ndStartUndo(%client);

			if(isObject(%client.player))
				%client.player.playThread(3, "undo");

			%client.ndUndoConfirm = 0;
			return;
		}

		%client.ndUndoConfirm = 0;
		%client.undoStack.push(%state);
		parent::serverCmdUndoBrick(%client);
	}
};

package NewDuplicator_Server_Final
{
	//Planting the ghost brick (default: numpad enter)
	function serverCmdPlantBrick(%client)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onPlantBrick(%client);
		else
			parent::serverCmdPlantBrick(%client);
	}

	//Removing the ghost brick (default: numpad 0)
	function serverCmdCancelBrick(%client)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onCancelBrick(%client);
		else
			parent::serverCmdCancelBrick(%client);
	}
};



//Custom keybind commands
///////////////////////////////////////////////////////////////////////////

//Copy selection (ctrl c)
function serverCmdNdCopy(%client)
{
	if(%client.ndModeIndex)
		%client.ndMode.onCopy(%client);
}

//Paste selection (ctrl v)
function serverCmdNdPaste(%client)
{
	if(%client.ndModeIndex)
		%client.ndMode.onPaste(%client);
}

//Cut selection (ctrl x)
function serverCmdNdCut(%client)
{
	if(%client.ndModeIndex)
		%client.ndMode.onCut(%client);
}

//Cut selection
function serverCmdCut(%client)
{
	serverCmdNdCut(%client);
}

//Supercut selection
function serverCmdSuperCut(%client)
{
	if(%client.ndModeIndex != $NDM::BoxSelect)
	{
		messageClient(%client, '', "\c6Supercut can only be used on box selection mode.");
		return;
	}

	if(!isObject(%client.ndSelectionBox))
	{
		messageClient(%client, '', "\c6Supercut can only be used with a selection box.");
		return;
	}

	if(%client.ndSelectionAvailable)
	{
		messageClient(%client, '', "\c6Supercut can not be used with any bricks selected.");
		return;
	}

	commandToClient(%client, 'messageBoxOkCancel', "New Duplicator | Supercut",
		"Supercut is destructive and does\nNOT support undo at this time." @
		"\n\nPlease make sure the box is correct,\nthen press OK below.",
		'ndConfirmSuperCut');
}

//Confirm Supercut selection
function serverCmdNdConfirmSuperCut(%client)
{
	if(%client.ndModeIndex != $NDM::BoxSelect)
	{
		messageClient(%client, '', "\c6Supercut can only be used on box selection mode.");
		return;
	}

	if(!isObject(%client.ndSelectionBox))
	{
		messageClient(%client, '', "\c6Supercut can only be used with a selection box.");
		return;
	}

	if(%client.ndSelectionAvailable)
	{
		messageClient(%client, '', "\c6Supercut can not be used with any bricks selected.");
		return;
	}

	%client.fillBricksAfterSuperCut = false;
	%client.ndMode.onSuperCut(%client);
}

//Alternative short command
function serverCmdSC(%client){serverCmdSuperCut(%client);}

//Fill volume with bricks
function serverCmdFillBricks(%client)
{
	if($Pref::Server::ND::FillBricksAdminOnly && !%client.isAdmin)
	{
		messageClient(%client, '', "\c6Fill Bricks is admin only. Ask an admin for help.");
		return;
	}

	if(!isObject(%client.ndSelectionBox))
	{
		messageClient(%client, '', "\c6The fillBricks command can only be used with a selection box.");
		return;
	}

	if(%client.ndSelectionAvailable)
	{
		messageClient(%client, '', "\c6The fillBricks command can not be used with any bricks selected.");
		return;
	}

	if(!%client.ndSelectionBox.hasVolume())
	{
		messageClient(%client, '', "\c6The fillBricks command can only be used with a selection box that has a volume.");
		return;
	}

	commandToClient(%client, 'messageBoxOkCancel', "New Duplicator | /FillBricks",
		"/FillBricks will first do a Supercut\nbefore placing bricks, to fix overlap." @
		"\n\nSupercut is destructive and does\nNOT support undo at this time." @
		"\n\nPlease make sure the box is correct,\nthen press OK below to continue.",
		'ndConfirmFillBricks');
}

//Confirm fill volume with bricks
function serverCmdNdConfirmFillBricks(%client)
{
	if($Pref::Server::ND::FillBricksAdminOnly && !%client.isAdmin)
	{
		messageClient(%client, '', "\c6Fill Bricks is admin only. Ask an admin for help.");
		return;
	}

	if(!isObject(%client.ndSelectionBox))
	{
		messageClient(%client, '', "\c6The fillBricks command can only be used with a selection box.");
		return;
	}

	if(%client.ndSelectionAvailable)
	{
		messageClient(%client, '', "\c6The fillBricks command can not be used with any bricks selected.");
		return;
	}

	if(!%client.ndSelectionBox.hasVolume())
	{
		messageClient(%client, '', "\c6The fillBricks command can only be used with a selection box that has a volume.");
		return;
	}

	%client.fillBricksAfterSuperCut = true;
	%client.ndMode.onSuperCut(%client);
}

//Alternative short command
function serverCmdFB(%client){serverCmdFillBricks(%client);}


//MultiSelect toggle (ctrl)
function serverCmdNdMultiSelect(%client, %bool)
{
	%client.ndMultiSelect = !!%bool;

	if(%client.ndModeIndex == $NDM::StackSelect || %client.ndModeIndex == $NDM::BoxSelect)
		%client.ndUpdateBottomPrint();
}



//Mirror commands
///////////////////////////////////////////////////////////////////////////

//Mirror selection on X relative to player
function serverCmdMirrorX(%client)
{
	if((getAngleIDFromPlayer(%client.getControlObject()) - %client.ndSelection.ghostAngleID) % 2 == 1)
		%client.ndMirror(0);
	else
		%client.ndMirror(1);
}

//Mirror selection on Y relative to player
function serverCmdMirrorY(%client)
{
	if((getAngleIDFromPlayer(%client.getControlObject()) - %client.ndSelection.ghostAngleID) % 2 == 1)
		%client.ndMirror(1);
	else
		%client.ndMirror(0);
}

//Mirror selection on Z
function serverCmdMirrorZ(%client)
{
	%client.ndMirror(2);
}

//Alternative short commands
function serverCmdMX(%client){serverCmdMirrorX(%client);}
function serverCmdMY(%client){serverCmdMirrorY(%client);}
function serverCmdMZ(%client){serverCmdMirrorZ(%client);}

//Attempt to mirror selection on axis
function GameConnection::ndMirror(%client, %axis)
{
	//Make sure symmetry table is created
	if(!$ND::SymmetryTableCreated)
	{
		if(!$ND::SymmetryTableCreating)
			ndCreateSymmetryTable();

		messageClient(%client, '', "\c6Please wait for the symmetry table to finish, then mirror again.");
		return;
	}

	//If we're in plant mode, mirror the selection
	if(isObject(%client.ndSelection) && %client.ndModeIndex == $NDM::PlantCopy)
	{
		%client.ndSelection.mirrorGhostBricks(%axis);
		return;
	}

	//If we have a ghost brick, mirror that instead
	if(isObject(%client.player) && isObject(%client.player.tempBrick))
	{
		%client.player.tempBrick.ndMirrorGhost(%client, %axis);
		return;
	}

	//We didn't mirror anything
	messageClient(%client, '', "\c6The mirror command can only be used in plant mode or with a ghost brick.");
}

//List potential mirror errors in last plant
function serverCmdMirErrors(%client)
{
	%xerr = $NS[%client, "MXC"];
	%zerr = $NS[%client, "MZC"];

	if(%xerr)
	{
		messageClient(%client, '', " ");
		messageClient(%client, '', "\c6These bricks are asymmetric and probably mirrored incorrectly:");

		for(%i = 0; %i < %xerr; %i++)
		{
			%db = $NS[%client, "MXE", %i];
			messageClient(%client, '', "\c7 -" @ %i + 1 @ "- \c6" @ %db.category @ "/" @ %db.subCategory @ "/" @ %db.uiName);
		}
	}

	if(%zerr)
	{
		messageClient(%client, '', " ");
		messageClient(%client, '', "\c6These bricks are not vertically symmetric and probably incorrect:");

		for(%i = 0; %i < %zerr; %i++)
		{
			%db = $NS[%client, "MZE", %i];
			messageClient(%client, '', "\c7 -" @ %i + 1 @ "- \c6" @ %db.category @ "/" @ %db.subCategory @ "/" @ %db.uiName);
		}
	}

	if(!%xerr && !%zerr)
		messageClient(%client, '', "\c6There were no mirror errors in your last plant attempt.");
}



//Force plant
///////////////////////////////////////////////////////////////////////////

//Force plant one time
function serverCmdForcePlant(%client)
{
	//Check mode
	if(%client.ndModeIndex != $NDM::PlantCopy)
	{
		messageClient(%client, '', "\c6Force Plant can only be used in Plant Mode.");
		return;
	}

	//Check admin
	if($Pref::Server::ND::FloatAdminOnly && !%client.isAdmin)
	{
		messageClient(%client, '', "\c6Force Plant is admin only. Ask an admin for help.");
		return;
	}

	NDM_PlantCopy.conditionalPlant(%client, true);
}

//Alternative short command
function serverCmdFP(%client){serverCmdForcePlant(%client);}

//Keep force plant enabled
function serverCmdToggleForcePlant(%client)
{
	//Check admin
	if($Pref::Server::ND::FloatAdminOnly && !%client.isAdmin)
	{
		messageClient(%client, '', "\c6Force Plant is admin only. Ask an admin for help.");
		return;
	}

	%client.ndForcePlant = !%client.ndForcePlant;

	if(%client.ndForcePlant)
		messageClient(%client, '', "\c6Force Plant has been enabled. Use \c3/toggleForcePlant\c6 to disable it.");
	else
		messageClient(%client, '', "\c6Force Plant has been disabled. Use \c3/toggleForcePlant\c6 to enable it again.");
}

//Alternative short command
function serverCmdTFP(%client){serverCmdToggleForcePlant(%client);}



//Fill color
///////////////////////////////////////////////////////////////////////////

package NewDuplicator_Server
{
	//Enable fill color mode or show the current color
	function serverCmdUseSprayCan(%client, %index)
	{
		%mode = %client.ndModeIndex;

		if(%mode == $NDM::StackSelect || %mode == $NDM::BoxSelect)
		{
			if(isObject(%client.ndSelection) && %client.ndSelection.brickCount)
			{
				%client.currentColor = %index;
				%client.currentFxColor = "";
				%client.ndSetMode(NDM_FillColor);
				return;
			}
		}
		else if(%mode == $NDM::FillColor || %client.ndModeIndex == $NDM::FillColorProgress)
		{
			%client.currentColor = %index;
			%client.currentFxColor = "";
			%client.ndUpdateBottomPrint();
			return;
		}

		cancel(%client.ndToolSchedule);
		parent::serverCmdUseSprayCan(%client, %index);
	}

	//Enable fill color mode or show the current color
	function serverCmdUseFxCan(%client, %index)
	{
		%mode = %client.ndModeIndex;

		if(%mode == $NDM::StackSelect || %mode == $NDM::BoxSelect)
		{
			if(isObject(%client.ndSelection) && %client.ndSelection.brickCount)
			{
				%client.currentFxColor = %index;
				%client.ndSetMode(NDM_FillColor);
			}
			else
				parent::serverCmdUseFxCan(%client, %index);
		}
		else if(%mode == $NDM::FillColor || %client.ndModeIndex == $NDM::FillColorProgress)
		{
			%client.currentFxColor = %index;
			%client.ndUpdateBottomPrint();
		}
		else
			parent::serverCmdUseFxCan(%client, %index);
	}
};



//Fill wrench
///////////////////////////////////////////////////////////////////////////

//Open the fill wrench gui
function serverCmdFillWrench(%client)
{
	//Check version
	if(!%client.ndClient)
	{
		messageClient(%client, '', "\c6You need to have the new duplicator installed to use Fill Wrench.");
		return;
	}

	if(ndCompareVersion("1.2.0", %client.ndVersion) == 1)
	{
		messageClient(%client, '', "\c6Your version of the new duplicator is too old to use Fill Wrench.");
		return;
	}

	//Check admin
	if($Pref::Server::ND::WrenchAdminOnly && !%client.isAdmin)
	{
		messageClient(%client, '', "\c6Fill Wrench is admin only. Ask an admin for help.");
		return;
	}

	//Check mode
	if(%client.ndModeIndex != $NDM::StackSelect && %client.ndModeIndex != $NDM::BoxSelect)
	{
		messageClient(%client, '', "\c6Fill Wrench can only be used in Selection Mode.");
		return;
	}

	//Check selection
	if(!isObject(%client.ndSelection) || !%client.ndSelection.brickCount)
	{
		messageClient(%client, '', "\c6Fill Wrench can only be used with a selection.");
		return;
	}

	//Open fill wrench gui
	commandToClient(%client, 'ndOpenWrenchGui');
}

//Short command
function serverCmdFW(%client) {serverCmdFillWrench(%client);}

//Send data from gui
function serverCmdNdStartFillWrench(%client, %data)
{
	//Check admin
	if($Pref::Server::ND::WrenchAdminOnly && !%client.isAdmin)
	{
		messageClient(%client, '', "\c6Fill Wrench is admin only. Ask an admin for help.");
		return;
	}

	//Check mode
	if(%client.ndModeIndex != $NDM::StackSelect && %client.ndModeIndex != $NDM::BoxSelect)
	{
		messageClient(%client, '', "\c6Fill Wrench can only be used in Selection Mode.");
		return;
	}

	//Check selection
	if(!isObject(%client.ndSelection) || !%client.ndSelection.brickCount)
	{
		messageClient(%client, '', "\c6Fill Wrench can only be used with a selection.");
		return;
	}

	//Change mode
	%client.ndSetMode(NDM_WrenchProgress);
	%client.ndSelection.startFillWrench(%data);
}



//Saving and loading
///////////////////////////////////////////////////////////////////////////

package NewDuplicator_Server_Final
{
	//Save current selection to file
	function serverCmdSaveDup(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
	{
		//Check timeout
		if(!%client.isAdmin && %client.ndLastSaveTime + 10 > $Sim::Time)
		{
			%remain = mCeil(%client.ndLastSaveTime + 10 - $Sim::Time);

			if(%remain != 1)
				%s = "s";

			messageClient(%client, '', "\c6Please wait\c3 " @ %remain @ "\c6 second" @ %s @ " before saving again!");
			return;
		}

		//Check admin
		if($Pref::Server::ND::SaveAdminOnly && !%client.isAdmin)
		{
			messageClient(%client, '', "\c6Saving duplications is admin only. Ask an admin for help.");
			return;
		}

		//Check mode
		if(%client.ndModeIndex != $NDM::PlantCopy)
		{
			messageClient(%client, '', "\c6Saving duplications can only be used in Plant Mode.");
			return;
		}

		//Filter file name
		%allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ._-()";
		%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);
		%filePath = $ND::ConfigPath @ "Saves/" @ %fileName @ ".bls";
		%filePath = strReplace(%filePath, ".bls.bls", ".bls");

		for(%i = 0; %i < strLen(%fileName); %i++)
		{
			if(strStr(%allowed, getSubStr(%fileName, %i, 1)) == -1)
			{
				%forbidden = true;
				break;
			}
		}

		if(%forbidden || !strLen(%fileName) || strLen(%fileName) > 50)
		{
			messageClient(%client, '', "\c6Bad save name \"\c3" @ %fileName @ "\c6\", please try again.");
			messageClient(%client, '', "\c6Only \c3a-z A-Z 0-9 ._-()\c6 and \c3space\c6 are allowed, with a max length of 50 characters.");
			return;
		}

		//Check overwrite
		if(isFile(%filePath) && %client.ndPotentialOverwrite !$= %fileName)
		{
			messageClient(%client, '', "\c6Save \"\c3" @ %fileName @ "\c6\" already exists. Repeat the command to overwrite.");
			%client.ndPotentialOverwrite = %fileName;
			return;
		}

		%client.ndPotentialOverwrite = "";

		//Check writeable
		if(!isWriteableFileName(%filePath))
		{
			messageClient(%client, '', "\c6File \"\c3" @ %fileName @ "\c6\" is not writeable. Ask the host for help.");
			return;
		}

		messageClient(%client, '', "\c6Saving selection to \"\c3" @ %fileName @ "\c6\"...");

		//Notify admins
		if(!%client.isAdmin)
		{
			for(%i = 0; %i < ClientGroup.getCount(); %i++)
			{
				%cl = ClientGroup.getObject(%i);

				if(%cl.isAdmin && %cl != %client)
					messageClient(%cl, '', "\c3" @ %client.name @ "\c6 is saving duplication \"\c3" @ %fileName @ "\c6\"");
			}
		}

		//Write log
		echo("ND: " @ %client.name @ " (" @ %client.bl_id @ ") is saving duplication \"" @ %fileName @ "\"");

		//Change mode
		%client.ndSetMode(NDM_SaveProgress);

		if(!%client.ndSelection.startSaving(%filePath))
		{
			messageClient(%client, '', "\c6Failed to write save \"\c3" @ %fileName @ "\c6\". Ask the host for help.");
			%client.ndSetMode(NDM_PlantCopy);
		}
	}

	//Load selection from file
	function serverCmdLoadDup(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
	{
		//Check timeout
		if(!%client.isAdmin && %client.ndLastLoadTime + 5 > $Sim::Time)
		{
			%remain = mCeil(%client.ndLastLoadTime + 5 - $Sim::Time);

			if(%remain != 1)
				%s = "s";

			messageClient(%client, '', "\c6Please wait\c3 " @ %remain @ "\c6 second" @ %s @ " before loading again!");
			return;
		}

		//Check admin
		if($Pref::Server::ND::LoadAdminOnly && !%client.isAdmin)
		{
			messageClient(%client, '', "\c6Loading duplications is admin only. Ask an admin for help.");
			return;
		}

		//Attempt to get a duplicator
		if(!%client.ndEquipped)
		{
			serverCmdNewDuplicator(%client);

			if(!%client.ndEquipped)
				return;
		}

		//Check mode
		%mode = %client.ndModeIndex;

		if(%mode != $NDM::StackSelect && %mode != $NDM::BoxSelect && %mode != $NDM::PlantCopy)
		{
			messageClient(%client, '', "\c6Loading duplications can only be used in Plant or Selection Mode.");
			return;
		}

		//Filter file name
		%allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ._-()";
		%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);
		%filePath = $ND::ConfigPath @ "Saves/" @ %fileName @ ".bls";
		%filePath = strReplace(%filePath, ".bls.bls", ".bls");

		for(%i = 0; %i < strLen(%fileName); %i++)
		{
			if(strStr(%allowed, getSubStr(%fileName, %i, 1)) == -1)
			{
				%forbidden = true;
				break;
			}
		}

		if(%forbidden || !strLen(%fileName) || strLen(%fileName) > 50)
		{
			messageClient(%client, '', "\c6Bad save name \"\c3" @ %fileName @ "\c6\", please try again.");
			messageClient(%client, '', "\c6Only \c3a-z A-Z 0-9 ._-()\c6 and \c3space\c6 are allowed, with a max length of 50 characters.");
			return;
		}

		//Check if file exists
		if(!isFile(%filePath))
		{
			messageClient(%client, '', "\c6Save \"\c3" @ %fileName @ "\c6\" does not exist, please try again.");
			return;
		}

		messageClient(%client, '', "\c6Loading selection from \"\c3" @ %fileName @ "\c6\"...");

		//Notify admins
		if(!%client.isAdmin)
		{
			for(%i = 0; %i < ClientGroup.getCount(); %i++)
			{
				%cl = ClientGroup.getObject(%i);

				if(%cl.isAdmin && %cl != %client)
					messageClient(%cl, '', "\c3" @ %client.name @ "\c6 is loading duplication \"\c3" @ %fileName @ "\c6\"");
			}
		}

		//Write log
		echo("ND: " @ %client.name @ " (" @ %client.bl_id @ ") is loading duplication \"" @ %fileName @ "\"");

		//Change mode
		%client.ndSetMode(NDM_LoadProgress);

		if(!%client.ndSelection.startLoading(%filePath))
		{
			messageClient(%client, '', "\c6Failed to read save \"\c3" @ %fileName @ "\c6\". Ask the host for help.");
			%client.ndSetMode(%client.ndLastSelectMode);
		}
	}
};

//Get list of all available dups
function serverCmdAllDups(%client, %pattern)
{
	//Check admin
	if($Pref::Server::ND::LoadAdminOnly && !%client.isAdmin)
	{
		messageClient(%client, '', "\c6Loading duplications is admin only. Ask an admin for help.");
		return;
	}

	if(strLen(%pattern))
	{
		//Filter pattern
		%allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._-()";
		%pattern = trim(%pattern);

		for(%i = 0; %i < strLen(%pattern); %i++)
		{
			if(strStr(%allowed, getSubStr(%pattern, %i, 1)) == -1)
			{
				messageClient(%client, '', "\c6Bad pattern \"\c3" @ %pattern @ "\c6\", please try again.");
				messageClient(%client, '', "\c6Only \c3a-z A-Z 0-9 ._-()\c6 are allowed.");
				return;
			}
		}

		%p = $ND::ConfigPath @ "Saves/*" @ %pattern @ "*.bls";
	}
	else
		%p = $ND::ConfigPath @ "Saves/*.bls";

	//Get sorted list of files
	%sort = new GuiTextListCtrl();

	for(%i = findFirstFile(%p); isFile(%i); %i = findNextFile(%p))
		%sort.addRow(0, fileBase(%i));

	%fileCount = %sort.rowCount();
	%sort.sort(0, 1);

	//Dump list to client
	if(%fileCount)
	{
		%s = (%fileCount == 1) ? " is" : "s are";

		if(strLen(%pattern))
			messageClient(%client, '', "\c3" @ %fileCount @ "\c6 saved duplication" @ %s @ " available for filter \"\c3" @ %pattern @ "\c6\":");
		else
			messageClient(%client, '', "\c3" @ %fileCount @ "\c6 saved duplication" @ %s @ " available:");

		for(%i = 0; %i < %fileCount; %i++)
			messageClient(%client, '', "\c6 - \c3" @ %sort.getRowText(%i));
	}
	else
	{
		if(strLen(%pattern))
			messageClient(%client, '', "\c6No saved duplications are available for filter \"\c3" @ %pattern @ "\c6\".");
		else
			messageClient(%client, '', "\c6No saved duplications are available.");
	}

	%sort.delete();

	messageClient(%client, '', "\c6Scroll using \c3PageUp\c6 and \c3PageDown\c6 if you can't see the whole list.");
}

//Alternative short commands
function serverCmdSD(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7) {serverCmdSaveDup(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7);}
function serverCmdLD(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7) {serverCmdLoadDup(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7);}
function serverCmdAD(%client, %pattern) {serverCmdAllDups(%client, %pattern);}



//Admin commands
///////////////////////////////////////////////////////////////////////////

//Cancel all active dups in case of spamming
function serverCmdClearDups(%client)
{
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "\c6Canceling all duplicators is admin only. Ask an admin for help.");
		return;
	}

	messageAll('MsgClearBricks', "\c3" @ %client.getPlayerName() @ "\c0 canceled all duplicators.");

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);

		if(%cl.ndModeIndex)
			%cl.ndKillMode();
	}
}
