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



//Duplicator command
///////////////////////////////////////////////////////////////////////////

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



//Existing keybinds used to control the duplicator
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
	%this.ndTickUndo(%this.brickCount, %client);
}

//Tick undo bricks
function SimSet::ndTickUndo(%this, %count, %client)
{
	if(%count > %this.getCount())
		%start = %this.getCount();
	else
		%start = %count;

	if(%start > $Pref::Server::ND::PlantBricksPerTick)
		%end = %start - $Pref::Server::ND::PlantBricksPerTick;
	else
		%end = 0;

	%instant = %start > 5000;

	for(%i = %start - 1; %i >= %end; %i--)
	{
		%brick = %this.getObject(%i);

		if(!%brick.isDead())
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
	
	%this.schedule($Pref::Server::ND::PlantBricksTickDelay, ndUndoTick, %end, %client);
}



//General support functions
///////////////////////////////////////////////////////////////////////////

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

//Send a message if a client doesn't have trust to a brick
function ndTrustCheckMessage(%obj, %client)
{
	%group1 = %obj.getGroup();
	%group2 = %client.brickGroup;
	%bl_id = %client.bl_id;
	%admin = %client.isAdmin;

	if(ndTrustCheck(%obj, %admin, %group1, %group2, %client.bl_id))
		return true;

	messageClient(%client, 'MsgError', "");
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6You don't have enough trust to do that!", 5);
	return false;
}

//Check whether a client has enough trust to a brick
function ndTrustCheck(%obj, %admin, %group1, %group2, %bl_id)
{
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
	if(%group1.isPublicDomain && $Pref::Server::ND::SelectPublicBricks)
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
