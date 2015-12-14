// * ######################################################################
// *
// *    New Duplicator - Scripts - Server 
// *    Duplicator
// *
// *    -------------------------------------------------------------------
// *    Handles general functions that don't fit in a specific class
// *
// * ######################################################################

//Connecting, disconnecting, death
///////////////////////////////////////////////////////////////////////////

package NewDuplicator_Server
{
	//Set initial variables on join
	function GameConnection::onClientEnterGame(%this)
	{
		%this.ndPivot     = true;
		%this.ndLimited   = true;
		%this.ndDirection = true;

		%this.ndMode           = NDM_Disabled;
		%this.ndModeIndex      = $NDM::Disabled;
		%this.ndLastSelectMode = NDM_StackSelect;

		parent::onClientEnterGame(%this);
	}

	//Kill duplicator mode when a client leaves
	function GameConnection::onClientLeaveGame(%this)
	{
		if(%this.ndModeIndex)
			%this.ndKillMode(%this);

		//Remove from client lists of selections
		for(%i = 0; %i < ND_ServerGroup.getCount(); %i++)
		{
			%obj = ND_ServerGroup.getObject(%i);

			if(%obj.getName() $= "ND_Selection")
			{				
				%obj.numClients = 0;

				for(%j = 0; %j < ClientGroup.getCount(); %j++)
				{
					%cl = ClientGroup.getObject(%j);

					if(%cl.getId() != %this.getId()
					&& %cl.hasSpawnedOnce
					&& isObject(%ctrl = %cl.getControlObject())
					&& vectorDist(%obj.ghostPosition, %ctrl.getTransform()) < 10000)
					{
						$NS[%obj, "CL", %obj.numClients] = %cl;
						%obj.numClients++;
					}
				}
			}
		}

		parent::onClientLeaveGame(%this);
	}

	//Kill duplicator mode when a player dies
	function GameConnection::onDeath(%this, %a, %b, %c, %d)
	{
		if(%this.ndModeIndex)
			%this.ndKillMode(%this);

		%this.ndEquipped = false;

		parent::onDeath(%this, %a, %b, %c, %d);
	}

	//Kill duplicator mode when a player is force respawned
	function GameConnection::spawnPlayer(%this)
	{
		if(%this.ndModeIndex)
			%this.ndKillMode(%this);

		%this.ndEquipped = false;

		parent::spawnPlayer(%this);
	}
};



//Duplicator modes and bottomprints
///////////////////////////////////////////////////////////////////////////

//Change duplication mode
function GameConnection::ndSetMode(%this, %newMode)
{
	%oldMode = %this.ndMode;

	if(%oldMode.index == %newMode.index)
		return;

	%this.ndMode      = %newMode;
	%this.ndModeIndex = %newMode.index;

	%oldMode.onChangeMode(%this, %newMode.index);
	%newMode.onStartMode(%this, %oldMode.index);

	//Enable keybinds
	if(!%oldMode.index)
		commandToClient(%this, 'ndEnableKeybinds', true);
}

//Kill duplication mode
function GameConnection::ndKillMode(%this)
{
	if(!%this.ndModeIndex)
		return;

	%this.ndMode.onKillMode(%this);

	%this.ndMode = NDM_Disabled;
	%this.ndModeIndex = $NDM::Disabled;

	%this.ndUpdateBottomPrint();

	//Disable keybinds
	commandToClient(%this, 'ndEnableKeybinds', false);
}

//Update the bottomprint
function GameConnection::ndUpdateBottomPrint(%this)
{
	if(%this.ndModeIndex)
		commandToClient(%this, 'bottomPrint', %this.ndMode.getBottomPrint(%this), 0, true);
	else
		commandToClient(%this, 'clearBottomPrint');
}

//Format bottomprint message with left and right justified text
function ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2, %r2)
{
	%message = "<font:Arial:22>";

	//Last used alignment, false = left | true = right
	%align = false;

	if(strPos("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%title, 0, 1)) < 0)
		%message = %message @ "\c6";

	%message = %message @ %title @ "\n<font:Verdana:16>";

	for(%i = 0; strLen(%l[%i]) || strLen(%r[%i]); %i++)
	{
		if(strLen(%l[%i]))
		{
			if(%align)
				%message = %message @ "<just:left>";

			if(strPos("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%l[%i], 0, 1)) < 0)
				%message = %message @ "\c6";

			%message = %message @ %l[%i];
			%align = false;
		}

		if(strLen(%r[%i]))
		{
			if(!%align)
				%message = %message @ "<just:right>";

			if(strPos("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%r[%i], 0, 1)) < 0)
				%message = %message @ "\c6";

			%message = %message @ %r[%i] @ " ";
			%align = true;
		}

		%message = %message @ "\n";
	}

	return %message @ " ";
}



//Duplicator commands
///////////////////////////////////////////////////////////////////////////

//Cancel active dups (admin command)
function serverCmdClearDups(%client)
{
	if(!%client.isAdmin)
		return;

	messageAll('MsgClearBricks', "\c3" @ %client.getPlayerName() @ "\c0 cleared all dups.");

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);

		if(%cl.ndModeIndex)
			%cl.ndKillMode();
	}
}

//Short commands to equip a duplicator
function serverCmdDuplicator(%this, %cmd){%this.ndHandleCommand(%cmd);}
function serverCmdDuplicato (%this, %cmd){%this.ndHandleCommand(%cmd);}
function serverCmdDuplicat  (%this, %cmd){%this.ndHandleCommand(%cmd);}
function serverCmdDuplica   (%this, %cmd){%this.ndHandleCommand(%cmd);}
function serverCmdDuplic    (%this, %cmd){%this.ndHandleCommand(%cmd);}
function serverCmdDupli     (%this, %cmd){%this.ndHandleCommand(%cmd);}
function serverCmdDupl      (%this, %cmd){%this.ndHandleCommand(%cmd);}
function serverCmdDup       (%this, %cmd){%this.ndHandleCommand(%cmd);}
function serverCmdDu        (%this, %cmd){%this.ndHandleCommand(%cmd);}
function serverCmdD         (%this, %cmd){%this.ndHandleCommand(%cmd);}

//Equip a duplicator or show information
function GameConnection::ndHandleCommand(%this, %cmd)
{
	switch$(%cmd)
	{
		case "version" or "v":
			messageClient(%this, '', "\c6Blockland version: \c3r" @ getBuildNumber());
			messageClient(%this, '', "\c6New Duplicator version: \c3" @ $ND::Version);

			if(%this.ndClient)
				messageClient(%this, '', "\c6Your New Duplicator version: \c3" @ %this.ndVersion);
			else
				messageClient(%this, '', "\c6You don't have the New Duplicator installed");

		case "clients" or "c":
			messageClient(%this, '', "\c6New Duplicator versions:");

			%cnt = ClientGroup.getCount();
			for(%i = 0; %i < %cnt; %i++)
			{
				%client = ClientGroup.getObject(%i);

				if(%client.ndClient)
					messageClient(%this, '', "\c3" @ %client.name @ "\c6 has version \c3" @ %client.ndVersion);
				else
					messageClient(%this, '', "\c3" @ %client.name @ "\c6 doesn't have it installed");
			}

		case "prefs" or "p":
			ND_PrefManager.dumpPrefs(%this);

		default:
			if(!isObject(%player = %this.player))
			{
				messageClient(%this, '', "\c6You must be alive to use the duplicator!");
				return;
			}

			if($Pref::Server::ND::AdminOnly && !%this.isAdmin)
			{
				messageClient(%this, '', "\c6You must be admin to use the duplicator!");
				return;
			}

			%minigame = getMinigameFromObject(%this);

			if(isObject(%minigame) && !%minigame.enablebuilding)
			{
				messageClient(%this, '', "\c6Building must be enabled to use the duplicator!");
				return;
			}

			//Hide brick selector and tool gui
			%this.ndLastEquipTime = $Sim::Time;
			commandToClient(%this, 'setScrollMode', 3);

			//Give player a duplicator
			if(%this.ndBlueImage)
				%image = NewDuplicatorBlueImage.getId();
			else
				%image = NewDuplicatorImage.getId();

			%player.updateArm(%image);
			%player.mountImage(%image, 0);
	}
}

package NewDuplicator_Server
{
	//Prevent accidently unequipping the duplicator
	function serverCmdUnUseTool(%client)
	{
		if(%client.ndLastEquipTime + 1.5 > $Sim::Time)
			return;

		parent::serverCmdUnUseTool(%client);
	}



//Keybinds used to control the duplicator
///////////////////////////////////////////////////////////////////////////

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
		else
			parent::serverCmdShiftBrick(%client, %x, %y, %z);
	}

	//Super-shifting the ghost brick (default: alt numpad 2468/5+)
	function serverCmdSuperShiftBrick(%client, %x, %y, %z)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onSuperShiftBrick(%client, %x, %y, %z);
		else
			parent::serverCmdSuperShiftBrick(%client, %x, %y, %z);
	}

	//Rotating the ghost brick (default: numpad 79)
	function serverCmdRotateBrick(%client, %direction)
	{
		if(%client.ndModeIndex)
			%client.ndMode.onRotateBrick(%client, %direction);
		else
			parent::serverCmdRotateBrick(%client, %direction);
	}

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

	//Undo bricks (default: ctrl z)
	function serverCmdUndoBrick(%client)
	{
		if(%client.ndUndoInProgress)
			return;

		%state = %client.undoStack.pop();

		if(getField(%state, 1) $= "DUPLICATE")
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

//Cut selection (command)
function serverCmdCut(%client)
{
	serverCmdNdCut(%client);
}



//Undo bricks
///////////////////////////////////////////////////////////////////////////

//Start undo bricks
function SimSet::ndStartUndo(%this, %client)
{
	%this.brickCount = %this.getCount();

	if(!%this.brickCount)
	{
		%this.delete();
		return;
	}

	%client.ndUndoInProgress = true;
	%client.ndLastMessageTime = $Sim::Time;
	%this.ndTickUndo(%this.brickCount, %this.getCount() > 1500, %client);
}

//Tick undo bricks
function SimSet::ndTickUndo(%this, %count, %instant, %client)
{
	if(%count > %this.getCount())
		%start = %this.getCount();
	else
		%start = %count;

	if(%start > $Pref::Server::ND::ProcessPerTick)
		%end = %start - $Pref::Server::ND::ProcessPerTick;
	else
		%end = 0;

	for(%i = %start - 1; %i >= %end; %i--)
	{
		%brick = %this.getObject(%i);
		%brick.killBrick();

		//Instantly delete bricks if we have many thousand left to
		//prevent killBrick() from hogging schedules on the server
		if(%instant)
			%brick.delete();
	}

	//If undo is taking long, tell the client how far we get
	if(%client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%client.ndLastMessageTime = $Sim::Time;

		%percent = mCeil(100 - (%end * 100 / %this.brickCount));
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Undo in progress...\n<font:Verdana:17>\c3" @ %percent @ "%\c6 finished.", 1);
	}

	if(%end <= 0)
	{
		%this.delete();
		%client.ndUndoInProgress = false;

		return;
	}
	
	%this.schedule(30, ndTickUndo, %end, %instant, %client);
}



//Mirrors
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
	}
}

//Shorter commands to do the same thing
function serverCmdMirrX(%client){serverCmdMirrorX(%client);}
function serverCmdMirrY(%client){serverCmdMirrorY(%client);}
function serverCmdMirrZ(%client){serverCmdMirrorZ(%client);}

function serverCmdMirX(%client){serverCmdMirrorX(%client);}
function serverCmdMirY(%client){serverCmdMirrorY(%client);}
function serverCmdMirZ(%client){serverCmdMirrorZ(%client);}

function serverCmdMirr(%client, %a){serverCmdMirror(%client, %a);}
function serverCmdMir(%client, %a){serverCmdMirror(%client, %a);}

//Attempt to mirror selection on axis
function GameConnection::ndMirror(%client, %axis)
{
	if(!isObject(%client.ndSelection) || %client.ndModeIndex != $NDM::PlantCopy)
		return;

	//Make sure symmetry table is created
	if(!$ND::SymmetryTableCreated)
	{
		if(!isObject(ND_SymmetryTable))
			ND_SymmetryTable();

		if(!$ND::SymmetryTableCreating)
			ND_SymmetryTable.buildTable();
	}
	else
		%client.ndSelection.mirrorGhostBricks(%axis);
}

function serverCmdMirErrors(%client)
{
	%xerr = $NS[%client, "MirErrorsX"];
	%zerr = $NS[%client, "MirErrorsZ"];

	if(%xerr)
	{
		messageClient(%client, '', " ");
		messageClient(%client, '', "\c6These bricks are asymmetric and probably mirrored incorrectly:");

		for(%i = 0; %i < %xerr; %i++)
		{
			%db = $NS[%client, "MirErrorX", %i];
			messageClient(%client, '', "\c7 -" @ %i + 1 @ "- \c6" @ %db.category @ "/" @ %db.subCategory @ "/" @ %db.uiName);
		}
	}

	if(%zerr)
	{
		messageClient(%client, '', " ");
		messageClient(%client, '', "\c6These bricks are not symmetric on Z and probably incorrect:");

		for(%i = 0; %i < %zerr; %i++)
		{
			%db = $NS[%client, "MirErrorZ", %i];
			messageClient(%client, '', "\c7 -" @ %i + 1 @ "- \c6" @ %db.category @ "/" @ %db.subCategory @ "/" @ %db.uiName);
		}		
	}

	if(!%xerr && !%zerr)
		messageClient(%client, '', "\c6There were no mirror errors in your last plant attempt.");
}



//General support functions
///////////////////////////////////////////////////////////////////////////

package NewDuplicator_Server
{
	//Prevent creating ghost bricks in modes that allow un-mount
	function BrickDeployProjectile::onCollision(%this, %obj, %col, %fade, %pos, %normal)
	{
		%client = %obj.client;

		if(isObject(%client) && %client.ndModeIndex)
			%client.ndMode.onSelectObject(%client, %col, %pos, %normal);
		else
			parent::onCollision(%this, %obj, %col, %fade, %pos, %normal);
	}
};

//Rotate vector around +Z in 90 degree steps
function ndRotateVector(%vector, %steps)
{
	switch(%steps % 4)
	{
		case 0: return %vector;
		case 1: return  getWord(%vector, 1) SPC -getWord(%vector, 0) SPC getWord(%vector, 2);
		case 2: return -getWord(%vector, 0) SPC -getWord(%vector, 1) SPC getWord(%vector, 2);
		case 3: return -getWord(%vector, 1) SPC  getWord(%vector, 0) SPC getWord(%vector, 2);
	}
}

//Send a message if a client doesn't have trust to a brick
function ndTrustCheckMessage(%obj, %client)
{
	%group = %client.brickGroup.getId();
	%bl_id = %client.bl_id;
	%admin = %client.isAdmin;

	if(ndTrustCheckSelection(%obj, %group, %bl_id, %admin))
		return true;

	messageClient(%client, 'MsgError', "");
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6You don't have enough trust to do that!", 5);
	return false;
}

//Check whether a client has enough trust to select a brick
function ndTrustCheckSelection(%obj, %group2, %bl_id, %admin)
{
	%group1 = %obj.getGroup();

	//Client owns brick
	if(%group1 == %group2)
		return true;

	//Client owns stack
	if(%obj.stackBL_ID == %bl_id)
		return true;

	//Client has trust to the brick
	if(%group1.Trust[%bl_id] >= $Pref::Server::ND::TrustLimit)
		return true;

	//Client has trust to the stack of the brick
	if(%group2.Trust[%obj.stackBL_ID] >= $Pref::Server::ND::TrustLimit)
		return true;

	//Client is admin
	if(%admin && !$Pref::Server::ND::AdminTrustRequired)
		return true;

	//Client can duplicate public bricks
	if(%group1.bl_id == 888888 && $Pref::Server::ND::SelectPublicBricks)
		return true;

	return false;
}

//Check whether a client has enough trust to cut a brick
function ndTrustCheckCut(%obj, %group2, %bl_id)
{
	%group1 = %obj.getGroup();

	//Client owns brick
	if(%group1 == %group2)
		return true;

	//Client owns stack
	if(%obj.stackBL_ID == %bl_id)
		return true;

	//Client has trust to the brick
	if(%group1.Trust[%bl_id] >= 2)
		return true;

	//Client has trust to the stack of the brick
	if(%group2.Trust[%obj.stackBL_ID] >= 2)
		return true;

	return false;
}

//Get the closest paint color to an rgb value
function ndGetClosestColorID(%rgb)
{
	//Set initial value
	%color = getColorI(getColorIdTable(0));

	%best = 0;
	%bestDiff = vectorLen(vectorSub(%rgb, %color));

	if(getWord(%color, 3) != 255)
		%bestDiff += 1000;

	for(%i = 1; %i < 64; %i++)
	{
		%color = getColorI(getColorIdTable(%i));

		%diff = vectorLen(vectorSub(%rgb, %color));

		if(getWord(%color, 3) != 255)
			%diff += 1000;

		if(%diff < %bestDiff)
		{
			%best = %i;
			%bestDiff = %diff;
		}
	}

	return %best;
}

