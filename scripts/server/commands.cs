// * ######################################################################
// *
// *    New Duplicator - Scripts - Server 
// *    Commands
// *
// *    -------------------------------------------------------------------
// *    Server commands to control the new duplicator go here
// *
// * ######################################################################

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



//Equip commands
///////////////////////////////////////////////////////////////////////////

//Command to equip the old duplicator
function serverCmdOldDup(%client)
{
	if(!isObject(%client.player))
	{
		messageClient(%client, '', "\c6You must be spawned to equip the old duplicator.");
		return;
	}

	if(isObject(DuplorcatorImage))
	{
		%client.player.updateArm(DuplorcatorImage);
		%client.player.mountImage(DuplorcatorImage, 0);
	}
	else if(isObject(DuplicatorImage))
	{
		%client.player.updateArm(DuplicatorImage);
		%client.player.mountImage(DuplicatorImage, 0);
	}
	else
		messageClient(%client, '', "\c6The server does not have an old duplicator installed.");
}

//Command to equip the new duplicator
function serverCmdNewDuplicator(%client, %cmd)
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
			(getField(%state, 0)).ndStartUndo(%client);

			if(isObject(%client.player))
				%client.player.playThread(3, "undo");

			return;
		}

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

//Alternative command with space
function serverCmdMirror(%client, %a)
{
	switch$(%a)
	{
		case "X": serverCmdMirrorX(%client);
		case "Y": serverCmdMirrorY(%client);
		case "Z": serverCmdMirrorZ(%client);
		default: messageClient(%client, '', "\c6Please specify a mirror axis: \c3/mirror [X, Y, Z]");
	}
}

//Alternative shorter commands
function serverCmdMirX(%client){serverCmdMirrorX(%client);}
function serverCmdMirY(%client){serverCmdMirrorY(%client);}
function serverCmdMirZ(%client){serverCmdMirrorZ(%client);}
function serverCmdMir(%client, %a){serverCmdMirror(%client, %a);}

function serverCmdMX(%client){serverCmdMirrorX(%client);}
function serverCmdMY(%client){serverCmdMirrorY(%client);}
function serverCmdMZ(%client){serverCmdMirrorZ(%client);}
function serverCmdM(%client, %a){serverCmdMirror(%client, %a);}

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

		if(%mode == $NDM::StackSelect || %mode == $NDM::CubeSelect)
		{
			if(isObject(%client.ndSelection) && %client.ndSelection.brickCount)
			{
				%client.currentColor = %index;
				%client.currentFxColor = "";
				%client.ndSetMode(NDM_FillColor);
			}
			else
				parent::serverCmdUseSprayCan(%client, %index);
		}
		else if(%mode == $NDM::FillColor || %client.ndModeIndex == $NDM::FillColorProgress)
		{
			%client.currentColor = %index;
			%client.currentFxColor = "";
			%client.ndUpdateBottomPrint();
		}
		else
			parent::serverCmdUseSprayCan(%client, %index);
	}

	//Enable fill color mode or show the current color
	function serverCmdUseFxCan(%client, %index)
	{
		%mode = %client.ndModeIndex;

		if(%mode == $NDM::StackSelect || %mode == $NDM::CubeSelect)
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
	if(%client.ndModeIndex != $NDM::StackSelect && %client.ndModeIndex != $NDM::CubeSelect)
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
	if(%client.ndModeIndex != $NDM::StackSelect && %client.ndModeIndex != $NDM::CubeSelect)
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
