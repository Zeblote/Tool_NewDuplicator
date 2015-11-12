// * ######################################################################
// *
// *    New Duplicator - Scripts - Server 
// *    Duplicator
// *
// *    -------------------------------------------------------------------
// *    Functions for the duplicator
// *
// * ######################################################################

package NewDuplicator_Server
{
	//Set initial variables on join
	function GameConnection::onClientEnterGame(%this)
	{
		%this.ndMode = NDDM_Disabled;
		%this.ndModeNum = $NDDM::Disabled;
		%this.ndDirection = true;
		%this.ndLimited = true;
		%this.ndPivot = true;

		parent::onClientEnterGame(%this);
	}

	//Reset dupli mode when a client dies
	function GameConnection::onDeath(%this, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		if(%this.ndModeNum)
			%this.ndSetMode(NDDM_Disabled);

		parent::onDeath(%this, %sourceObject, %sourceClient, %damageType, %damLoc);
	}

	//Reset dupli mode before a client leaves (changing to disabled will clean up all data)
	function GameConnection::onClientLeaveGame(%this)
	{
		if(%this.ndModeNum)
			%this.ndSetMode(NDDM_Disabled);

		parent::onClientLeaveGame(%this);
	}
};

//Change duplication mode
function GameConnection::ndSetMode(%this, %newMode)
{
	%oldMode = %this.ndMode;

	//echo("Change dupli mode for " @ %this.getPlayerName() @ " from " @ %oldMode.getName() @ " to " @ %newMode.getName());

	//Can't change to same mode
	if(%oldMode.getId() == %newMode.getId())
	{
		error("Already set to dupli mode " @ %newMode.getName() @ "!");
		return;
	}

	//Only change to possible following modes
	if((%oldMode.allowedModes & %newMode.num) == 0 && %newMode.num != $NDDM::Disabled)
	{
		error("Changing dupli mode from " @ %oldMode.getName() @ " to " @ %newMode.getName() @ " is not allowed!");
		return;
	}

	//First end the current mode
	%oldMode.onChangeMode(%this, %newMode.num);

	%this.ndMode = %newMode;
	%this.ndModeNum = %newMode.num;

	//Then start the new mode
	%newMode.onStartMode(%this, %oldMode.num);
}

//Format bottomprint message with left and right justified text
function ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2, %r2, %l3, %r3, %l4, %r4, %l5, %r5)
{
	%lastAlign = false;
	%message = "<font:Arial:22>";

	if(strPos("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%title, 0, 1)) < 0)
		%message = %message @ "\c6";

	%message = %message @ %title @ "\n<font:Verdana:16>";

	for(%i = 0; strLen(%l[%i]) || strLen(%r[%i]); %i++)
	{
		if(strLen(%l[%i]))
		{
			if(%lastAlign)
				%message = %message @ "<just:left>";

			if(strPos("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%l[%i], 0, 1)) < 0)
				%message = %message @ "\c6";

			%message = %message @ %l[%i];
			%lastAlign = false;
		}

		if(strLen(%r[%i]))
		{
			if(!%lastAlign)
				%message = %message @ "<just:right>";

			if(strPos("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%r[%i], 0, 1)) < 0)
				%message = %message @ "\c6";

			%message = %message @ %r[%i] @ " ";
			%lastAlign = true;
		}

		%message = %message @ "\n";
	}

	%message = %message @ " ";
	return %message;
}

//Update the bottomprint
function GameConnection::ndUpdateBottomPrint(%this)
{
	commandToClient(%this, 'bottomPrint', %this.ndMode.getBottomPrint(%this), 0, true);
}

//Commands and keybinds
///////////////////////////////////////////////////////////////////////////

//Equip a duplicator
function GameConnection::ndHandleCommand(%this, %a)
{
	switch$(%a)
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

			if(!isObject(%this.player))
			{
				messageClient(%this, '', "\c6You must be alive to use the duplicator!");
				return;
			}

			if($ND::AdminOnly && !%this.isAdmin)
			{
				messageClient(%this, '', "\c6You must be admin to use the duplicator!");
				return;
			}

			//Give player a duplicator
			%this.player.updateArm(ND_DupliImage);
			%this.player.mountImage(ND_DupliImage, 0);

			//Hide brick selector if possible
			commandToClient(%this, 'setScrollMode', 3);
	}
}

//Commands to equip a duplicator
function serverCmdDuplicator(%this, %a){%this.ndHandleCommand(%a);}
function serverCmdDuplicato (%this, %a){%this.ndHandleCommand(%a);}
function serverCmdDuplicat  (%this, %a){%this.ndHandleCommand(%a);}
function serverCmdDuplica   (%this, %a){%this.ndHandleCommand(%a);}
function serverCmdDuplic    (%this, %a){%this.ndHandleCommand(%a);}
function serverCmdDupli     (%this, %a){%this.ndHandleCommand(%a);}
function serverCmdDupl      (%this, %a){%this.ndHandleCommand(%a);}
function serverCmdDup       (%this, %a){%this.ndHandleCommand(%a);}
function serverCmdDu        (%this, %a){%this.ndHandleCommand(%a);}
function serverCmdD         (%this, %a){%this.ndHandleCommand(%a);}

//Existing keybinds used to control the duplicator
package NewDuplicator_Server
{
	//Light key (default: L)
	function serverCmdLight(%client)
	{
		if(%client.ndModeNum)
			%client.ndMode.onLight(%client);
		else
			parent::serverCmdLight(%client);
	}

	//Next seat (default: .)
	function serverCmdNextSeat(%client)
	{
		if(%client.ndModeNum)
			%client.ndMode.onNextSeat(%client);
		else
			parent::serverCmdNextSeat(%client);
	}

	//Previous seat (default: ,)
	function serverCmdPrevSeat(%client)
	{
		if(%client.ndModeNum)
			%client.ndMode.onPrevSeat(%client);
		else
			parent::serverCmdPrevSeat(%client);
	}

	//Shifting the ghost brick (default: numpad 2468/13/5+)
	function serverCmdShiftBrick(%client, %x, %y, %z)
	{
		if(%client.ndModeNum)
			%client.ndMode.onShiftBrick(%client, %x, %y, %z);
		else
			parent::serverCmdShiftBrick(%client, %x, %y, %z);
	}

	//Super-shifting the ghost brick (default: alt numpad 2468/5+)
	function serverCmdSuperShiftBrick(%client, %x, %y, %z)
	{
		if(%client.ndModeNum)
			%client.ndMode.onSuperShiftBrick(%client, %x, %y, %z);
		else
			parent::serverCmdSuperShiftBrick(%client, %x, %y, %z);
	}

	//Rotating the ghost brick (default: numpad 79)
	function serverCmdRotateBrick(%client, %direction)
	{
		if(%client.ndModeNum)
			%client.ndMode.onRotateBrick(%client, %direction);
		else
			parent::serverCmdRotateBrick(%client, %direction);
	}

	//Planting the ghost brick (default: numpad enter)
	function serverCmdPlantBrick(%client)
	{
		if(%client.ndModeNum)
			%client.ndMode.onPlantBrick(%client);
		else
			parent::serverCmdPlantBrick(%client);
	}

	//Removing the ghost brick (default: numpad 0)
	function serverCmdCancelBrick(%client)
	{
		if(%client.ndModeNum)
			%client.ndMode.onCancelBrick(%client);
		else
			parent::serverCmdCancelBrick(%client);
	}
};



//This should go somewhere else later

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
